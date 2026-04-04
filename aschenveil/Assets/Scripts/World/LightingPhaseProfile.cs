using System;
using UnityEngine;

namespace Ashenveil.World
{
    /// <summary>
    /// Lighting phase data used to cross-fade the demo's world mood.
    /// Referenced GDD section: 3.4
    /// </summary>
    [CreateAssetMenu(fileName = "LightingPhaseProfile", menuName = "Ashenveil/World/Lighting Phase Profile")]
    public class LightingPhaseProfile : ScriptableObject
    {
        [Serializable]
        public struct LightingPhaseDefinition
        {
            public string phaseName;
            public bool directionalLightEnabled;
            public Color directionalColor;
            [Range(0f, 5f)] public float directionalIntensity;
            public float directionalAngle;
            public Color ambientColor;
            public Color fogColor;
            [Min(0.01f)] public float transitionDuration;
        }

        [Header("Phases")]
        [SerializeField] private LightingPhaseDefinition[] _phases = new LightingPhaseDefinition[4];

        public LightingPhaseDefinition[] Phases => _phases;
        public int PhaseCount => _phases != null ? _phases.Length : 0;

        public static LightingPhaseProfile CreateRuntimeDefaults()
        {
            LightingPhaseProfile profile = CreateInstance<LightingPhaseProfile>();
            profile._phases = new[]
            {
                new LightingPhaseDefinition
                {
                    phaseName = "Hunt",
                    directionalLightEnabled = true,
                    directionalColor = HexColor("#FFE4B5"),
                    directionalIntensity = 1.2f,
                    directionalAngle = 15f,
                    ambientColor = new Color(0.83f, 0.72f, 0.55f, 1f),
                    fogColor = new Color(0.72f, 0.79f, 0.68f, 1f),
                    transitionDuration = 2f
                },
                new LightingPhaseDefinition
                {
                    phaseName = "Village",
                    directionalLightEnabled = true,
                    directionalColor = HexColor("#FFFFF0"),
                    directionalIntensity = 1.5f,
                    directionalAngle = 60f,
                    ambientColor = new Color(0.68f, 0.78f, 1f, 1f),
                    fogColor = new Color(0.78f, 0.84f, 0.93f, 1f),
                    transitionDuration = 2f
                },
                new LightingPhaseDefinition
                {
                    phaseName = "Aether",
                    directionalLightEnabled = true,
                    directionalColor = HexColor("#B0C4DE"),
                    directionalIntensity = 0.8f,
                    directionalAngle = 10f,
                    ambientColor = new Color(0.32f, 0.27f, 0.46f, 1f),
                    fogColor = new Color(0.34f, 0.31f, 0.51f, 1f),
                    transitionDuration = 2f
                },
                new LightingPhaseDefinition
                {
                    phaseName = "Destruction",
                    directionalLightEnabled = false,
                    directionalColor = Color.black,
                    directionalIntensity = 0f,
                    directionalAngle = 0f,
                    ambientColor = new Color(0.08f, 0.06f, 0.09f, 1f),
                    fogColor = new Color(0.09f, 0.08f, 0.1f, 1f),
                    transitionDuration = 2f
                }
            };

            return profile;
        }

        public bool HasMinimumPhaseCount(int minimumCount)
        {
            return _phases != null && minimumCount > 0 && _phases.Length >= minimumCount;
        }

        public LightingPhaseDefinition GetPhase(int index)
        {
            if (_phases == null || _phases.Length == 0)
            {
                return default;
            }

            int clampedIndex = Mathf.Clamp(index, 0, _phases.Length - 1);
            return _phases[clampedIndex];
        }

        private static Color HexColor(string htmlColor)
        {
            ColorUtility.TryParseHtmlString(htmlColor, out Color color);
            color.a = 1f;
            return color;
        }
    }
}
