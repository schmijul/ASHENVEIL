using UnityEngine;

namespace Ashenveil.World
{
    /// <summary>
    /// Pure math helpers for lighting phase blending.
    /// Referenced GDD section: 3.4
    /// </summary>
    public static class LightingPhaseMath
    {
        public static LightingPhaseProfile.LightingPhaseDefinition Lerp(
            LightingPhaseProfile.LightingPhaseDefinition from,
            LightingPhaseProfile.LightingPhaseDefinition to,
            float t)
        {
            float clampedT = Mathf.Clamp01(t);
            return new LightingPhaseProfile.LightingPhaseDefinition
            {
                phaseName = clampedT < 0.5f ? from.phaseName : to.phaseName,
                directionalLightEnabled = clampedT < 0.5f ? from.directionalLightEnabled : to.directionalLightEnabled,
                directionalColor = Color.Lerp(from.directionalColor, to.directionalColor, clampedT),
                directionalIntensity = Mathf.Lerp(from.directionalIntensity, to.directionalIntensity, clampedT),
                directionalAngle = Mathf.Lerp(from.directionalAngle, to.directionalAngle, clampedT),
                ambientColor = Color.Lerp(from.ambientColor, to.ambientColor, clampedT),
                fogColor = Color.Lerp(from.fogColor, to.fogColor, clampedT),
                transitionDuration = Mathf.Lerp(from.transitionDuration, to.transitionDuration, clampedT)
            };
        }
    }
}
