using UnityEngine;

namespace Ashenveil.World
{
    /// <summary>
    /// Pure validation helpers for the terrain and forest configuration.
    /// Referenced GDD section: 3.2
    /// </summary>
    public static class TerrainEnvironmentValidation
    {
        public static bool MatchesGddTerrainTarget(TerrainEnvironmentProfile profile)
        {
            if (profile == null)
            {
                return false;
            }

            Vector3 terrainSize = profile.TerrainSize;
            return Mathf.Approximately(terrainSize.x, 500f)
                && Mathf.Approximately(terrainSize.y, 100f)
                && Mathf.Approximately(terrainSize.z, 500f)
                && profile.HeightmapResolution == 513;
        }

        public static bool HasMinimumLayerCount(TerrainEnvironmentProfile profile, int minimumLayerCount)
        {
            if (profile == null || minimumLayerCount <= 0)
            {
                return false;
            }

            TerrainLayer[] terrainLayers = profile.TerrainLayers;
            return terrainLayers != null && terrainLayers.Length >= minimumLayerCount;
        }

        public static bool HasPlayableForestDensity(TerrainEnvironmentProfile profile)
        {
            if (profile == null)
            {
                return false;
            }

            return profile.DetailDensity >= 0.8f
                && profile.DetailDistance >= 80f
                && profile.TreeCount > 0
                && profile.TreeRadius > 0f
                && profile.RockRadius > 0f;
        }
    }
}
