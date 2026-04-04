using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ashenveil.World.Editor
{
    /// <summary>
    /// Creates the Grauwald terrain scene and required world profile assets.
    /// Referenced GDD sections: 3.2 and 3.4
    /// </summary>
    public static class GrauwaldSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Grauwald.unity";
        private const string TerrainDataPath = "Assets/Scenes/GrauwaldTerrainData.asset";
        private const string WorldAssetFolder = "Assets/ScriptableObjects/World";
        private const string TerrainProfilePath = WorldAssetFolder + "/GrauwaldTerrainEnvironmentProfile.asset";
        private const string LightingProfilePath = WorldAssetFolder + "/GrauwaldLightingPhaseProfile.asset";
        private const string VillageProfilePath = WorldAssetFolder + "/GrauweilerVillageEnvironmentProfile.asset";
        private const string ForestFloorLayerPath = WorldAssetFolder + "/GrauwaldForestFloor.terrainlayer";

        private const string MossLayerPath = "Assets/Supercyan Free Forest Sample/TerrainLayers/forestpack_moss_light_terrainlayer.terrainlayer";
        private const string RoadLayerPath = "Assets/Supercyan Free Forest Sample/TerrainLayers/forestpack_road_terrailayer.terrainlayer";
        private const string RockLayerPath = "Assets/Supercyan Free Forest Sample/TerrainLayers/forestpack_rock_terrainlayer.terrainlayer";

        [MenuItem("Ashenveil/World/Build Grauwald Scene")]
        public static void BuildGrauwaldSceneMenu()
        {
            BuildGrauwaldScene();
        }

        public static void BuildGrauwaldScene()
        {
            EnsureFolder("Assets/ScriptableObjects");
            EnsureFolder(WorldAssetFolder);
            EnsureFolder("Assets/Scenes");

            TerrainEnvironmentProfile terrainProfile = LoadOrCreateTerrainProfile();
            LightingPhaseProfile lightingProfile = LoadOrCreateLightingProfile();
            VillageEnvironmentProfile villageProfile = LoadOrCreateVillageProfile();
            TerrainData terrainData = LoadOrCreateTerrainData();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Grauwald";

            GameObject root = new GameObject("Grauwald");
            GameObject lightingRoot = new GameObject("World Lighting");
            lightingRoot.transform.SetParent(root.transform, false);

            GameObject directionalLightObject = new GameObject("Directional Light");
            directionalLightObject.transform.SetParent(lightingRoot.transform, false);
            Light directionalLight = directionalLightObject.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.shadows = LightShadows.Soft;

            GameObject environmentRoot = new GameObject("Environment");
            environmentRoot.transform.SetParent(root.transform, false);

            GameObject generatedContentRoot = new GameObject("Generated Forest Content");
            generatedContentRoot.transform.SetParent(environmentRoot.transform, false);
            GameObject villageRoot = new GameObject("Village");
            villageRoot.transform.SetParent(environmentRoot.transform, false);
            GameObject generatedVillageContentRoot = new GameObject("Generated Village Content");
            generatedVillageContentRoot.transform.SetParent(villageRoot.transform, false);

            LightingPhaseManager lightingManager = lightingRoot.AddComponent<LightingPhaseManager>();
            ForestEnvironmentBootstrapper bootstrapper = environmentRoot.AddComponent<ForestEnvironmentBootstrapper>();
            VillageEnvironmentBootstrapper villageBootstrapper = villageRoot.AddComponent<VillageEnvironmentBootstrapper>();

            AssignLightingManager(lightingManager, lightingProfile, directionalLight);
            AssignBootstrapper(bootstrapper, terrainProfile, lightingProfile, lightingManager, generatedContentRoot.transform, terrainData);

            bootstrapper.BuildEnvironment();
            AssignVillageBootstrapper(villageBootstrapper, villageProfile, bootstrapper.Terrain, generatedVillageContentRoot.transform);
            villageBootstrapper.BuildVillage();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static TerrainEnvironmentProfile LoadOrCreateTerrainProfile()
        {
            TerrainEnvironmentProfile profile = AssetDatabase.LoadAssetAtPath<TerrainEnvironmentProfile>(TerrainProfilePath);
            if (profile == null)
            {
                profile = TerrainEnvironmentProfile.CreateRuntimeDefaults();
                AssetDatabase.CreateAsset(profile, TerrainProfilePath);
            }

            TerrainLayer moss = AssetDatabase.LoadAssetAtPath<TerrainLayer>(MossLayerPath);
            TerrainLayer road = AssetDatabase.LoadAssetAtPath<TerrainLayer>(RoadLayerPath);
            TerrainLayer rock = AssetDatabase.LoadAssetAtPath<TerrainLayer>(RockLayerPath);
            TerrainLayer forestFloor = LoadOrCreateForestFloorLayer(moss);

            GameObject[] treePrefabs =
            {
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Supercyan Free Forest Sample/Prefabs/High Quality/Tree/Fir/forestpack_tree_fir_tall.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Supercyan Free Forest Sample/Prefabs/High Quality/Tree/Leaf/Normal/forestpack_tree_1_leaf_1.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PolyOne/Free Tree/Prefabs/SM_FreeTree_01.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PolyOne/Free Tree/Prefabs/SM_FreeTree_02.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PolyOne/Free Tree/Prefabs/SM_FreeTree_03.prefab")
            };

            GameObject[] rockPrefabs =
            {
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Supercyan Free Forest Sample/Prefabs/High Quality/Stone/forestpack_stone_large_1.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Supercyan Free Forest Sample/Prefabs/High Quality/Stone/forestpack_stone_medium_1.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Rocks and Boulders 2/Rocks/Prefabs/Rock1A.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Rocks and Boulders 2/Rocks/Prefabs/Rock2.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Rocks and Boulders 2/Rocks/Prefabs/Rock4A.prefab")
            };

            GameObject[] foliagePrefabs =
            {
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Supercyan Free Forest Sample/Prefabs/High Quality/Foliage/Grass/forestpack_foliage_grassPatch_small_1.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Supercyan Free Forest Sample/Prefabs/High Quality/Foliage/Grass/forestpack_foliage_grassPatch_small_2.prefab")
            };

            SerializedObject serializedProfile = new SerializedObject(profile);
            SetObjectArray(serializedProfile.FindProperty("_terrainLayers"), new Object[] { moss, road, forestFloor, rock });
            SetObjectArray(serializedProfile.FindProperty("_treePrefabs"), treePrefabs);
            SetObjectArray(serializedProfile.FindProperty("_rockPrefabs"), rockPrefabs);
            SetObjectArray(serializedProfile.FindProperty("_foliagePrefabs"), foliagePrefabs);
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);

            return profile;
        }

        private static LightingPhaseProfile LoadOrCreateLightingProfile()
        {
            LightingPhaseProfile profile = AssetDatabase.LoadAssetAtPath<LightingPhaseProfile>(LightingProfilePath);
            if (profile == null)
            {
                profile = LightingPhaseProfile.CreateRuntimeDefaults();
                AssetDatabase.CreateAsset(profile, LightingProfilePath);
            }

            return profile;
        }

        private static VillageEnvironmentProfile LoadOrCreateVillageProfile()
        {
            VillageEnvironmentProfile profile = AssetDatabase.LoadAssetAtPath<VillageEnvironmentProfile>(VillageProfilePath);
            if (profile == null)
            {
                profile = VillageEnvironmentProfile.CreateRuntimeDefaults();
                AssetDatabase.CreateAsset(profile, VillageProfilePath);
            }

            GameObject[] residencePrefabs =
            {
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Buildings/pf_build_small_house_01.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Buildings/pf_build_small_house_straw_roof_01.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Buildings/pf_build_small_house_tall_roof_01.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/3DForge/Blueprints/PremiumBlueprints/PB_VIK_VEK/FrontierSettlement/BLUEPRINTS/Frontiers/PB_FS_House.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Buildings/pf_build_small_house_01.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Buildings/pf_build_small_house_straw_roof_01.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Buildings/pf_build_small_house_tall_roof_01.prefab")
            };

            GameObject[] ambientProps =
            {
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Props/pf_barrels_01.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Props/pf_buckets_01.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Props/pf_logpile_01.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Props/pf_shed_01.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Props/pf_torch_stick_01.prefab")
            };

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("_elderHousePrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Buildings/pf_build_bighouse_01.prefab");
            serializedProfile.FindProperty("_traderShopPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Buildings/pf_build_bighouse_02.prefab");
            serializedProfile.FindProperty("_blacksmithPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/3DForge/Blueprints/PremiumBlueprints/PB_VIK_VEK/FrontierSettlement/BLUEPRINTS/Frontiers/PB_FS_Blacksmith.prefab");
            serializedProfile.FindProperty("_healerHutPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/3DForge/Blueprints/PremiumBlueprints/PB_VIK_VEK/FrontierSettlement/BLUEPRINTS/Frontiers/PB_FS_House.prefab");
            serializedProfile.FindProperty("_tavernPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Buildings/pf_build_bighouse_02.prefab");
            SetObjectArray(serializedProfile.FindProperty("_residencePrefabs"), residencePrefabs);
            serializedProfile.FindProperty("_storageBarnPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Buildings/pf_build_big_storage_01.prefab");
            serializedProfile.FindProperty("_watchTowerPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Buildings/pf_build_tower_01.prefab");
            serializedProfile.FindProperty("_stablePrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Buildings/pf_build_barracks_single_01.prefab");
            serializedProfile.FindProperty("_wallPanelPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Buildings/pf_build_wall_panel_01.prefab");
            serializedProfile.FindProperty("_wallCornerPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Buildings/pf_build_wall_corner_01.prefab");
            serializedProfile.FindProperty("_gatePrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Buildings/pf_build_gate_01.prefab");
            serializedProfile.FindProperty("_fencePrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Props/pf_fence_01_double.prefab");
            serializedProfile.FindProperty("_pathPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Prefabs/Props/pf_plankpath_01.prefab");
            SetObjectArray(serializedProfile.FindProperty("_ambientProps"), ambientProps);
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);

            return profile;
        }

        private static TerrainData LoadOrCreateTerrainData()
        {
            TerrainData terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(TerrainDataPath);
            if (terrainData == null)
            {
                terrainData = new TerrainData();
                AssetDatabase.CreateAsset(terrainData, TerrainDataPath);
            }

            return terrainData;
        }

        private static TerrainLayer LoadOrCreateForestFloorLayer(TerrainLayer sourceLayer)
        {
            TerrainLayer terrainLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(ForestFloorLayerPath);
            if (terrainLayer == null)
            {
                terrainLayer = new TerrainLayer();
                if (sourceLayer != null)
                {
                    terrainLayer.diffuseTexture = sourceLayer.diffuseTexture;
                    terrainLayer.normalMapTexture = sourceLayer.normalMapTexture;
                    terrainLayer.maskMapTexture = sourceLayer.maskMapTexture;
                    terrainLayer.tileOffset = sourceLayer.tileOffset;
                    terrainLayer.metallic = sourceLayer.metallic;
                    terrainLayer.smoothness = sourceLayer.smoothness;
                    terrainLayer.normalScale = sourceLayer.normalScale;
                }

                terrainLayer.tileSize = new Vector2(7.5f, 7.5f);
                AssetDatabase.CreateAsset(terrainLayer, ForestFloorLayerPath);
            }

            return terrainLayer;
        }

        private static void AssignLightingManager(LightingPhaseManager lightingManager, LightingPhaseProfile lightingProfile, Light directionalLight)
        {
            SerializedObject serializedManager = new SerializedObject(lightingManager);
            serializedManager.FindProperty("_profile").objectReferenceValue = lightingProfile;
            serializedManager.FindProperty("_directionalLight").objectReferenceValue = directionalLight;
            serializedManager.FindProperty("_startingPhaseIndex").intValue = 0;
            serializedManager.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignBootstrapper(
            ForestEnvironmentBootstrapper bootstrapper,
            TerrainEnvironmentProfile terrainProfile,
            LightingPhaseProfile lightingProfile,
            LightingPhaseManager lightingManager,
            Transform contentRoot,
            TerrainData terrainData)
        {
            SerializedObject serializedBootstrapper = new SerializedObject(bootstrapper);
            serializedBootstrapper.FindProperty("_terrainProfile").objectReferenceValue = terrainProfile;
            serializedBootstrapper.FindProperty("_lightingProfile").objectReferenceValue = lightingProfile;
            serializedBootstrapper.FindProperty("_lightingManager").objectReferenceValue = lightingManager;
            serializedBootstrapper.FindProperty("_contentRoot").objectReferenceValue = contentRoot;
            serializedBootstrapper.FindProperty("_terrainData").objectReferenceValue = terrainData;
            serializedBootstrapper.FindProperty("_buildOnAwake").boolValue = false;
            serializedBootstrapper.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignVillageBootstrapper(
            VillageEnvironmentBootstrapper bootstrapper,
            VillageEnvironmentProfile villageProfile,
            Terrain terrain,
            Transform contentRoot)
        {
            SerializedObject serializedBootstrapper = new SerializedObject(bootstrapper);
            serializedBootstrapper.FindProperty("_profile").objectReferenceValue = villageProfile;
            serializedBootstrapper.FindProperty("_terrain").objectReferenceValue = terrain;
            serializedBootstrapper.FindProperty("_contentRoot").objectReferenceValue = contentRoot;
            serializedBootstrapper.FindProperty("_buildOnAwake").boolValue = false;
            serializedBootstrapper.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectArray(SerializedProperty arrayProperty, Object[] values)
        {
            arrayProperty.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parentFolder = folderPath.Substring(0, folderPath.LastIndexOf('/'));
            string folderName = folderPath.Substring(folderPath.LastIndexOf('/') + 1);
            EnsureFolder(parentFolder);
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }
    }
}
