using System.Collections.Generic;
using Ashenveil.Dialog;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Ashenveil.UI
{
    /// <summary>
    /// Renders a <see cref="DialogRunnerModel"/>: speaker name, node text, and numbered
    /// choice buttons (also selectable with number keys 1–4). Referenced GDD section:
    /// Demo-Ablauf Phase 4.
    /// </summary>
    public sealed class DialogScreenController : MonoBehaviour
    {
        private GameObject _root;
        private TextMeshProUGUI _speakerLabel;
        private TextMeshProUGUI _textLabel;
        private RectTransform _choicesRoot;
        private readonly List<Button> _choiceButtons = new List<Button>();
        private DialogRunnerModel _model;

        private void Awake()
        {
            BuildUi();
            SetVisible(false);
        }

        /// <summary>
        /// Whether a conversation is currently displayed.
        /// </summary>
        public bool IsOpen => _root != null && _root.activeSelf;

        /// <summary>
        /// Opens the screen bound to a dialog runner.
        /// </summary>
        public void Open(DialogRunnerModel model)
        {
            _model = model;
            SetVisible(true);
            Refresh();
        }

        /// <summary>
        /// Closes the dialog screen.
        /// </summary>
        public void Close()
        {
            _model = null;
            SetVisible(false);
        }

        private void Update()
        {
            if (!IsOpen || _model == null)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame) { ChooseByIndex(0); }
            else if (keyboard.digit2Key.wasPressedThisFrame) { ChooseByIndex(1); }
            else if (keyboard.digit3Key.wasPressedThisFrame) { ChooseByIndex(2); }
            else if (keyboard.digit4Key.wasPressedThisFrame) { ChooseByIndex(3); }
        }

        private void ChooseByIndex(int index)
        {
            if (_model == null || index >= _model.AvailableChoices.Count)
            {
                return;
            }

            _model.Choose(index);
            if (_model.IsFinished)
            {
                Close();
            }
            else
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            DialogNode node = _model.CurrentNode;
            if (node == null)
            {
                Close();
                return;
            }

            _speakerLabel.text = node.SpeakerName;
            _textLabel.text = node.Text;

            IReadOnlyList<DialogChoice> choices = _model.AvailableChoices;
            for (int i = 0; i < _choiceButtons.Count; i++)
            {
                bool active = i < choices.Count;
                _choiceButtons[i].gameObject.SetActive(active);
                if (active)
                {
                    var label = _choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    label.text = $"{i + 1}. {choices[i].Text}";
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
            Canvas canvas = UIFactory.CreateCanvas("Dialog_Canvas", sortOrder: 40);
            canvas.transform.SetParent(transform, false);

            Image panel = UIFactory.CreatePanel("DialogPanel", canvas.transform, UITheme.PanelBackground);
            UIFactory.Place(panel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(1200f, 360f));
            _root = panel.gameObject;

            _speakerLabel = UIFactory.CreateLabel(
                "Speaker", panel.transform, string.Empty, UITheme.FontSizeHeading, UITheme.AetherFill, TextAlignmentOptions.Left);
            UIFactory.Place(_speakerLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -20f), new Vector2(1120f, 40f));

            _textLabel = UIFactory.CreateLabel(
                "Text", panel.transform, string.Empty, UITheme.FontSizeBody, UITheme.TextPrimary, TextAlignmentOptions.TopLeft);
            UIFactory.Place(_textLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -70f), new Vector2(1120f, 120f));

            RectTransform choices = UIFactory.CreateRect("Choices", panel.transform);
            UIFactory.Place(choices, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(1120f, 170f));
            _choicesRoot = choices;

            for (int i = 0; i < 4; i++)
            {
                int index = i;
                Button button = UIFactory.CreateButton("Choice" + i, choices, string.Empty, () => ChooseByIndex(index));
                UIFactory.Place(button.image.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -i * 42f), new Vector2(1080f, 38f));
                _choiceButtons.Add(button);
            }
        }
    }
}
