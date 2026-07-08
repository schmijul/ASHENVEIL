using UnityEngine;

namespace Ashenveil.UI
{
    /// <summary>
    /// Central styling constants for the demo UI, giving every screen a consistent
    /// dark-gothic look. Referenced GDD section: Kernsysteme / UI.
    /// </summary>
    public static class UITheme
    {
        /// <summary>Reference resolution the CanvasScaler targets.</summary>
        public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

        /// <summary>Panel background (near-black, high opacity).</summary>
        public static readonly Color PanelBackground = new Color(0.06f, 0.06f, 0.08f, 0.92f);

        /// <summary>Subtle panel border / divider.</summary>
        public static readonly Color PanelBorder = new Color(0.4f, 0.36f, 0.28f, 1f);

        /// <summary>Primary readable text (parchment).</summary>
        public static readonly Color TextPrimary = new Color(0.90f, 0.86f, 0.76f, 1f);

        /// <summary>Muted secondary text.</summary>
        public static readonly Color TextMuted = new Color(0.62f, 0.58f, 0.50f, 1f);

        /// <summary>Warning / failure text (blood red).</summary>
        public static readonly Color TextWarning = new Color(0.78f, 0.20f, 0.16f, 1f);

        /// <summary>Health bar fill.</summary>
        public static readonly Color HealthFill = new Color(0.68f, 0.14f, 0.12f, 1f);

        /// <summary>Stamina bar fill.</summary>
        public static readonly Color StaminaFill = new Color(0.42f, 0.52f, 0.20f, 1f);

        /// <summary>Aether charge bar fill (cyan glow).</summary>
        public static readonly Color AetherFill = new Color(0.24f, 0.72f, 0.82f, 1f);

        /// <summary>Corruption meter fill (creeping purple).</summary>
        public static readonly Color CorruptionFill = new Color(0.42f, 0.16f, 0.52f, 1f);

        /// <summary>Bar background track.</summary>
        public static readonly Color BarTrack = new Color(0.10f, 0.10f, 0.12f, 0.85f);

        /// <summary>Button idle color.</summary>
        public static readonly Color ButtonNormal = new Color(0.14f, 0.13f, 0.16f, 0.95f);

        /// <summary>Button hover color.</summary>
        public static readonly Color ButtonHover = new Color(0.24f, 0.22f, 0.26f, 1f);

        /// <summary>Title font size.</summary>
        public const float FontSizeTitle = 46f;

        /// <summary>Heading font size.</summary>
        public const float FontSizeHeading = 30f;

        /// <summary>Body font size.</summary>
        public const float FontSizeBody = 22f;

        /// <summary>Small/caption font size.</summary>
        public const float FontSizeSmall = 18f;
    }
}
