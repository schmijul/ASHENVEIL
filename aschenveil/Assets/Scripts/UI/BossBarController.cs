using Ashenveil.AI.Boss;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ashenveil.UI
{
    /// <summary>
    /// Top-center boss health bar, hidden until the arena trigger activates the boss.
    /// Referenced GDD section: Demo-Ablauf Phase 6.
    /// </summary>
    public sealed class BossBarController : MonoBehaviour
    {
        [SerializeField] private MutatedWolfBossController _boss;
        [SerializeField] private BossArenaTrigger _arenaTrigger;

        private GameObject _root;
        private TextMeshProUGUI _nameLabel;
        private Image _healthFill;

        private void Awake()
        {
            BuildUi();
            SetVisible(false);
        }

        private void OnEnable()
        {
            if (_boss != null)
            {
                _boss.Activated += OnActivated;
                _boss.HealthChanged += OnHealthChanged;
            }

            if (_arenaTrigger != null)
            {
                _arenaTrigger.BossEncounterStarted += OnEncounterStarted;
            }
        }

        private void OnDisable()
        {
            if (_boss != null)
            {
                _boss.Activated -= OnActivated;
                _boss.HealthChanged -= OnHealthChanged;
            }

            if (_arenaTrigger != null)
            {
                _arenaTrigger.BossEncounterStarted -= OnEncounterStarted;
            }
        }

        /// <summary>
        /// Binds the bar to a boss controller at runtime (used by the scene builder).
        /// </summary>
        public void Bind(MutatedWolfBossController boss)
        {
            _boss = boss;
            if (_boss != null)
            {
                _boss.Activated += OnActivated;
                _boss.HealthChanged += OnHealthChanged;
            }
        }

        private void OnEncounterStarted(MutatedWolfBossController boss)
        {
            OnActivated(boss != null ? boss.DisplayName : "Boss", 1f);
        }

        private void OnActivated(string displayName, float fraction)
        {
            _nameLabel.text = displayName;
            _healthFill.fillAmount = fraction;
            SetVisible(true);
        }

        private void OnHealthChanged(string displayName, float fraction)
        {
            _healthFill.fillAmount = fraction;
            if (fraction <= 0f)
            {
                SetVisible(false);
            }
        }

        private void SetVisible(bool visible)
        {
            if (_root != null)
            {
                _root.SetActive(visible);
            }
        }

        private void BuildUi()
        {
            Canvas canvas = UIFactory.CreateCanvas("BossBar_Canvas", sortOrder: 20);
            canvas.transform.SetParent(transform, false);

            RectTransform panel = UIFactory.CreatePanel("BossBar", canvas.transform, UITheme.PanelBackground).rectTransform;
            UIFactory.Place(panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(760f, 70f));
            _root = panel.gameObject;

            _nameLabel = UIFactory.CreateLabel(
                "BossName", panel, "Mutierter Wolf", UITheme.FontSizeHeading, UITheme.TextPrimary, TextAlignmentOptions.Center);
            UIFactory.Place(_nameLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(740f, 34f));

            _healthFill = UIFactory.CreateBar("BossHealth", panel, UITheme.HealthFill);
            RectTransform track = _healthFill.rectTransform.parent as RectTransform;
            UIFactory.Place(track, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(730f, 22f));
        }
    }
}
