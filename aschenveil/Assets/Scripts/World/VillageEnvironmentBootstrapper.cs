using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ashenveil.World
{
    /// <summary>
    /// Builds the Grauweiler village layout, props and palisade on top of the forest terrain.
    /// Referenced GDD section: 3.3
    /// </summary>
    public class VillageEnvironmentBootstrapper : MonoBehaviour
    {
        private const string GENERATED_CONTENT_ROOT_NAME = "Generated Village Content";

        [Header("References")]
        [SerializeField] private VillageEnvironmentProfile _profile;
        [SerializeField] private Terrain _terrain;
        [SerializeField] private Transform _contentRoot;

        [Header("Build")]
        [SerializeField] private bool _buildOnAwake;

        private int _residencePrefabIndex;
        private int _ambientPropIndex;

        private void Awake()
        {
            if (_buildOnAwake)
            {
                BuildVillage();
            }
        }

        public void BuildVillage()
        {
            if (_profile == null)
            {
                Debug.LogError($"{nameof(VillageEnvironmentBootstrapper)} on {name} requires a {nameof(VillageEnvironmentProfile)}.");
                return;
            }

            EnsureContentRoot();
            ClearChildren(_contentRoot);

            Vector3 villageCenter = transform.position + _profile.VillageCenter;
            FlattenVillageTerrain(villageCenter);

            _residencePrefabIndex = 0;
            _ambientPropIndex = 0;

            IReadOnlyList<VillageLayoutMath.BuildingPlacement> buildingPlan = VillageLayoutMath.CreateDefaultBuildingPlan(
                _profile.CentralSquareSize,
                _profile.VillageFootprint);

            BuildBuildings(villageCenter, buildingPlan);
            BuildPalisade(villageCenter);
            BuildSquare(villageCenter);
            BuildPaths(villageCenter, buildingPlan);
            BuildAmbientProps(villageCenter);
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

        private void FlattenVillageTerrain(Vector3 villageCenter)
        {
            if (_terrain == null || _terrain.terrainData == null)
            {
                return;
            }

            TerrainData terrainData = _terrain.terrainData;
            int resolution = terrainData.heightmapResolution;
            float[,] heights = terrainData.GetHeights(0, 0, resolution, resolution);

            Vector3 terrainPosition = _terrain.transform.position;
            float targetWorldHeight = _terrain.SampleHeight(villageCenter) + terrainPosition.y;
            float normalizedTargetHeight = Mathf.Clamp01((targetWorldHeight - terrainPosition.y) / terrainData.size.y);
            float halfWidth = _profile.VillageFootprint.x * 0.5f;
            float halfLength = _profile.VillageFootprint.y * 0.5f;
            float blendDistance = Mathf.Max(1f, _profile.TerrainBlendDistance);

            for (int z = 0; z < resolution; z++)
            {
                float normalizedZ = (float)z / (resolution - 1);
                float worldZ = terrainPosition.z + normalizedZ * terrainData.size.z;
                for (int x = 0; x < resolution; x++)
                {
                    float normalizedX = (float)x / (resolution - 1);
                    float worldX = terrainPosition.x + normalizedX * terrainData.size.x;

                    float dx = Mathf.Abs(worldX - villageCenter.x);
                    float dz = Mathf.Abs(worldZ - villageCenter.z);
                    float blendFactor = Mathf.Max(
                        Mathf.InverseLerp(halfWidth + blendDistance, halfWidth, dx),
                        Mathf.InverseLerp(halfLength + blendDistance, halfLength, dz));

                    if (blendFactor <= 0f)
                    {
                        continue;
                    }

                    heights[z, x] = Mathf.Lerp(heights[z, x], normalizedTargetHeight, blendFactor);
                }
            }

            terrainData.SetHeights(0, 0, heights);
        }

        private void BuildBuildings(Vector3 villageCenter, IReadOnlyList<VillageLayoutMath.BuildingPlacement> buildingPlan)
        {
            foreach (VillageLayoutMath.BuildingPlacement placement in buildingPlan)
            {
                GameObject prefab = ResolveBuildingPrefab(placement.Kind);
                if (prefab == null)
                {
                    continue;
                }

                Vector3 worldPosition = villageCenter + placement.LocalPosition;
                worldPosition.y = ResolveTerrainHeight(worldPosition);

                GameObject instance = CreatePrefabInstance(prefab);
                instance.name = placement.Kind == VillageLayoutMath.VillageBuildingKind.Residence
                    ? $"Residence_{_residencePrefabIndex:00}"
                    : placement.Kind.ToString();
                instance.transform.SetParent(_contentRoot, false);
                instance.transform.position = worldPosition;
                instance.transform.rotation = Quaternion.Euler(0f, placement.YawDegrees, 0f);
            }
        }

        private void BuildPalisade(Vector3 villageCenter)
        {
            IReadOnlyList<VillageLayoutMath.StructurePlacement> wallPlan = VillageLayoutMath.CreatePalisadePlan(
                _profile.VillageFootprint,
                _profile.GateWidth,
                _profile.WallSegmentLength);

            foreach (VillageLayoutMath.StructurePlacement placement in wallPlan)
            {
                GameObject prefab = ResolveStructurePrefab(placement);
                if (prefab == null)
                {
                    continue;
                }

                Vector3 worldPosition = villageCenter + placement.LocalPosition;
                worldPosition.y = ResolveTerrainHeight(worldPosition);

                GameObject instance = CreatePrefabInstance(prefab);
                instance.name = placement.IsGate ? "VillageGate" : placement.IsCorner ? "PalisadeCorner" : "PalisadeWall";
                instance.transform.SetParent(_contentRoot, false);
                instance.transform.position = worldPosition;
                instance.transform.rotation = Quaternion.Euler(0f, placement.YawDegrees, 0f);
            }
        }

        private void BuildSquare(Vector3 villageCenter)
        {
            GameObject squareRoot = new GameObject("VillageSquare");
            squareRoot.transform.SetParent(_contentRoot, false);
            squareRoot.transform.position = villageCenter;

            CreateWell(squareRoot.transform, villageCenter);
        }

        private void BuildPaths(Vector3 villageCenter, IReadOnlyList<VillageLayoutMath.BuildingPlacement> buildingPlan)
        {
            if (_profile.PathPrefab == null)
            {
                return;
            }

            foreach (VillageLayoutMath.BuildingPlacement placement in buildingPlan)
            {
                if (placement.Kind == VillageLayoutMath.VillageBuildingKind.WatchTower)
                {
                    continue;
                }

                CreatePathStrip(villageCenter, villageCenter + placement.LocalPosition);
            }

            float villageSouthEdge = _profile.VillageFootprint.y * 0.5f;
            CreatePathStrip(villageCenter, villageCenter + new Vector3(0f, 0f, -villageSouthEdge - 10f));
        }

        private void BuildAmbientProps(Vector3 villageCenter)
        {
            if (_profile.AmbientProps == null || _profile.AmbientProps.Length == 0)
            {
                return;
            }

            Vector3[] offsets =
            {
                new Vector3(10f, 0f, 6f),
                new Vector3(-9f, 0f, 7f),
                new Vector3(12f, 0f, -8f),
                new Vector3(-11f, 0f, -9f),
                new Vector3(36f, 0f, 16f),
                new Vector3(-34f, 0f, 18f)
            };

            foreach (Vector3 offset in offsets)
            {
                GameObject prefab = _profile.AmbientProps[_ambientPropIndex % _profile.AmbientProps.Length];
                _ambientPropIndex++;
                if (prefab == null)
                {
                    continue;
                }

                Vector3 worldPosition = villageCenter + offset;
                worldPosition.y = ResolveTerrainHeight(worldPosition);

                GameObject instance = CreatePrefabInstance(prefab);
                instance.name = $"VillageProp_{_ambientPropIndex:00}";
                instance.transform.SetParent(_contentRoot, false);
                instance.transform.position = worldPosition;
                instance.transform.rotation = Quaternion.Euler(0f, (_profile.Seed + _ambientPropIndex * 31) % 360, 0f);
            }
        }

        private void CreatePathStrip(Vector3 origin, Vector3 destination)
        {
            Vector3 delta = destination - origin;
            delta.y = 0f;
            float pathLength = delta.magnitude;
            if (pathLength < 1f)
            {
                return;
            }

            Vector3 direction = delta / pathLength;
            int segmentCount = Mathf.Max(1, Mathf.CeilToInt(pathLength / 4f));
            float segmentSpacing = pathLength / segmentCount;

            for (int i = 1; i <= segmentCount; i++)
            {
                Vector3 position = origin + direction * (segmentSpacing * i);
                position.y = ResolveTerrainHeight(position) + 0.02f;

                GameObject instance = CreatePrefabInstance(_profile.PathPrefab);
                instance.name = $"VillagePath_{i:00}";
                instance.transform.SetParent(_contentRoot, false);
                instance.transform.position = position;
                instance.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }

        private void CreateWell(Transform parent, Vector3 villageCenter)
        {
            float baseHeight = ResolveTerrainHeight(villageCenter);

            GameObject baseCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseCylinder.name = "WellBase";
            baseCylinder.transform.SetParent(parent, false);
            baseCylinder.transform.position = new Vector3(villageCenter.x, baseHeight + 0.6f, villageCenter.z);
            baseCylinder.transform.localScale = new Vector3(2.6f, 0.6f, 2.6f);

            GameObject innerCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            innerCylinder.name = "WellWater";
            innerCylinder.transform.SetParent(parent, false);
            innerCylinder.transform.position = new Vector3(villageCenter.x, baseHeight + 0.25f, villageCenter.z);
            innerCylinder.transform.localScale = new Vector3(1.9f, 0.15f, 1.9f);

            GameObject supportA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            supportA.name = "WellSupportA";
            supportA.transform.SetParent(parent, false);
            supportA.transform.position = new Vector3(villageCenter.x - 1.3f, baseHeight + 2.2f, villageCenter.z);
            supportA.transform.localScale = new Vector3(0.2f, 2.2f, 0.2f);

            GameObject supportB = GameObject.CreatePrimitive(PrimitiveType.Cube);
            supportB.name = "WellSupportB";
            supportB.transform.SetParent(parent, false);
            supportB.transform.position = new Vector3(villageCenter.x + 1.3f, baseHeight + 2.2f, villageCenter.z);
            supportB.transform.localScale = new Vector3(0.2f, 2.2f, 0.2f);

            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "WellBeam";
            beam.transform.SetParent(parent, false);
            beam.transform.position = new Vector3(villageCenter.x, baseHeight + 3.15f, villageCenter.z);
            beam.transform.localScale = new Vector3(2.9f, 0.18f, 0.18f);
        }

        private GameObject ResolveBuildingPrefab(VillageLayoutMath.VillageBuildingKind kind)
        {
            switch (kind)
            {
                case VillageLayoutMath.VillageBuildingKind.ElderHouse:
                    return _profile.ElderHousePrefab;
                case VillageLayoutMath.VillageBuildingKind.TraderShop:
                    return _profile.TraderShopPrefab;
                case VillageLayoutMath.VillageBuildingKind.Blacksmith:
                    return _profile.BlacksmithPrefab;
                case VillageLayoutMath.VillageBuildingKind.HealerHut:
                    return _profile.HealerHutPrefab;
                case VillageLayoutMath.VillageBuildingKind.Tavern:
                    return _profile.TavernPrefab;
                case VillageLayoutMath.VillageBuildingKind.Residence:
                {
                    GameObject[] residences = _profile.ResidencePrefabs;
                    if (residences == null || residences.Length == 0)
                    {
                        return null;
                    }

                    GameObject prefab = residences[_residencePrefabIndex % residences.Length];
                    _residencePrefabIndex++;
                    return prefab;
                }
                case VillageLayoutMath.VillageBuildingKind.StorageBarn:
                    return _profile.StorageBarnPrefab;
                case VillageLayoutMath.VillageBuildingKind.WatchTower:
                    return _profile.WatchTowerPrefab;
                case VillageLayoutMath.VillageBuildingKind.Stable:
                    return _profile.StablePrefab;
                default:
                    return null;
            }
        }

        private GameObject ResolveStructurePrefab(VillageLayoutMath.StructurePlacement placement)
        {
            if (placement.IsGate)
            {
                return _profile.GatePrefab != null ? _profile.GatePrefab : _profile.WallPanelPrefab;
            }

            if (placement.IsCorner)
            {
                return _profile.WallCornerPrefab != null ? _profile.WallCornerPrefab : _profile.WallPanelPrefab;
            }

            return _profile.WallPanelPrefab;
        }

        private float ResolveTerrainHeight(Vector3 worldPosition)
        {
            if (_terrain == null || _terrain.terrainData == null)
            {
                return worldPosition.y;
            }

            return _terrain.SampleHeight(worldPosition) + _terrain.transform.position.y;
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
