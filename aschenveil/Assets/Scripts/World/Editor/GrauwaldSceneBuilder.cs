using System.Collections.Generic;
using Ashenveil.AI;
using Ashenveil.AI.Boss;
using Ashenveil.Aether;
using Ashenveil.Combat;
using Ashenveil.Core;
using Ashenveil.Dialog;
using Ashenveil.Flow;
using Ashenveil.Inventory;
using Ashenveil.Player;
using Ashenveil.Trade;
using Ashenveil.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Ashenveil.World.Editor
{
    /// <summary>
    /// Source of truth for the Grauwald demo scene. Builds the entire playable loop from
    /// code — terrain, Mischwald (no fog), nordic village, aether crystal clearing, boss
    /// arena, player rig, UI, and all runtime wiring — so the scene is reproducible and
    /// never hand-edited. Run via menu or -executeMethod BuildGrauwaldScene.
    /// Referenced GDD section: Demo-Ablauf.
    /// </summary>
    public static class GrauwaldSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Grauwald.unity";
        private const string TreeDir = "Assets/Environment/Trees";
        private const string VikingBuildings = "Assets/Viking Village/Prefabs/Buildings";

        // Landmark anchors (X,Z). Village near origin; deep forest + crystal to the north; boss beyond.
        private static readonly Vector3 PlayerStart = new Vector3(0f, 1f, -40f);
        private static readonly Vector3 VillageCenter = new Vector3(0f, 0f, 0f);
        private static readonly Vector3 CrystalPos = new Vector3(20f, 0f, 90f);
        private static readonly Vector3 BossArenaPos = new Vector3(-10f, 0f, 130f);

        [MenuItem("Ashenveil/Build Grauwald Scene")]
        public static void BuildGrauwaldScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GrauwaldContentFactory.Content content = GrauwaldContentFactory.Build();

            BuildLightingAndVolume();
            Transform ground = BuildGround();
            BuildForest();

            // Village (normal + burning variants share a parent for the destruction swap).
            var villageNormal = new GameObject("Village_Normal");
            var villageBurning = new GameObject("Village_Burning");
            BuildVillage(villageNormal.transform);
            BuildBurningVillage(villageBurning.transform);

            // Player rig + camera.
            PlayerRig rig = BuildPlayer(content);

            // Aether crystal in the deep-forest clearing.
            AetherCrystal crystal = BuildCrystal(rig.AetherPool);

            // Boss + arena.
            (MutatedWolfBossController boss, BossArenaTrigger arena) = BuildBoss();

            // Wildlife hunting grounds between start and village.
            BuildWildlife(content, rig.Root.transform);

            // UI + services + director.
            UiRefs ui = BuildUi(rig, content);
            var director = BuildDirector(ui, villageNormal, villageBurning);

            // NPCs (healer, smith, vendor) with their dialog/trade + services wiring.
            BuildNpcs(content, rig, ui, director);

            BossBind(ui.BossBar, boss, arena);

            EnsureScenesFolder();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[GrauwaldSceneBuilder] Grauwald scene built at " + ScenePath);
        }

        // ---------------------------------------------------------------- lighting

        private static void BuildLightingAndVolume()
        {
            var sunGo = new GameObject("Directional Light (Sun)");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.96f, 0.86f);
            sun.intensity = 1.5f;
            sun.shadows = LightShadows.Soft;
            sunGo.transform.rotation = Quaternion.Euler(48f, -30f, 0f);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.42f, 0.48f, 0.55f);
            RenderSettings.ambientEquatorColor = new Color(0.30f, 0.33f, 0.30f);
            RenderSettings.ambientGroundColor = new Color(0.12f, 0.12f, 0.10f);
            // GDD: KEIN Nebel im Wald.
            RenderSettings.fog = false;

            // Global post volume (URP). ACES/TAA are project-wide; the volume adds grade.
            var volumeGo = new GameObject("Global Volume");
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, "Assets/Settings/GrauwaldVolumeProfile.asset");
            volume.sharedProfile = profile;
        }

        // ---------------------------------------------------------------- ground

        private static Transform BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(40f, 1f, 40f); // 400x400 m
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                color = new Color(0.20f, 0.24f, 0.15f)
            };
            AssetDatabase.CreateAsset(mat, "Assets/Settings/GroundMaterial.asset");
            ground.GetComponent<MeshRenderer>().sharedMaterial = mat;
            ground.isStatic = true;
            ground.tag = "Untagged";
            return ground.transform;
        }

        // ---------------------------------------------------------------- forest

        private static void BuildForest()
        {
            List<GameObject> trees = LoadTrees();
            if (trees.Count == 0)
            {
                Debug.LogWarning("[GrauwaldSceneBuilder] No tree models found in " + TreeDir);
                return;
            }

            var forest = new GameObject("Forest");
            var rng = new System.Random(1337);
            int placed = 0;

            for (int i = 0; i < 900; i++)
            {
                float x = (float)(rng.NextDouble() * 360.0 - 180.0);
                float z = (float)(rng.NextDouble() * 360.0 - 180.0);
                var pos = new Vector3(x, 0f, z);

                // Keep clearings: village, player path, crystal, boss arena.
                if (Near(pos, VillageCenter, 34f)) continue;
                if (Near(pos, CrystalPos, 12f)) continue;
                if (Near(pos, BossArenaPos, 16f)) continue;
                if (Mathf.Abs(x) < 5f && z < -10f && z > -45f) continue; // opening path

                GameObject prefab = trees[rng.Next(trees.Count)];
                var tree = (GameObject)PrefabUtility.InstantiatePrefab(prefab, forest.transform);
                tree.transform.position = pos;
                tree.transform.rotation = Quaternion.Euler(0f, (float)(rng.NextDouble() * 360.0), 0f);
                float s = 0.9f + (float)rng.NextDouble() * 0.5f;
                tree.transform.localScale = new Vector3(s, s, s);
                AddTrunkCollider(tree);
                placed++;
            }

            Debug.Log($"[GrauwaldSceneBuilder] Placed {placed} trees.");
        }

        private static List<GameObject> LoadTrees()
        {
            var list = new List<GameObject>();
            foreach (string guid in AssetDatabase.FindAssets("t:GameObject", new[] { TreeDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("LOD0"))
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (go != null)
                    {
                        list.Add(go);
                    }
                }
            }

            return list;
        }

        private static void AddTrunkCollider(GameObject tree)
        {
            var col = tree.AddComponent<CapsuleCollider>();
            col.radius = 0.4f;
            col.height = 12f;
            col.center = new Vector3(0f, 6f, 0f);
        }

        // ---------------------------------------------------------------- village

        private static void BuildVillage(Transform parent)
        {
            string[] houses =
            {
                "pf_build_small_house_straw_roof_01",
                "pf_build_small_house_tall_roof_01",
                "pf_build_small_house_01",
                "pf_build_bighouse_01",
                "pf_build_storage_01",
                "pf_build_barracks_single_01"
            };

            var rng = new System.Random(77);
            int count = 6;
            for (int i = 0; i < count; i++)
            {
                string name = houses[i % houses.Length];
                GameObject prefab = LoadPrefab($"{VikingBuildings}/{name}.prefab");
                float angle = i / (float)count * Mathf.PI * 2f;
                var pos = VillageCenter + new Vector3(Mathf.Cos(angle) * 16f, 0f, Mathf.Sin(angle) * 16f);
                if (prefab != null)
                {
                    var house = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                    house.transform.position = pos;
                    house.transform.rotation = Quaternion.LookRotation(VillageCenter - pos);
                }
                else
                {
                    // Fallback block so the village exists even if the pack is missing.
                    var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    block.transform.SetParent(parent);
                    block.transform.position = pos + Vector3.up * 2f;
                    block.transform.localScale = new Vector3(6f, 4f, 6f);
                }
            }
        }

        private static void BuildBurningVillage(Transform parent)
        {
            // Reuse the same buildings tinted dark, plus fire/smoke particle stand-ins.
            BuildVillage(parent);
            foreach (Transform child in parent)
            {
                var renderers = child.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers)
                {
                    var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                    {
                        color = new Color(0.10f, 0.07f, 0.05f)
                    };
                    r.sharedMaterial = mat;
                }
            }

            // Orange fire light for atmosphere.
            var fire = new GameObject("FireGlow");
            fire.transform.SetParent(parent);
            fire.transform.position = VillageCenter + Vector3.up * 4f;
            var light = fire.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.45f, 0.15f);
            light.intensity = 6f;
            light.range = 45f;
        }

        // ---------------------------------------------------------------- player

        private sealed class PlayerRig
        {
            public GameObject Root;
            public Camera Camera;
            public PlayerMovementController Movement;
            public PlayerVitals Vitals;
            public PlayerInventory Inventory;
            public CombatController Combat;
            public AetherPool AetherPool;
        }

        private static PlayerRig BuildPlayer(GrauwaldContentFactory.Content content)
        {
            EnsureTag("Player");

            var root = new GameObject("Player");
            root.tag = "Player";
            root.transform.position = PlayerStart;

            var cc = root.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            // Visible body.
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            Object.DestroyImmediate(body.GetComponent<Collider>());

            var inputConfig = ScriptableObject.CreateInstance<PlayerInputConfig>();
            AssetDatabase.CreateAsset(inputConfig, "Assets/ScriptableObjects/Player_InputConfig.asset");

            var vitals = root.AddComponent<PlayerVitals>();
            SetRef(vitals, "_inputConfig", inputConfig);
            SetFloat(vitals, "_maxHealth", 100f);

            var movement = root.AddComponent<PlayerMovementController>();
            SetRef(movement, "_inputConfig", inputConfig);
            SetRef(movement, "_vitals", vitals);

            var inventory = root.AddComponent<PlayerInventory>();

            var aetherPool = root.AddComponent<AetherPool>();

            // Weapon hitbox on a child.
            var hitboxGo = new GameObject("WeaponHitbox");
            hitboxGo.transform.SetParent(root.transform, false);
            hitboxGo.transform.localPosition = new Vector3(0f, 1f, 1.2f);
            var hitboxCol = hitboxGo.AddComponent<BoxCollider>();
            hitboxCol.isTrigger = true;
            hitboxCol.size = new Vector3(1.4f, 1.4f, 2.4f);
            var hitbox = hitboxGo.AddComponent<WeaponHitbox>();

            var combat = root.AddComponent<CombatController>();
            SetRef(combat, "_weaponDefinition", content.Sword);
            SetRef(combat, "_movementController", movement);
            SetRef(combat, "_weaponHitbox", hitbox);

            var bridge = root.AddComponent<AetherCombatBridge>();
            SetRef(bridge, "_aetherPool", aetherPool);
            SetRef(bridge, "_movementController", movement);
            SetRef(bridge, "_combatController", combat);

            // Hand glow light.
            var handGo = new GameObject("HandGlow");
            handGo.transform.SetParent(root.transform, false);
            handGo.transform.localPosition = new Vector3(0.3f, 1f, 0.5f);
            var handLight = handGo.AddComponent<Light>();
            handLight.type = LightType.Point;
            handLight.color = new Color(0.3f, 0.85f, 0.95f);
            handLight.range = 4f;
            handLight.intensity = 0f;
            var glow = root.AddComponent<AetherHandGlow>();
            SetRef(glow, "_aetherPool", aetherPool);
            SetRef(glow, "_handLight", handLight);

            // Camera.
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            var camData = camGo.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.TemporalAntiAliasing;
            var camCtrl = camGo.AddComponent<Ashenveil.CameraRig.ThirdPersonCameraController>();
            SetRef(camCtrl, "_target", root.transform);
            SetRef(movement, "_cameraTransform", camGo.transform);

            return new PlayerRig
            {
                Root = root,
                Camera = cam,
                Movement = movement,
                Vitals = vitals,
                Inventory = inventory,
                Combat = combat,
                AetherPool = aetherPool
            };
        }

        // ---------------------------------------------------------------- crystal

        private static AetherCrystal BuildCrystal(AetherPool pool)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "AetherCrystal";
            go.transform.position = CrystalPos + Vector3.up * 1.2f;
            go.transform.localScale = new Vector3(0.8f, 2.2f, 0.8f);
            go.transform.rotation = Quaternion.Euler(12f, 30f, 8f);
            var col = go.GetComponent<Collider>();
            col.isTrigger = false;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                color = new Color(0.2f, 0.7f, 0.85f)
            };
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.3f, 1.4f, 1.7f));
            AssetDatabase.CreateAsset(mat, "Assets/Settings/CrystalMaterial.asset");
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var glowLight = new GameObject("CrystalLight");
            glowLight.transform.SetParent(go.transform, false);
            var l = glowLight.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.3f, 0.9f, 1f);
            l.range = 14f;
            l.intensity = 4f;

            var crystal = go.AddComponent<AetherCrystal>();
            SetRef(crystal, "_aetherPool", pool);
            return crystal;
        }

        // ---------------------------------------------------------------- boss

        private static (MutatedWolfBossController, BossArenaTrigger) BuildBoss()
        {
            var bossGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bossGo.name = "MutatedWolf";
            bossGo.transform.position = BossArenaPos + Vector3.up * 1f;
            bossGo.transform.localScale = new Vector3(1.6f, 1.2f, 2.6f);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                color = new Color(0.18f, 0.14f, 0.16f)
            };
            AssetDatabase.CreateAsset(mat, "Assets/Settings/BossMaterial.asset");
            bossGo.GetComponent<MeshRenderer>().sharedMaterial = mat;
            var boss = bossGo.AddComponent<MutatedWolfBossController>();

            var arenaGo = new GameObject("BossArena");
            arenaGo.transform.position = BossArenaPos;
            var trigger = arenaGo.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 14f;
            var arena = arenaGo.AddComponent<BossArenaTrigger>();
            SetRef(arena, "_boss", boss);

            return (boss, arena);
        }

        // ---------------------------------------------------------------- wildlife

        private static void BuildWildlife(GrauwaldContentFactory.Content content, Transform threat)
        {
            var agentPrefab = BuildWildlifeAgentPrefab();

            SpawnHerd("DeerSpawner", content.Deer, agentPrefab, threat, new Vector3(25f, 0f, -20f), 3);
            SpawnHerd("BoarSpawner", content.Boar, agentPrefab, threat, new Vector3(-28f, 0f, -18f), 2);
        }

        private static WildlifeAgent BuildWildlifeAgentPrefab()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "WildlifeAgent";
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.2f;
            cc.radius = 0.4f;
            var health = go.AddComponent<WildlifeHealth>();
            var agent = go.AddComponent<WildlifeAgent>();

            const string prefabPath = "Assets/ScriptableObjects/Wildlife/WildlifeAgent.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<WildlifeAgent>();
        }

        private static void SpawnHerd(string name, WildlifeSpecies species, WildlifeAgent agentPrefab, Transform threat, Vector3 center, int count)
        {
            var spawnerGo = new GameObject(name);
            spawnerGo.transform.position = center;
            var spawner = spawnerGo.AddComponent<WildlifeSpawner>();
            SetRef(spawner, "_species", species);
            SetRef(spawner, "_agentPrefab", agentPrefab);
            SetInt(spawner, "_maxAlive", count);
            SetRef(spawner, "_threat", threat);

            var points = new List<Object>();
            for (int i = 0; i < count; i++)
            {
                var p = new GameObject("Spawn" + i);
                p.transform.SetParent(spawnerGo.transform);
                p.transform.position = center + new Vector3(i * 4f - count * 2f, 0f, 0f);
                points.Add(p.transform);
            }

            SetList(spawner, "_spawnPoints", points);
        }

        // ---------------------------------------------------------------- UI

        private sealed class UiRefs
        {
            public HudController Hud;
            public FadeScreenController Fade;
            public EndScreenController End;
            public DialogScreenController Dialog;
            public InventoryScreenController Inventory;
            public TradeScreenController Trade;
            public JournalScreenController Journal;
            public BossBarController BossBar;
        }

        private static UiRefs BuildUi(PlayerRig rig, GrauwaldContentFactory.Content content)
        {
            var uiRoot = new GameObject("UI");

            var hud = NewChild(uiRoot, "HUD").AddComponent<HudController>();
            SetRef(hud, "_vitals", rig.Vitals);
            SetRef(hud, "_aetherPool", rig.AetherPool);

            var fade = NewChild(uiRoot, "Fade").AddComponent<FadeScreenController>();
            var end = NewChild(uiRoot, "EndScreen").AddComponent<EndScreenController>();
            var dialog = NewChild(uiRoot, "DialogScreen").AddComponent<DialogScreenController>();

            var inv = NewChild(uiRoot, "InventoryScreen").AddComponent<InventoryScreenController>();
            SetRef(inv, "_movementController", rig.Movement);
            SetRef(inv, "_playerInventory", rig.Inventory);

            var trade = NewChild(uiRoot, "TradeScreen").AddComponent<TradeScreenController>();
            var journal = NewChild(uiRoot, "JournalScreen").AddComponent<JournalScreenController>();
            var bossBar = NewChild(uiRoot, "BossBar").AddComponent<BossBarController>();

            // Interactor needs HUD to display prompts.
            var interactor = rig.Root.AddComponent<PlayerInteractor>();
            SetRef(interactor, "_camera", rig.Camera);
            SetRef(interactor, "_movementController", rig.Movement);
            SetRef(interactor, "_hud", hud);

            return new UiRefs
            {
                Hud = hud, Fade = fade, End = end, Dialog = dialog,
                Inventory = inv, Trade = trade, Journal = journal, BossBar = bossBar
            };
        }

        private static DemoDirector BuildDirector(UiRefs ui, GameObject villageNormal, GameObject villageBurning)
        {
            var go = new GameObject("DemoDirector");
            var director = go.AddComponent<DemoDirector>();
            SetRef(director, "_hud", ui.Hud);
            SetRef(director, "_fade", ui.Fade);
            SetRef(director, "_endScreen", ui.End);
            SetRef(director, "_villageNormalRoot", villageNormal);
            SetRef(director, "_villageBurningRoot", villageBurning);
            return director;
        }

        // ---------------------------------------------------------------- NPCs

        private static void BuildNpcs(GrauwaldContentFactory.Content content, PlayerRig rig, UiRefs ui, DemoDirector director)
        {
            var speakers = new List<Object>();
            var vendors = new List<Object>();

            DialogSpeaker healer = BuildSpeaker("Heilerin", content.HealerDialog, VillageCenter + new Vector3(6f, 0f, 4f));
            DialogSpeaker smith = BuildSpeaker("Schmied", content.SmithDialog, VillageCenter + new Vector3(-6f, 0f, 4f));
            speakers.Add(healer);
            speakers.Add(smith);

            VendorController vendor = BuildVendor(content, rig.Inventory, VillageCenter + new Vector3(0f, 0f, 8f));
            vendors.Add(vendor);

            var servicesGo = new GameObject("GameServices");
            var services = servicesGo.AddComponent<GameServices>();
            SetRef(services, "_playerInventory", rig.Inventory);
            SetRef(services, "_dialogScreen", ui.Dialog);
            SetRef(services, "_tradeScreen", ui.Trade);
            SetRef(services, "_journalScreen", ui.Journal);
            SetList(services, "_quests", ToObjectList(content.AllQuests));
            SetList(services, "_speakers", speakers);
            SetList(services, "_vendors", vendors);
        }

        private static DialogSpeaker BuildSpeaker(string npcName, DialogGraph graph, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "NPC_" + npcName;
            go.transform.position = pos + Vector3.up * 1f;
            var speaker = go.AddComponent<DialogSpeaker>();
            SetRef(speaker, "_dialogGraph", graph);
            SetString(speaker, "_npcDisplayName", npcName);
            return speaker;
        }

        private static VendorController BuildVendor(GrauwaldContentFactory.Content content, PlayerInventory inventory, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "NPC_Vendor";
            go.transform.position = pos + Vector3.up * 1f;
            var vendor = go.AddComponent<VendorController>();
            SetRef(vendor, "_vendorDefinition", content.Vendor);
            SetRef(vendor, "_playerInventory", inventory);
            return vendor;
        }

        private static void BossBind(BossBarController bar, MutatedWolfBossController boss, BossArenaTrigger arena)
        {
            SetRef(bar, "_boss", boss);
            SetRef(bar, "_arenaTrigger", arena);
        }

        // ---------------------------------------------------------------- helpers

        private static bool Near(Vector3 a, Vector3 b, float dist)
        {
            a.y = 0f; b.y = 0f;
            return Vector3.Distance(a, b) < dist;
        }

        private static GameObject NewChild(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static GameObject LoadPrefab(string path)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static List<Object> ToObjectList<T>(List<T> items) where T : Object
        {
            var list = new List<Object>();
            foreach (T item in items)
            {
                list.Add(item);
            }

            return list;
        }

        private static void EnsureScenesFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
        }

        private static void EnsureTag(string tag)
        {
            EnsureTagExists(tag);
        }

        private static void EnsureTagExists(string tag)
        {
            var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (asset.Length == 0)
            {
                return;
            }

            var so = new SerializedObject(asset[0]);
            SerializedProperty tags = so.FindProperty("tags");
            for (int i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    return;
                }
            }

            tags.arraySize++;
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Serialized-field setters (private fields authored from the editor).
        private static void SetRef(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(Object target, string field, float value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInt(Object target, string field, int value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetString(Object target, string field, string value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetList(Object target, string field, List<Object> values)
        {
            var so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(field);
            prop.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
