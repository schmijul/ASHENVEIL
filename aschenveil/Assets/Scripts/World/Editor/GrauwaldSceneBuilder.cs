using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ashenveil.Player;
using Ashenveil.Camera;

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

        private const string PlayerCharacterPrefabPath = "Assets/Blink/Art/Characters/Stylized/Humans/Prefabs_Humans/HumanMale_Character_Free.prefab";
        private const string InputActionsPath = "Assets/Settings/AshenveilInputActions.inputactions";
        private const string FallbackInputActionsPath = "Assets/InputSystem_Actions.inputactions";

        private const string AnimControllerPath = "Assets/ScriptableObjects/Player/PlayerMovementAnimator.controller";

        private const string IdleAnimPath = "Assets/Kevin Iglesias/Human Animations/Animations/Male/Idles/HumanM@Idle01.fbx";
        private const string WalkAnimPath = "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Forward.fbx";
        private const string RunAnimPath = "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Run/HumanM@Run01_Forward.fbx";
        private const string SprintAnimPath = "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Sprint/HumanM@Sprint01_Forward.fbx";
        private const string JumpAnimPath = "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Jump/HumanM@Jump01.fbx";
        private const string FallAnimPath = "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Jump/HumanM@Fall01.fbx";

        private const string LogPilePrefabPath = "Assets/Viking Village/Prefabs/Props/pf_logpile_01.prefab";
        private const string WallLogsPrefabPath = "Assets/Viking Village/Prefabs/Props/pf_wall_logs_04.prefab";
        private const string LargeRockPrefabPath = "Assets/Rocks and Boulders 2/Rocks/Prefabs/Rock1A.prefab";
        private const string TreeStumpPrefabPath = "Assets/Supercyan Free Forest Sample/Prefabs/High Quality/Tree/Treestump/forestpack_tree_stump_1.prefab";

        private static readonly Vector3[] TutorialPathWaypoints = new Vector3[]
        {
            new Vector3(150f, 0f, 300f),   // Start
            new Vector3(152f, 0f, 288f),   // slight right
            new Vector3(147f, 0f, 275f),   // curve left
            new Vector3(155f, 0f, 262f),   // curve right
            new Vector3(150f, 0f, 250f),   // back to center
            new Vector3(143f, 0f, 238f),   // curve left
            new Vector3(147f, 0f, 225f),   // Hindernis area
            new Vector3(155f, 0f, 214f),   // curve right
            new Vector3(163f, 0f, 204f),   // opening toward clearing
            new Vector3(170f, 0f, 195f),   // clearing entry
            new Vector3(172f, 0f, 185f),   // clearing — Item (left) + Gegner (right)
        };

        [MenuItem("Ashenveil/World/Build Grauwald Scene")]
        public static void BuildGrauwaldSceneMenu()
        {
            BuildGrauwaldScene();
        }

        private static AnimationClip LoadClipFromFBX(string path)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            if (assets == null) return null;
            foreach (Object asset in assets)
            {
                if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                {
                    return clip;
                }
            }
            return null;
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
            ConfigureTutorialPath(bootstrapper);

            bootstrapper.BuildEnvironment();
            AssignVillageBootstrapper(villageBootstrapper, villageProfile, bootstrapper.Terrain, generatedVillageContentRoot.transform);
            villageBootstrapper.BuildVillage();

            // Player spawns in the forest for Phase 1 (The Hunt), not in the village
            Vector3 forestSpawnPoint = TutorialPathWaypoints[0];
            BuildPlayerSetup(root.transform, forestSpawnPoint, bootstrapper.Terrain);
            root.AddComponent<Ashenveil.World.OpeningLoopBootstrapper>();
            BuildTutorialObstacles(root.transform, bootstrapper.Terrain);

            UpgradeSceneMaterialsToURP(root.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void UpgradeSceneMaterialsToURP(Transform root)
        {
            Shader hdrpLit = Shader.Find("HDRP/Lit");
            if (hdrpLit == null)
            {
                // Fallback to URP if HDRP not yet active
                hdrpLit = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (hdrpLit == null)
            {
                Debug.LogWarning("Neither HDRP/Lit nor URP/Lit shader found — skipping material upgrade.");
                return;
            }

            bool isHDRP = hdrpLit.name.StartsWith("HDRP");

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            HashSet<Material> upgraded = new HashSet<Material>();
            int count = 0;

            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material mat = materials[i];
                    if (mat == null || upgraded.Contains(mat))
                        continue;

                    string shaderName = mat.shader.name;
                    bool needsUpgrade = shaderName == "Standard" ||
                        shaderName == "Standard (Specular setup)" ||
                        shaderName == "Legacy Shaders/Diffuse" ||
                        shaderName == "Legacy Shaders/Bumped Diffuse" ||
                        shaderName == "Legacy Shaders/Specular" ||
                        shaderName == "Legacy Shaders/Bumped Specular" ||
                        shaderName == "Legacy Shaders/Transparent/Diffuse" ||
                        shaderName == "Mobile/Diffuse" ||
                        shaderName == "Universal Render Pipeline/Lit" ||
                        shaderName == "Universal Render Pipeline/Simple Lit";

                    if (!needsUpgrade)
                        continue;

                    Texture mainTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                    if (mainTex == null && mat.HasProperty("_BaseMap")) mainTex = mat.GetTexture("_BaseMap");
                    if (mainTex == null && mat.HasProperty("_BaseColorMap")) mainTex = mat.GetTexture("_BaseColorMap");

                    Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") :
                                  mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white;

                    Texture normalMap = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;
                    if (normalMap == null && mat.HasProperty("_NormalMap")) normalMap = mat.GetTexture("_NormalMap");

                    float metallic = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0f;
                    float smoothness = mat.HasProperty("_Glossiness") ? mat.GetFloat("_Glossiness") :
                                       mat.HasProperty("_Smoothness") ? mat.GetFloat("_Smoothness") : 0.5f;

                    bool isTransparent = shaderName.Contains("Transparent") ||
                                         (mat.HasProperty("_Mode") && mat.GetFloat("_Mode") >= 2f) ||
                                         (mat.HasProperty("_Surface") && mat.GetFloat("_Surface") >= 1f);

                    mat.shader = hdrpLit;

                    if (isHDRP)
                    {
                        if (mainTex != null && mat.HasProperty("_BaseColorMap"))
                            mat.SetTexture("_BaseColorMap", mainTex);
                        if (mat.HasProperty("_BaseColor"))
                            mat.SetColor("_BaseColor", color);
                        if (normalMap != null && mat.HasProperty("_NormalMap"))
                            mat.SetTexture("_NormalMap", normalMap);
                        if (mat.HasProperty("_Metallic"))
                            mat.SetFloat("_Metallic", metallic);
                        if (mat.HasProperty("_Smoothness"))
                            mat.SetFloat("_Smoothness", smoothness);
                        if (isTransparent && mat.HasProperty("_SurfaceType"))
                            mat.SetFloat("_SurfaceType", 1f);
                    }
                    else
                    {
                        if (mainTex != null && mat.HasProperty("_BaseMap"))
                            mat.SetTexture("_BaseMap", mainTex);
                        if (mat.HasProperty("_BaseColor"))
                            mat.SetColor("_BaseColor", color);
                        if (normalMap != null && mat.HasProperty("_BumpMap"))
                            mat.SetTexture("_BumpMap", normalMap);
                        if (mat.HasProperty("_Metallic"))
                            mat.SetFloat("_Metallic", metallic);
                        if (mat.HasProperty("_Smoothness"))
                            mat.SetFloat("_Smoothness", smoothness);
                    }

                    EditorUtility.SetDirty(mat);
                    upgraded.Add(mat);
                    count++;
                }
            }

            if (count > 0)
            {
                Debug.Log($"Upgraded {count} material(s) to {(isHDRP ? "HDRP" : "URP")} Lit.");
            }
        }

        private static void BuildPlayerSetup(Transform sceneRoot, Vector3 spawnPoint, Terrain terrain)
        {
            float spawnHeight = terrain != null
                ? terrain.SampleHeight(spawnPoint) + terrain.transform.position.y
                : 0f;
            Vector3 spawnPosition = new Vector3(spawnPoint.x, spawnHeight + 0.1f, spawnPoint.z);

            // Face south along the tutorial path
            Vector3 pathDirection = (TutorialPathWaypoints[1] - TutorialPathWaypoints[0]).normalized;
            float facingYaw = Mathf.Atan2(pathDirection.x, pathDirection.z) * Mathf.Rad2Deg;

            // Player root
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Default");
            player.transform.SetParent(sceneRoot, false);
            player.transform.position = spawnPosition;
            player.transform.rotation = Quaternion.Euler(0f, facingYaw, 0f);

            // Character visual
            GameObject characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCharacterPrefabPath);
            Animator characterAnimator = null;
            if (characterPrefab != null)
            {
                GameObject characterModel = (GameObject)PrefabUtility.InstantiatePrefab(characterPrefab);
                characterModel.name = "CharacterModel";
                characterModel.transform.SetParent(player.transform, false);
                characterModel.transform.localPosition = Vector3.zero;
                characterModel.transform.localRotation = Quaternion.identity;

                characterAnimator = characterModel.GetComponentInChildren<Animator>();
                if (characterAnimator == null)
                {
                    characterAnimator = characterModel.AddComponent<Animator>();
                }
            }

            // Player components — RequireComponent auto-adds CharacterController
            PlayerInputHandler inputHandler = player.AddComponent<PlayerInputHandler>();
            StaminaSystem staminaSystem = player.AddComponent<StaminaSystem>();
            PlayerMovementController movementController = player.AddComponent<PlayerMovementController>();

            // Wire input asset
            var inputActions = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(InputActionsPath);
            if (inputActions == null)
            {
                inputActions = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(FallbackInputActionsPath);
            }
            if (inputActions != null)
            {
                SerializedObject serializedInput = new SerializedObject(inputHandler);
                serializedInput.FindProperty("_inputActions").objectReferenceValue = inputActions;
                serializedInput.ApplyModifiedPropertiesWithoutUndo();
            }

            // Wire movement controller
            SerializedObject serializedMovement = new SerializedObject(movementController);
            serializedMovement.FindProperty("_inputHandler").objectReferenceValue = inputHandler;
            serializedMovement.FindProperty("_staminaSystem").objectReferenceValue = staminaSystem;

            // Camera setup — position behind player along path direction
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(sceneRoot, false);
            cameraObject.transform.position = spawnPosition - pathDirection * 3f + Vector3.up * 2f;
            cameraObject.transform.rotation = Quaternion.Euler(15f, facingYaw, 0f);

            UnityEngine.Camera cam = cameraObject.AddComponent<UnityEngine.Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 55f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 1000f;
            cameraObject.AddComponent<AudioListener>();

            ThirdPersonCameraController cameraController = cameraObject.AddComponent<ThirdPersonCameraController>();
            SerializedObject serializedCamera = new SerializedObject(cameraController);
            serializedCamera.FindProperty("_target").objectReferenceValue = player.transform;
            serializedCamera.FindProperty("_inputHandler").objectReferenceValue = inputHandler;
            serializedCamera.ApplyModifiedPropertiesWithoutUndo();

            // Finish wiring movement to camera
            serializedMovement.FindProperty("_cameraTransform").objectReferenceValue = cameraObject.transform;
            serializedMovement.ApplyModifiedPropertiesWithoutUndo();

            // Animation setup
            if (characterAnimator != null)
            {
                RuntimeAnimatorController animController = BuildAnimatorController();
                if (animController != null)
                {
                    characterAnimator.runtimeAnimatorController = animController;
                    characterAnimator.applyRootMotion = false;

                    PlayerAnimationController animScript = characterAnimator.gameObject.AddComponent<PlayerAnimationController>();
                    SerializedObject serializedAnim = new SerializedObject(animScript);
                    serializedAnim.FindProperty("_inputHandler").objectReferenceValue = inputHandler;
                    serializedAnim.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        private static RuntimeAnimatorController BuildAnimatorController()
        {
            AnimationClip idleClip = LoadClipFromFBX(IdleAnimPath);
            AnimationClip walkClip = LoadClipFromFBX(WalkAnimPath);
            AnimationClip runClip = LoadClipFromFBX(RunAnimPath);
            AnimationClip sprintClip = LoadClipFromFBX(SprintAnimPath);
            AnimationClip jumpClip = LoadClipFromFBX(JumpAnimPath);
            AnimationClip fallClip = LoadClipFromFBX(FallAnimPath);

            if (idleClip == null || walkClip == null)
            {
                Debug.LogWarning("Could not load animation clips — skipping animator setup.");
                return null;
            }

            EnsureFolder("Assets/ScriptableObjects/Player");

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(AnimControllerPath);
            if (controller != null)
            {
                AssetDatabase.DeleteAsset(AnimControllerPath);
            }

            controller = AnimatorController.CreateAnimatorControllerAtPath(AnimControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine rootSM = controller.layers[0].stateMachine;

            // Locomotion blend tree: Idle → Walk → Run → Sprint
            BlendTree blendTree;
            AnimatorState locomotionState = controller.CreateBlendTreeInController("Locomotion", out blendTree);
            blendTree.blendParameter = "Speed";
            blendTree.blendType = BlendTreeType.Simple1D;
            blendTree.AddChild(idleClip, 0f);
            blendTree.AddChild(walkClip, 0.5f);
            if (runClip != null) blendTree.AddChild(runClip, 1f);
            if (sprintClip != null) blendTree.AddChild(sprintClip, 2f);

            rootSM.defaultState = locomotionState;

            // Jump state
            if (jumpClip != null)
            {
                AnimatorState jumpState = rootSM.AddState("Jump");
                jumpState.motion = jumpClip;

                AnimatorStateTransition toJump = locomotionState.AddTransition(jumpState);
                toJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");
                toJump.hasExitTime = false;
                toJump.duration = 0.1f;

                if (fallClip != null)
                {
                    AnimatorState fallState = rootSM.AddState("Fall");
                    fallState.motion = fallClip;

                    AnimatorStateTransition toFall = jumpState.AddTransition(fallState);
                    toFall.hasExitTime = true;
                    toFall.exitTime = 0.85f;
                    toFall.duration = 0.1f;

                    AnimatorStateTransition fallToLoco = fallState.AddTransition(locomotionState);
                    fallToLoco.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
                    fallToLoco.hasExitTime = false;
                    fallToLoco.duration = 0.15f;
                }
                else
                {
                    AnimatorStateTransition jumpToLoco = jumpState.AddTransition(locomotionState);
                    jumpToLoco.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
                    jumpToLoco.hasExitTime = true;
                    jumpToLoco.exitTime = 0.85f;
                    jumpToLoco.duration = 0.15f;
                }
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            return controller;
        }

        private static void ConfigureTutorialPath(ForestEnvironmentBootstrapper bootstrapper)
        {
            SerializedObject serialized = new SerializedObject(bootstrapper);
            SerializedProperty waypoints = serialized.FindProperty("_pathWaypoints");
            waypoints.arraySize = TutorialPathWaypoints.Length;
            for (int i = 0; i < TutorialPathWaypoints.Length; i++)
            {
                waypoints.GetArrayElementAtIndex(i).vector3Value = TutorialPathWaypoints[i];
            }
            serialized.FindProperty("_pathWidth").floatValue = 8f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildTutorialObstacles(Transform sceneRoot, Terrain terrain)
        {
            GameObject obstacleRoot = new GameObject("Tutorial Obstacles");
            obstacleRoot.transform.SetParent(sceneRoot, false);

            // Hindernis: Log pile across the path — low enough to jump over (~0.8m)
            GameObject logPilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LogPilePrefabPath);
            if (logPilePrefab != null)
            {
                PlaceObstacle(logPilePrefab, new Vector3(147f, 0f, 225f), 70f, 0.4f, obstacleRoot.transform, terrain, "JumpObstacle_Logs");
            }

            // Second obstacle: Wall logs, also low enough to jump
            GameObject wallLogsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WallLogsPrefabPath);
            if (wallLogsPrefab != null)
            {
                PlaceObstacle(wallLogsPrefab, new Vector3(155f, 0f, 214f), 55f, 0.35f, obstacleRoot.transform, terrain, "JumpObstacle_WallLogs");
            }

            // Item gefunden: Rock marking the item location in the clearing
            GameObject rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LargeRockPrefabPath);
            if (rockPrefab != null)
            {
                PlaceObstacle(rockPrefab, new Vector3(163f, 0f, 188f), 15f, 0.8f, obstacleRoot.transform, terrain, "ItemLocation_Rock");
            }

            // Stumps along the winding path for atmosphere
            GameObject stumpPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TreeStumpPrefabPath);
            if (stumpPrefab != null)
            {
                PlaceObstacle(stumpPrefab, new Vector3(149f, 0f, 280f), 45f, 1.2f, obstacleRoot.transform, terrain, "Stump_001");
                PlaceObstacle(stumpPrefab, new Vector3(153f, 0f, 255f), 130f, 1.0f, obstacleRoot.transform, terrain, "Stump_002");
                PlaceObstacle(stumpPrefab, new Vector3(145f, 0f, 235f), 200f, 0.9f, obstacleRoot.transform, terrain, "Stump_003");
            }
        }

        private static void PlaceObstacle(GameObject prefab, Vector3 position, float yRotation, float scale, Transform parent, Terrain terrain, string name)
        {
            float height = terrain != null
                ? terrain.SampleHeight(position) + terrain.transform.position.y
                : 0f;
            position.y = height;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
            instance.transform.localScale = prefab.transform.localScale * scale;

            // Ensure obstacle has a collider for physics interaction
            if (instance.GetComponentInChildren<Collider>() == null)
            {
                MeshFilter meshFilter = instance.GetComponentInChildren<MeshFilter>();
                if (meshFilter != null)
                {
                    MeshCollider collider = meshFilter.gameObject.AddComponent<MeshCollider>();
                    collider.convex = true;
                }
            }
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
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PolyOne/Free Tree/Prefabs/SM_FreeTree_03.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PolyOne/Free Tree/Prefabs/SM_FreeTree_04.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PolyOne/Free Tree/Prefabs/SM_FreeTree_05.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PolyOne/Free Tree/Prefabs/SM_FreeTree_06.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PolyOne/Free Tree/Prefabs/SM_FreeTree_07.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PolyOne/Free Tree/Prefabs/SM_FreeTree_08.prefab"),
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
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Supercyan Free Forest Sample/Prefabs/High Quality/Foliage/Grass/forestpack_foliage_grassPatch_small_2.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Book of the Dead/Vegetation/Ferns/Fern_var01_Prefab.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Book of the Dead/Vegetation/Ferns/Fern_var02_Prefab.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Book of the Dead/Vegetation/Ferns/Fern_var03_Prefab.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Book of the Dead/Vegetation/BroadleafShrub_01/Broadleaf_Shrub_01_Var4_Prefab.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Book of the Dead/Vegetation/MeadowGrass_01/Meadow_Grass_01_Var1_Prefab.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Book of the Dead/Vegetation/MeadowGrass_01/Meadow_Grass_01_Var3_Prefab.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Book of the Dead/Vegetation/Plant_Perennials/PH_Plant_Perennials_a2_1x1x2_A_Prefab.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Viking Village/Book of the Dead/Vegetation/Plant_Perennials/PH_Plant_Perennials_a2_1x1x2_B_Prefab.prefab"),
            };

            SerializedObject serializedProfile = new SerializedObject(profile);
            SetObjectArray(serializedProfile.FindProperty("_terrainLayers"), new Object[] { moss, road, forestFloor, rock });
            SetObjectArray(serializedProfile.FindProperty("_treePrefabs"), treePrefabs);
            SetObjectArray(serializedProfile.FindProperty("_rockPrefabs"), rockPrefabs);
            SetObjectArray(serializedProfile.FindProperty("_foliagePrefabs"), foliagePrefabs);

            // Override density values for a very dense mixed forest (GDD 3.2)
            serializedProfile.FindProperty("_treeCount").intValue = 8000;
            serializedProfile.FindProperty("_foliageCount").intValue = 5000;
            serializedProfile.FindProperty("_rockCount").intValue = 200;
            serializedProfile.FindProperty("_treeRadius").floatValue = 210f;
            serializedProfile.FindProperty("_rockRadius").floatValue = 220f;
            serializedProfile.FindProperty("_foliageRadius").floatValue = 220f;
            serializedProfile.FindProperty("_minTreeScale").floatValue = 1.5f;
            serializedProfile.FindProperty("_maxTreeScale").floatValue = 3.0f;

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
