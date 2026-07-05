using System.Collections.Generic;
using Ashenveil.Inventory;
using Ashenveil.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ashenveil.UI
{
    /// <summary>
    /// Full-screen inventory: item stacks (name, count, weight, value), gold, and total
    /// carry weight. Toggled by the player's inventory input; pauses gameplay through an
    /// <see cref="IGamePauser"/>. Referenced GDD section: Demo-Ablauf Phase 3.
    /// </summary>
    public sealed class InventoryScreenController : MonoBehaviour
    {
        [SerializeField] private PlayerMovementController _movementController;
        [SerializeField] private PlayerInventory _playerInventory;

        private GameObject _root;
        private TextMeshProUGUI _goldLabel;
        private TextMeshProUGUI _weightLabel;
        private RectTransform _listRoot;
        private readonly List<TextMeshProUGUI> _rows = new List<TextMeshProUGUI>();
        private IGamePauser _pauser;
        private InventoryModel _model;

        private void Awake()
        {
            _pauser = new TimeScalePauser();
            if (_playerInventory != null)
            {
                _model = _playerInventory.Model;
            }

            BuildUi();
            SetVisible(false);
        }

        private void OnEnable()
        {
            if (_movementController != null)
            {
                _movementController.InventoryPressed += Toggle;
            }
        }

        private void OnDisable()
        {
            if (_movementController != null)
            {
                _movementController.InventoryPressed -= Toggle;
            }
        }

        /// <summary>
        /// Overrides the pauser (used for tests / custom pause handling).
        /// </summary>
        public void SetPauser(IGamePauser pauser)
        {
            _pauser = pauser;
        }

        /// <summary>
        /// Binds the inventory model at runtime.
        /// </summary>
        public void Bind(InventoryModel model)
        {
            _model = model;
        }

        /// <summary>
        /// Whether the inventory is open.
        /// </summary>
        public bool IsOpen => _root != null && _root.activeSelf;

        private void Toggle()
        {
            if (IsOpen)
            {
                SetVisible(false);
                _pauser.Resume();
            }
            else
            {
                Refresh();
                SetVisible(true);
                _pauser.Pause();
            }
        }

        private void Refresh()
        {
            if (_model == null)
            {
                return;
            }

            _goldLabel.text = $"Gold: {_model.Gold}";
            _weightLabel.text = $"Gewicht: {_model.TotalWeight:0.0} / {_model.MaxCarryWeight:0.0}";

            IReadOnlyList<InventoryStack> stacks = _model.Stacks;
            for (int i = 0; i < _rows.Count; i++)
            {
                if (i < stacks.Count)
                {
                    InventoryStack s = stacks[i];
                    _rows[i].gameObject.SetActive(true);
                    _rows[i].text = $"{s.Item.DisplayName}   x{s.Quantity}   {s.Item.Weight * s.Quantity:0.0} kg   {s.Item.GoldValue * s.Quantity} G";
                }
                else
                {
                    _rows[i].gameObject.SetActive(false);
                }
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
            Canvas canvas = UIFactory.CreateCanvas("Inventory_Canvas", sortOrder: 50);
            canvas.transform.SetParent(transform, false);

            Image panel = UIFactory.CreatePanel("InventoryPanel", canvas.transform, UITheme.PanelBackground);
            UIFactory.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 700f));
            _root = panel.gameObject;

            TextMeshProUGUI title = UIFactory.CreateLabel(
                "Title", panel.transform, "Inventar", UITheme.FontSizeTitle, UITheme.TextPrimary, TextAlignmentOptions.Center);
            UIFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(860f, 60f));

            _goldLabel = UIFactory.CreateLabel("Gold", panel.transform, "Gold: 0", UITheme.FontSizeBody, UITheme.TextPrimary, TextAlignmentOptions.Left);
            UIFactory.Place(_goldLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -90f), new Vector2(400f, 30f));

            _weightLabel = UIFactory.CreateLabel("Weight", panel.transform, "Gewicht: 0", UITheme.FontSizeBody, UITheme.TextMuted, TextAlignmentOptions.Right);
            UIFactory.Place(_weightLabel.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -90f), new Vector2(400f, 30f));

            RectTransform list = UIFactory.CreateRect("List", panel.transform);
            UIFactory.Place(list, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -140f), new Vector2(820f, 520f));
            _listRoot = list;

            for (int i = 0; i < 16; i++)
            {
                TextMeshProUGUI row = UIFactory.CreateLabel("Row" + i, list, string.Empty, UITheme.FontSizeBody, UITheme.TextPrimary, TextAlignmentOptions.Left);
                UIFactory.Place(row.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -i * 32f), new Vector2(800f, 30f));
                _rows.Add(row);
            }
        }
    }
}
