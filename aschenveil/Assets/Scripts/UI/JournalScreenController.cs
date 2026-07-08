using System.Collections.Generic;
using Ashenveil.Quests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ashenveil.UI
{
    /// <summary>
    /// Quest journal: lists active and completed quests with per-objective progress.
    /// Referenced GDD section: Demo-Ablauf Phase 4.
    /// </summary>
    public sealed class JournalScreenController : MonoBehaviour
    {
        private GameObject _root;
        private TextMeshProUGUI _body;
        private QuestLogModel _log;
        private IReadOnlyList<QuestDefinition> _quests;

        private void Awake()
        {
            BuildUi();
            SetVisible(false);
        }

        /// <summary>
        /// Whether the journal is open.
        /// </summary>
        public bool IsOpen => _root != null && _root.activeSelf;

        /// <summary>
        /// Binds the quest log and the full quest catalogue to render.
        /// </summary>
        public void Bind(QuestLogModel log, IReadOnlyList<QuestDefinition> quests)
        {
            _log = log;
            _quests = quests;
        }

        /// <summary>
        /// Toggles the journal open/closed.
        /// </summary>
        public void Toggle()
        {
            if (IsOpen)
            {
                SetVisible(false);
            }
            else
            {
                Refresh();
                SetVisible(true);
            }
        }

        private void Refresh()
        {
            if (_log == null || _quests == null)
            {
                _body.text = "Keine Aufträge.";
                return;
            }

            var sb = new System.Text.StringBuilder();
            foreach (QuestDefinition quest in _quests)
            {
                QuestState state = _log.GetQuestState(quest.Id);
                if (state == QuestState.Inactive)
                {
                    continue;
                }

                sb.AppendLine($"<b>{quest.Title}</b>  ({StateName(state)})");
                sb.AppendLine(quest.Description);
                foreach (QuestObjectiveDefinition obj in quest.Objectives)
                {
                    int progress = _log.GetObjectiveProgress(quest.Id, obj.Id);
                    sb.AppendLine($"   • {obj.Text}: {progress}/{obj.RequiredCount}");
                }

                sb.AppendLine();
            }

            _body.text = sb.Length == 0 ? "Keine aktiven Aufträge." : sb.ToString();
        }

        private static string StateName(QuestState state)
        {
            switch (state)
            {
                case QuestState.Active: return "aktiv";
                case QuestState.ObjectivesComplete: return "bereit zur Abgabe";
                case QuestState.Completed: return "abgeschlossen";
                default: return "inaktiv";
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
            Canvas canvas = UIFactory.CreateCanvas("Journal_Canvas", sortOrder: 52);
            canvas.transform.SetParent(transform, false);

            Image panel = UIFactory.CreatePanel("JournalPanel", canvas.transform, UITheme.PanelBackground);
            UIFactory.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 700f));
            _root = panel.gameObject;

            TextMeshProUGUI title = UIFactory.CreateLabel("Title", panel.transform, "Journal", UITheme.FontSizeTitle, UITheme.TextPrimary, TextAlignmentOptions.Center);
            UIFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(860f, 60f));

            _body = UIFactory.CreateLabel("Body", panel.transform, string.Empty, UITheme.FontSizeBody, UITheme.TextPrimary, TextAlignmentOptions.TopLeft);
            UIFactory.Place(_body.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(840f, 560f));
            _body.richText = true;
        }
    }
}
