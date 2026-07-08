using System;
using Ashenveil.Flow;
using TMPro;
using UnityEngine;

namespace Ashenveil.UI
{
    /// <summary>
    /// The demo's finale: "Demo Ende" with three escape-direction buttons
    /// (Kernwall / Flimmermoor / Hohensang). Choosing one invokes the bound callback
    /// and shows a German thanks message. Referenced GDD section: Demo-Ablauf Phase 8.
    /// </summary>
    public sealed class EndScreenController : MonoBehaviour
    {
        private GameObject _root;
        private GameObject _choices;
        private TextMeshProUGUI _thanksLabel;
        private Action<EscapeDirection> _onChosen;

        private void Awake()
        {
            BuildUi();
            SetVisible(false);
        }

        /// <summary>
        /// Binds the direction-choice callback (typically DemoFlowModel.ChooseEscape).
        /// </summary>
        public void Bind(Action<EscapeDirection> onChosen)
        {
            _onChosen = onChosen;
        }

        /// <summary>
        /// Shows the end screen with the direction choices.
        /// </summary>
        public void Show()
        {
            SetVisible(true);
            _choices.SetActive(true);
            _thanksLabel.gameObject.SetActive(false);
        }

        private void Choose(EscapeDirection direction)
        {
            _onChosen?.Invoke(direction);
            _choices.SetActive(false);
            _thanksLabel.gameObject.SetActive(true);
            _thanksLabel.text = $"Du ziehst Richtung {DirectionName(direction)}.\n\nDanke fürs Spielen — Demo Ende.";
        }

        private static string DirectionName(EscapeDirection direction)
        {
            switch (direction)
            {
                case EscapeDirection.Kernwall: return "Kernwall";
                case EscapeDirection.Flimmermoor: return "Flimmermoor";
                case EscapeDirection.Hohensang: return "Hohensang";
                default: return direction.ToString();
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
            Canvas canvas = UIFactory.CreateCanvas("End_Canvas", sortOrder: 95);
            canvas.transform.SetParent(transform, false);

            var bg = UIFactory.CreatePanel("EndBackground", canvas.transform, new Color(0f, 0f, 0f, 0.96f));
            UIFactory.Stretch(bg.rectTransform);
            _root = bg.gameObject;

            TextMeshProUGUI title = UIFactory.CreateLabel(
                "Title", bg.transform, "Demo Ende", UITheme.FontSizeTitle, UITheme.TextPrimary, TextAlignmentOptions.Center);
            UIFactory.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 240f), new Vector2(900f, 70f));

            TextMeshProUGUI prompt = UIFactory.CreateLabel(
                "Prompt", bg.transform, "Das Dorf brennt. Wohin fliehst du?", UITheme.FontSizeHeading, UITheme.TextMuted, TextAlignmentOptions.Center);
            UIFactory.Place(prompt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(900f, 50f));

            RectTransform choices = UIFactory.CreateRect("Choices", bg.transform);
            UIFactory.Place(choices, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(960f, 90f));
            _choices = choices.gameObject;

            MakeChoice(choices, "Kernwall", EscapeDirection.Kernwall, -320f);
            MakeChoice(choices, "Flimmermoor", EscapeDirection.Flimmermoor, 0f);
            MakeChoice(choices, "Hohensang", EscapeDirection.Hohensang, 320f);

            _thanksLabel = UIFactory.CreateLabel(
                "Thanks", bg.transform, string.Empty, UITheme.FontSizeHeading, UITheme.TextPrimary, TextAlignmentOptions.Center);
            UIFactory.Place(_thanksLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(1000f, 200f));
        }

        private void MakeChoice(Transform parent, string label, EscapeDirection direction, float x)
        {
            var button = UIFactory.CreateButton("Choice_" + label, parent, label, () => Choose(direction));
            UIFactory.Place(button.image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 0f), new Vector2(280f, 80f));
        }
    }
}
