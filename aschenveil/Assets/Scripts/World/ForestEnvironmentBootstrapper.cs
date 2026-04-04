using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ashenveil.World
{
    /// <summary>
    /// Builds the demo's terrain, props and lighting references from profiles.
    /// Referenced GDD sections: 3.2 and 3.4
    /// </summary>
    public class ForestEnvironmentBootstrapper : MonoBehaviour
    {
        private const string GENERATED_TERRAIN_NAME = "Grauwald Terrain";
        private const string GENERATED_CONTENT_ROOT_NAME = "Generated Forest Content";

        [Header("References")]
        [SerializeField] private TerrainEnvironmentProfile _terrainProfile;
        [SerializeField] private LightingPhaseProfile _lightingProfile;
        [SerializeField] private LightingPhaseManager _lightingManager;
        [SerializeField] private Transform _contentRoot;
        [SerializeField] private TerrainData _terrainData;

        [Header("Build")]
        [SerializeField] private bool _buildOnAwake = true;

        private Terrain _terrain;
        private TerrainCollider _terrainCollider;

        public Terrain Terrain => _terrain;

        private void Awake()
        {
            CacheTerrain();
            if (_buildOnAwake)
            {
                BuildEnvironment();
            }
        }

        public void BuildEnvironment()
        {
            if (_terrainProfile == null)
            {
                Debug.LogError($"{nameof(ForestEnvironmentBootstrapper)} on {name} requires a {nameof(TerrainEnvironmentProfile)}.");
                return;
            }

            CacheTerrain();
            EnsureTerrainObject();
            EnsureContentRoot();
            BuildTerrain();
            BuildPlacements();

            if (_lightingManager != null && _lightingProfile != null)
            {
                _lightingManager.ApplyPhaseInstant(0);
            }
        }

        private void CacheTerrain()
        {
            if (_terrain == null)
            {
                _terrain = GetComponentInChildren<Terrain>();
            }

            if (_terrainCollider == null && _terrain != null)
            {
                _terrainCollider = _terrain.GetComponent<TerrainCollider>();
            }
        }

        private void EnsureTerrainObject()
        {
            if (_terrain != null)
            {
                return;
            }

            TerrainData terrainData = _terrainData != null ? _terrainData : CreateTerrainData();
            ApplyTerrainProfileToData(terrainData);
            GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
            terrainObject.name = GENERATED_TERRAIN_NAME;
            terrainObject.transform.SetParent(transform, false);
            terrainObject.transform.localPosition = Vector3.zero;
            _terrain = terrainObject.GetComponent<Terrain>();
            _terrainCollider = terrainObject.GetComponent<TerrainCollider>();
            ApplyTerrainSettings(_terrain);
        }

        private void EnsureContentRoot()
        {
            if (_contentRoot != null)
            {
                return;
            }

            Transform existingRoot = transform.Find(GENERATED_CONTENT_ROOT_NAME);
            if (existingRoot != null)
            {
                _contentRoot = existingRoot;
                return;
            }

            GameObject contentRoot = new GameObject(GENERATED_CONTENT_ROOT_NAME);
            contentRoot.transform.SetParent(transform, false);
            _contentRoot = contentRoot.transform;
        }

        private void BuildTerrain()
        {
            if (_terrain == null)
            {
                return;
            }

            TerrainData terrainData = _terrain.terrainData;
            if (terrainData == null)
            {
                return;
            }

            ApplyTerrainProfileToData(terrainData);

            GenerateTerrainHeights(terrainData);
            PaintTerrainLayers(terrainData);
            ApplyTerrainSettings(_terrain);
        }

        private void BuildPlacements()
        {
            if (_contentRoot == null)
            {
                return;
            }

            ClearChildren(_contentRoot);

            Vector3 terrainCenter = transform.position + (_terrainProfile.TerrainSize * 0.5f);
            List<ForestPlacementPlanner.PlacementSample> treePlacements = ForestPlacementPlanner.GenerateClusters(
                6,
                Mathf.Max(1, _terrainProfile.TreeCount / 6),
                terrainCenter,
                _terrainProfile.TreeRadius,
                _terrainProfile.TreeRadius * 0.25f,
                _terrainProfile.Seed,
                _terrainProfile.MinTreeScale,
                _terrainProfile.MaxTreeScale,
                ResolveTerrainHeight);

            SpawnPrefabs(_terrainProfile.TreePrefabs, treePlacements, "Tree");

            List<ForestPlacementPlanner.PlacementSample> foliagePlacements = ForestPlacementPlanner.GenerateClusters(
                8,
                Mathf.Max(1, _terrainProfile.FoliageCount / 8),
                terrainCenter,
                _terrainProfile.FoliageRadius,
                _terrainProfile.FoliageRadius * 0.1f,
                _terrainProfile.Seed + 211,
                _terrainProfile.MinFoliageScale,
                _terrainProfile.MaxFoliageScale,
                ResolveTerrainHeight);

            SpawnPrefabs(_terrainProfile.FoliagePrefabs, foliagePlacements, "Foliage");

            List<ForestPlacementPlanner.PlacementSample> rockPlacements = ForestPlacementPlanner.GenerateClusters(
                3,
                Mathf.Max(1, _terrainProfile.RockCount / 3),
                terrainCenter,
                _terrainProfile.RockRadius,
                _terrainProfile.RockRadius * 0.15f,
                _terrainProfile.Seed + 997,
                _terrainProfile.MinRockScale,
                _terrainProfile.MaxRockScale,
                ResolveTerrainHeight);

            SpawnPrefabs(_terrainProfile.RockPrefabs, rockPlacements, "Rock");
        }

        private void SpawnPrefabs(GameObject[] prefabs, List<ForestPlacementPlanner.PlacementSample> placements, string category)
        {
            if (prefabs == null || prefabs.Length == 0 || placements == null)
            {
                return;
            }

            for (int i = 0; i < placements.Count; i++)
            {
                GameObject prefab = prefabs[i % prefabs.Length];
                if (prefab == null)
                {
                    continue;
                }

                ForestPlacementPlanner.PlacementSample sample = placements[i];
                GameObject instance = CreatePrefabInstance(prefab);
                instance.name = $"{category}_{i:000}";
                instance.transform.SetParent(_contentRoot, false);
                instance.transform.position = sample.Position;
                instance.transform.rotation = sample.Rotation;
                instance.transform.localScale = prefab.transform.localScale * sample.Scale;
            }
        }

        private GameObject CreatePrefabInstance(GameObject prefab)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            }
#endif
            return Instantiate(prefab);
        }

        private TerrainData CreateTerrainData()
        {
            TerrainData terrainData = new TerrainData();
            ApplyTerrainProfileToData(terrainData);
            return terrainData;
        }

        private void ApplyTerrainProfileToData(TerrainData terrainData)
        {
            if (terrainData == null || _terrainProfile == null)
            {
                return;
            }

            terrainData.heightmapResolution = _terrainProfile.HeightmapResolution;
            terrainData.alphamapResolution = _terrainProfile.AlphamapResolution;
            terrainData.baseMapResolution = _terrainProfile.AlphamapResolution;
            terrainData.SetDetailResolution(_terrainProfile.BaseDetailResolution, _terrainProfile.DetailResolutionPerPatch);
            terrainData.size = _terrainProfile.TerrainSize;
            terrainData.terrainLayers = _terrainProfile.TerrainLayers;
        }

        private void ApplyTerrainSettings(Terrain terrain)
        {
            if (terrain == null)
            {
                return;
            }

            terrain.detailObjectDensity = _terrainProfile.DetailDensity;
            terrain.detailObjectDistance = _terrainProfile.DetailDistance;
            terrain.treeDistance = Mathf.Max(_terrainProfile.TreeRadius, _terrainProfile.RockRadius) * 1.2f;
            terrain.treeBillboardDistance = terrain.treeDistance * 0.35f;
            terrain.treeCrossFadeLength = 5f;
            terrain.treeMaximumFullLODCount = 48;
            terrain.heightmapPixelError = 5f;
            terrain.basemapDistance = Mathf.Max(_terrainProfile.TerrainSize.x, _terrainProfile.TerrainSize.z) * 0.75f;
        }

        private void GenerateTerrainHeights(TerrainData terrainData)
        {
            if (terrainData == null)
            {
                return;
            }

            int resolution = terrainData.heightmapResolution;
            float[,] heights = new float[resolution, resolution];
            float seedOffset = _terrainProfile.Seed * 0.0137f;

            for (int y = 0; y < resolution; y++)
            {
                float normalizedZ = (float)y / (resolution - 1);
                for (int x = 0; x < resolution; x++)
                {
                    float normalizedX = (float)x / (resolution - 1);
                    float ridgeNoise = Mathf.PerlinNoise((normalizedX + seedOffset) * 2.4f, (normalizedZ + seedOffset) * 2.4f);
                    float detailNoise = Mathf.PerlinNoise((normalizedX + seedOffset * 1.9f) * 7.8f, (normalizedZ + seedOffset * 1.9f) * 7.8f);
                    float clearingDistance = Vector2.Distance(new Vector2(normalizedX, normalizedZ), new Vector2(0.5f, 0.5f));
                    float clearingMask = 1f - Mathf.SmoothStep(0.15f, 0.42f, clearingDistance);
                    float height = Mathf.Lerp(0.08f, 0.36f, ridgeNoise);
                    height += (detailNoise - 0.5f) * 0.06f;
                    height -= clearingMask * 0.04f;
                    heights[y, x] = Mathf.Clamp01(height);
                }
            }

            terrainData.SetHeights(0, 0, heights);
        }

        private float ResolveTerrainHeight(Vector3 worldPosition)
        {
            if (_terrain == null || _terrain.terrainData == null)
            {
                return worldPosition.y;
            }

            float normalizedX = Mathf.InverseLerp(_terrain.transform.position.x, _terrain.transform.position.x + _terrain.terrainData.size.x, worldPosition.x);
            float normalizedZ = Mathf.InverseLerp(_terrain.transform.position.z, _terrain.transform.position.z + _terrain.terrainData.size.z, worldPosition.z);
            float height = _terrain.terrainData.GetInterpolatedHeight(normalizedX, normalizedZ) + _terrain.transform.position.y;
            return height;
        }

        private void PaintTerrainLayers(TerrainData terrainData)
        {
            if (terrainData == null || terrainData.terrainLayers == null || terrainData.terrainLayers.Length == 0)
            {
                return;
            }

            int mapWidth = terrainData.alphamapWidth;
            int mapHeight = terrainData.alphamapHeight;
            int layerCount = terrainData.terrainLayers.Length;
            float[,,] alphaMap = new float[mapWidth, mapHeight, layerCount];

            for (int z = 0; z < mapHeight; z++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float normalizedX = (float)x / (mapWidth - 1);
                    float normalizedZ = (float)z / (mapHeight - 1);
                    float pathCenterDistance = Mathf.Abs(normalizedX - 0.5f);
                    float pathMask = 1f - Mathf.SmoothStep(0.08f, 0.22f, pathCenterDistance);
                    float mossNoise = Mathf.PerlinNoise(normalizedX * 4f + 12f, normalizedZ * 4f + 12f);
                    float forestFloorNoise = Mathf.PerlinNoise(normalizedX * 5f + 28f, normalizedZ * 5f + 28f);
                    float rockNoise = Mathf.PerlinNoise(normalizedX * 8f + 50f, normalizedZ * 8f + 50f);

                    float grass = Mathf.Clamp01(0.72f + mossNoise * 0.28f);
                    float dirt = Mathf.Clamp01(pathMask * 0.75f);
                    float forestFloor = Mathf.Clamp01(forestFloorNoise * 0.78f);
                    float rock = Mathf.Clamp01(rockNoise * 0.35f + Mathf.Max(0f, Mathf.Abs(normalizedZ - 0.5f) - 0.24f) * 0.35f);

                    float total = grass + dirt + forestFloor + rock;
                    alphaMap[x, z, 0] = grass / total;
                    if (layerCount > 1)
                    {
                        alphaMap[x, z, 1] = dirt / total;
                    }
                    if (layerCount > 2)
                    {
                        alphaMap[x, z, 2] = forestFloor / total;
                    }
                    if (layerCount > 3)
                    {
                        alphaMap[x, z, 3] = rock / total;
                    }
                }
            }

            terrainData.SetAlphamaps(0, 0, alphaMap);
        }

        private static void ClearChildren(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
                else
#endif
                {
                    Object.Destroy(child.gameObject);
                }
            }
        }
    }
}
