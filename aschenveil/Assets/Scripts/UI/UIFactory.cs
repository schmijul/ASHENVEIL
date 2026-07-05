using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ashenveil.UI
{
    /// <summary>
    /// Builds the demo UI programmatically (no prefabs) so the batch scene builder can
    /// assemble every screen from code. All helpers apply <see cref="UITheme"/> styling.
    /// Referenced GDD section: Kernsysteme / UI.
    /// </summary>
    public static class UIFactory
    {
        /// <summary>
        /// Creates a screen-space-overlay canvas with a scaler locked to the reference resolution.
        /// </summary>
        public static Canvas CreateCanvas(string name, int sortOrder = 0)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = UITheme.ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        /// <summary>
        /// Creates a full-rect child GameObject with a RectTransform under a parent.
        /// </summary>
        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        /// <summary>
        /// Creates a panel with a background image, stretched or sized by the caller afterward.
        /// </summary>
        public static Image CreatePanel(string name, Transform parent, Color? color = null)
        {
            var rt = CreateRect(name, parent);
            var image = rt.gameObject.AddComponent<Image>();
            image.color = color ?? UITheme.PanelBackground;
            return image;
        }

        /// <summary>
        /// Stretches a RectTransform to fill its parent with the given padding.
        /// </summary>
        public static void Stretch(RectTransform rt, float padding = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padding, padding);
            rt.offsetMax = new Vector2(-padding, -padding);
        }

        /// <summary>
        /// Anchors and sizes a RectTransform relative to a normalized anchor point.
        /// </summary>
        public static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }

        /// <summary>
        /// Creates a TextMeshPro label.
        /// </summary>
        public static TextMeshProUGUI CreateLabel(
            string name,
            Transform parent,
            string text,
            float fontSize,
            Color? color = null,
            TextAlignmentOptions alignment = TextAlignmentOptions.Left)
        {
            var rt = CreateRect(name, parent);
            var label = rt.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = color ?? UITheme.TextPrimary;
            label.alignment = alignment;
            label.enableWordWrapping = true;
            return label;
        }

        /// <summary>
        /// Creates a horizontal bar (track + fill) and returns the fill Image; set fillAmount to scale it.
        /// </summary>
        public static Image CreateBar(string name, Transform parent, Color fillColor)
        {
            Image track = CreatePanel(name, parent, UITheme.BarTrack);
            Image fill = CreatePanel(name + "_Fill", track.transform, fillColor);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 1f;
            Stretch(fill.rectTransform, 2f);
            return fill;
        }

        /// <summary>
        /// Creates a themed button with a TMP label and returns the Button.
        /// </summary>
        public static Button CreateButton(string name, Transform parent, string text, Action onClick)
        {
            Image bg = CreatePanel(name, parent, UITheme.ButtonNormal);
            var button = bg.gameObject.AddComponent<Button>();

            var colors = button.colors;
            colors.normalColor = UITheme.ButtonNormal;
            colors.highlightedColor = UITheme.ButtonHover;
            colors.pressedColor = UITheme.ButtonHover;
            colors.selectedColor = UITheme.ButtonHover;
            button.colors = colors;

            TextMeshProUGUI label = CreateLabel(
                name + "_Label", bg.transform, text, UITheme.FontSizeBody, UITheme.TextPrimary, TextAlignmentOptions.Center);
            Stretch(label.rectTransform, 6f);

            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }

            return button;
        }
    }
}
