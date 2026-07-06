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
using Ashenveil.VFX;
using UnityEditor;
using UnityEditor.Animations;
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
        private const string KenneyNatureKitDir = "Assets/Kenney/NatureKit";
        private const string VikingBuildings = "Assets/Viking Village/Prefabs/Buildings";

        // Landmark anchors (X,Z). Story order (GDD Phase 1→): the player WAKES deep in the
        // southern forest, walks north through the woods to the village, then continues north
        // into deeper forest to the crystal and boss beyond.
        private static readonly Vector3 PlayerStart = new Vector3(0f, 1f, -130f);
        private static readonly Vector3 VillageCenter = new Vector3(0f, 0f, 0f);
        private static readonly Vector3 CrystalPos = new Vector3(20f, 0f, 95f);
        private static readonly Vector3 BossArenaPos = new Vector3(-12f, 0f, 140f);
        private static readonly float GothicDesaturate = 0.55f;
        private static readonly float GothicDarken = 0.45f;
        private static Terrain _activeTerrain;

        [MenuItem("Ashenveil/Build Grauwald Scene")]
        public static void BuildGrauwaldScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GrauwaldContentFactory.Content content = GrauwaldContentFactory.Build();

            BuildLightingAndVolume();
            new GameObject("AudioDirector").AddComponent<Ashenveil.Audio.AudioDirector>();
            Transform ground = BuildGround();
            BuildForest();
            BuildUndergrowth();

            // Village (normal + burning variants share a parent for the destruction swap).
            var villageNormal = new GameObject("Village_Normal");
            var villageBurning = new GameObject("Village_Burning");
            BuildVillage(villageNormal.transform);
            BuildBurningVillage(villageBurning.transform);

            // Player rig + camera.
            PlayerRig rig = BuildPlayer(content);

            // Aether crystal in the deep-forest clearing.
            AetherCrystal crystal = BuildCrystal(rig.AetherPool);

            // Boss + arena (chases and damages the player).
            (MutatedWolfBossController boss, BossArenaTrigger arena) = BuildBoss(rig);

            // Wildlife hunting grounds between start and village.
            BuildWildlife(content, rig.Root.transform);

            // Quest items scattered in the world (herbs on the forest walk, hammer by the rocks).
            List<GameServices.QuestItemLink> questLinks = BuildQuestItems(content, rig.Inventory);

            // UI + services + director.
            UiRefs ui = BuildUi(rig, content);
            var director = BuildDirector(ui, villageNormal, villageBurning);

            // NPCs (healer, smith, vendor) with their dialog/trade + services wiring.
            BuildNpcs(content, rig, ui, director, questLinks);

            BossBind(ui.BossBar, boss, arena);

            EnsureScenesFolder();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[GrauwaldSceneBuilder] Grauwald scene built at " + ScenePath);
        }

        // ---------------------------------------------------------------- lighting

        private static void BuildLightingAndVolume()
        {
            // Low, warm sun — golden-hour rake for long soft shadows and mood.
            var sunGo = new GameObject("Directional Light (Sun)");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.82f, 0.62f);
            sun.intensity = 0.95f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.85f;
            sunGo.transform.rotation = Quaternion.Euler(24f, 40f, 0f);

            // Cool, low ambient so the warm sun reads and shadows stay moody.
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.30f, 0.36f, 0.46f);
            RenderSettings.ambientEquatorColor = new Color(0.22f, 0.24f, 0.22f);
            RenderSettings.ambientGroundColor = new Color(0.08f, 0.08f, 0.07f);

            // Atmospheric distance fog for depth (overrides the old "no fog" — the look
            // needs aerial perspective; it stays subtle so the near forest reads clearly).
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.42f, 0.45f, 0.48f);
            RenderSettings.fogDensity = 0.014f;

            BuildPostProcessing();
        }

        private static void BuildPostProcessing()
        {
            var volumeGo = new GameObject("Global Volume");
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, "Assets/Settings/GrauwaldVolumeProfile.asset");

            // ACES tonemapping — filmic contrast/rolloff, the core "not-flat" change.
            var tonemap = profile.Add<Tonemapping>(true);
            tonemap.mode.overrideState = true;
            tonemap.mode.value = TonemappingMode.ACES;

            // Moody grade: slight under-exposure, more contrast, desaturated, warm/cool split.
            var color = profile.Add<ColorAdjustments>(true);
            color.postExposure.overrideState = true;
            color.postExposure.value = -0.5f;
            color.contrast.overrideState = true;
            color.contrast.value = 18f;
            color.saturation.overrideState = true;
            color.saturation.value = -28f;
            color.colorFilter.overrideState = true;
            color.colorFilter.value = new Color(0.94f, 0.93f, 0.86f);

            var wb = profile.Add<WhiteBalance>(true);
            wb.temperature.overrideState = true;
            wb.temperature.value = 8f; // slightly warm

            // Cool shadows, warm highlights — classic cinematic split-tone.
            var smh = profile.Add<ShadowsMidtonesHighlights>(true);
            smh.shadows.overrideState = true;
            smh.shadows.value = new Vector4(0.90f, 0.97f, 1.08f, 0f);
            smh.highlights.overrideState = true;
            smh.highlights.value = new Vector4(1.06f, 1.0f, 0.90f, 0f);

            // Bloom to make the aether crystal and fire glow.
            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.9f;
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.9f;
            bloom.tint.overrideState = true;
            bloom.tint.value = new Color(0.95f, 0.95f, 1f);

            // Vignette to frame and darken edges.
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.32f;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.5f;

            volume.sharedProfile = profile;
            EditorUtility.SetDirty(profile);
        }

        // ---------------------------------------------------------------- ground

        private static Transform BuildGround()
        {
            var data = new TerrainData
            {
                heightmapResolution = 513,
                size = new Vector3(400f, 12f, 400f)
            };

            data.SetHeights(0, 0, BuildTerrainHeights(data.heightmapResolution, data.size));
            data.terrainLayers = BuildTerrainLayers();

            var ground = Terrain.CreateTerrainGameObject(data);
            ground.name = "Ground";
            ground.transform.position = new Vector3(-200f, 0f, -200f);
            ground.isStatic = true;
            ground.tag = "Untagged";

            var terrain = ground.GetComponent<Terrain>();
            terrain.materialTemplate = FindProjectTerrainMaterial();

            data.SetAlphamaps(0, 0, BuildTerrainAlphamaps(data));
            terrain.Flush();
            _activeTerrain = Terrain.activeTerrain != null ? Terrain.activeTerrain : terrain;
            return ground.transform;
        }

        private static float[,] BuildTerrainHeights(int resolution, Vector3 terrainSize)
        {
            var heights = new float[resolution, resolution];
            for (int y = 0; y < resolution; y++)
            {
                float nz = y / (float)(resolution - 1);
                float worldZ = nz * terrainSize.z - terrainSize.z * 0.5f;
                for (int x = 0; x < resolution; x++)
                {
                    float nx = x / (float)(resolution - 1);
                    float worldX = nx * terrainSize.x - terrainSize.x * 0.5f;

                    float broad = Mathf.PerlinNoise(worldX * 0.0085f + 17.3f, worldZ * 0.0085f + 91.7f);
                    float mid = Mathf.PerlinNoise(worldX * 0.021f + 141.9f, worldZ * 0.021f + 33.4f);
                    float fine = Mathf.PerlinNoise(worldX * 0.047f + 5.8f, worldZ * 0.047f + 211.2f);
                    float rise = Mathf.Pow(Mathf.PerlinNoise(worldX * 0.012f + 311.5f, worldZ * 0.012f + 18.6f), 3f);
                    float height = broad * 0.18f + mid * 0.10f + fine * 0.04f + rise * 0.35f;
                    height = Mathf.Clamp(height, 0.02f, 0.68f);

                    height = FlattenGameplayAnchor(height, worldX, worldZ, VillageCenter, 30f);
                    height = FlattenGameplayAnchor(height, worldX, worldZ, PlayerStart, 30f);
                    height = FlattenGameplayAnchor(height, worldX, worldZ, CrystalPos, 30f);
                    height = FlattenGameplayAnchor(height, worldX, worldZ, BossArenaPos, 30f);
                    heights[y, x] = height;
                }
            }

            return heights;
        }

        private static float FlattenGameplayAnchor(float height, float worldX, float worldZ, Vector3 anchor, float radius)
        {
            float dist = Vector2.Distance(new Vector2(worldX, worldZ), new Vector2(anchor.x, anchor.z));
            if (dist >= radius)
            {
                return height;
            }

            float t = Mathf.SmoothStep(0f, 1f, dist / radius);
            return Mathf.Lerp(0.15f, height, t);
        }

        private static TerrainLayer[] BuildTerrainLayers()
        {
            return new[]
            {
                MakeTerrainLayer("Assets/Viking Village/Textures/Terrain/terrain_grass_01_a.tif"),
                MakeTerrainLayer("Assets/Viking Village/Textures/Terrain/terrain_mudslide_01_a.tif"),
                MakeTerrainLayer("Assets/Viking Village/Textures/Terrain/terrain_wetmud_01_a.tif")
            };
        }

        private static TerrainLayer MakeTerrainLayer(string texturePath)
        {
            var layer = new TerrainLayer
            {
                diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath),
                tileSize = new Vector2(8f, 8f)
            };
            return layer;
        }

        private static float[,,] BuildTerrainAlphamaps(TerrainData data)
        {
            int width = data.alphamapWidth;
            int height = data.alphamapHeight;
            var maps = new float[width, height, 3];
            for (int y = 0; y < height; y++)
            {
                float nz = y / (float)(height - 1);
                for (int x = 0; x < width; x++)
                {
                    float nx = x / (float)(width - 1);
                    float steepness = data.GetSteepness(nx, nz);
                    float terrainHeight = data.GetInterpolatedHeight(nx, nz);

                    float dirt = Mathf.InverseLerp(8f, 24f, steepness);
                    float mud = Mathf.InverseLerp(18f, 34f, steepness);
                    float lowGrass = Mathf.InverseLerp(5.5f, 1.5f, terrainHeight);
                    float grass = Mathf.Clamp01(1f - dirt * 0.75f - mud * 0.65f + lowGrass * 0.25f);
                    dirt = Mathf.Clamp01(dirt * (1f - mud * 0.55f));
                    mud = Mathf.Clamp01(mud);

                    float total = grass + dirt + mud;
                    if (total < 0.001f)
                    {
                        grass = 1f;
                        total = 1f;
                    }

                    maps[x, y, 0] = grass / total;
                    maps[x, y, 1] = dirt / total;
                    maps[x, y, 2] = mud / total;
                }
            }

            return maps;
        }

        private static Material FindProjectTerrainMaterial()
        {
            string[] guids = AssetDatabase.FindAssets("t:Material Terrain", new[] { "Assets" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null && mat.shader != null && mat.shader.name.Contains("Terrain"))
                {
                    return mat;
                }
            }

            return null;
        }

        private static Vector3 GroundedPosition(Vector3 position)
        {
            return new Vector3(position.x, SampleGroundHeight(position.x, position.z) + position.y, position.z);
        }

        private static float SampleGroundHeight(float x, float z)
        {
            Terrain terrain = _activeTerrain != null ? _activeTerrain : Terrain.activeTerrain;
            if (terrain == null)
            {
                return 0f;
            }

            return terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y;
        }

        // ---------------------------------------------------------------- forest

        private static void BuildForest()
        {
            string[] treeNames =
            {
                "tree_pineTallA",
                "tree_pineTallB",
                "tree_pineTallC",
                "tree_pineTallD",
                "tree_pineDefaultA",
                "tree_pineDefaultB",
                "tree_pineRoundA",
                "tree_pineRoundB",
                "tree_pineRoundC",
                "tree_pineRoundD",
                "tree_oak",
                "tree_detailed",
                "tree_fat",
                "tree_tall",
                "tree_thin"
            };

            List<GameObject> trees = LoadKenneyModels(treeNames);
            if (trees.Count == 0)
            {
                Debug.LogWarning("[GrauwaldSceneBuilder] No Kenney tree models found in " + KenneyNatureKitDir);
                return;
            }

            var forest = new GameObject("Forest");
            var rng = new System.Random(1337);
            int placed = 0;

            int attempts = 0;
            while (placed < 900 && attempts < 5000)
            {
                attempts++;
                float x = (float)(rng.NextDouble() * 380.0 - 190.0);
                float z = (float)(rng.NextDouble() * 380.0 - 190.0);
                var pos = new Vector3(x, 0f, z);

                // Keep clearings only where the story needs open space; forest fills the rest,
                // including the whole stretch between the wake spot and the village.
                if (Near(pos, PlayerStart, 7f)) continue;   // small wake clearing
                if (Near(pos, VillageCenter, 32f)) continue; // village
                if (Near(pos, CrystalPos, 11f)) continue;    // crystal clearing
                if (Near(pos, BossArenaPos, 16f)) continue;  // boss arena

                GameObject prefab = trees[rng.Next(trees.Count)];
                float yaw = (float)(rng.NextDouble() * 360.0);
                bool tallPine = prefab.name.IndexOf("pineTall", System.StringComparison.OrdinalIgnoreCase) >= 0;
                float minHeight = tallPine ? 8f : 6f;
                float maxHeight = tallPine ? 13f : 11f;
                float targetHeight = RandomRange(rng, minHeight, maxHeight);
                GameObject tree = InstantiateFittedKenneyPrefab(prefab, forest.transform, pos, yaw, targetHeight, 1f, out float fittedHeight);
                foreach (Renderer r in tree.GetComponentsInChildren<Renderer>(true))
                {
                    Material[] mats = r.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        if (mats[i] != null)
                        {
                            GothicTint(mats[i]);
                        }
                    }
                }

                AddTrunkCollider(tree, fittedHeight);
                placed++;
            }

            Debug.Log($"[GrauwaldSceneBuilder] Placed {placed} trees.");
        }

        private static void BuildUndergrowth()
        {
            string[] propNames =
            {
                "grass", "grass", "grass", "grass", "grass", "grass",
                "grass_large", "grass_large", "grass_large", "grass_large", "grass_large",
                "grass_leafs", "grass_leafs", "grass_leafs", "grass_leafs", "grass_leafs", "grass_leafs", "grass_leafs",
                "plant_bush", "plant_bush",
                "plant_bushDetailed", "plant_bushDetailed",
                "plant_bushLarge", "plant_bushLarge",
                "plant_bushSmall", "plant_bushSmall",
                "mushroom_redGroup",
                "mushroom_tanGroup",
                "stump_round",
                "stump_old",
                "log",
                "rock",
                "rock_smallA",
                "rock_smallB",
                "rock_largeA",
                "rock_largeB"
            };

            List<GameObject> props = LoadKenneyModels(propNames);
            if (props.Count == 0)
            {
                Debug.LogWarning("[GrauwaldSceneBuilder] No Kenney undergrowth models found in " + KenneyNatureKitDir);
                return;
            }

            var undergrowth = new GameObject("Undergrowth");
            var rng = new System.Random(4242);
            int placed = 0;
            int attempts = 0;

            while (placed < 1400 && attempts < 8000)
            {
                attempts++;
                float x = (float)(rng.NextDouble() * 380.0 - 190.0);
                float z = (float)(rng.NextDouble() * 380.0 - 190.0);
                var pos = new Vector3(x, 0f, z);

                if (Near(pos, PlayerStart, 4f)) continue;
                if (Near(pos, VillageCenter, 30f)) continue;
                if (Near(pos, CrystalPos, 6f)) continue;
                if (Near(pos, BossArenaPos, 12f)) continue;

                GameObject prefab = props[rng.Next(props.Count)];
                float yaw = (float)(rng.NextDouble() * 360.0);
                float targetHeight = RandomUndergrowthHeight(prefab.name, rng);
                float extraScale = RandomRange(rng, 0.8f, 1.5f);
                GameObject prop = InstantiateFittedKenneyPrefab(prefab, undergrowth.transform, pos, yaw, targetHeight, extraScale, out _);
                foreach (Renderer r in prop.GetComponentsInChildren<Renderer>(true))
                {
                    Material[] mats = r.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        if (mats[i] != null)
                        {
                            GothicTint(mats[i]);
                        }
                    }
                }

                if (ShouldAddUndergrowthCollider(prefab.name))
                {
                    AddBoundsCollider(prop);
                }

                placed++;
            }

            Debug.Log($"[GrauwaldSceneBuilder] Placed {placed} undergrowth props.");
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

        private static List<GameObject> LoadKenneyModels(string[] names)
        {
            var list = new List<GameObject>();
            for (int i = 0; i < names.Length; i++)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{KenneyNatureKitDir}/{names[i]}.fbx");
                if (prefab != null)
                {
                    list.Add(prefab);
                }
            }

            return list;
        }

        private static GameObject InstantiateFittedKenneyPrefab(
            GameObject prefab,
            Transform parent,
            Vector3 position,
            float yaw,
            float targetHeight,
            float extraScale,
            out float fittedHeight)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.position = GroundedPosition(position);
            instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            fittedHeight = targetHeight;

            if (TryGetCombinedRendererBounds(instance, out Bounds bounds) && bounds.size.y > 0.001f)
            {
                float baseScale = targetHeight / bounds.size.y;
                float scale = baseScale * extraScale;
                instance.transform.localScale = Vector3.one * scale;
                if (TryGetCombinedRendererBounds(instance, out Bounds scaledBounds))
                {
                    fittedHeight = scaledBounds.size.y;
                }
            }

            UrpFixMaterials(instance);
            return instance;
        }

        private static bool TryGetCombinedRendererBounds(GameObject root, out Bounds bounds)
        {
            bounds = new Bounds(root.transform.position, Vector3.zero);
            bool hasBounds = false;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (!hasBounds)
                {
                    bounds = renderers[i].bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            return hasBounds;
        }

        private static float RandomRange(System.Random rng, float min, float max)
        {
            return min + (float)rng.NextDouble() * (max - min);
        }

        private static float RandomUndergrowthHeight(string name, System.Random rng)
        {
            if (name.IndexOf("grass", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return RandomRange(rng, 0.4f, 0.8f);
            }

            if (name.IndexOf("bush", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return RandomRange(rng, 0.8f, 1.6f);
            }

            if (name.IndexOf("stump", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("log", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return RandomRange(rng, 0.6f, 1.2f);
            }

            if (name.IndexOf("rock", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return RandomRange(rng, 0.5f, 1.4f);
            }

            return RandomRange(rng, 0.25f, 0.6f);
        }

        private static bool ShouldAddUndergrowthCollider(string name)
        {
            return name.IndexOf("stump", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("log", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("rock", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void GothicTint(Material m)
        {
            Color original = Color.white;
            if (m.HasProperty("_BaseColor"))
            {
                original = m.GetColor("_BaseColor");
            }
            else if (m.HasProperty("_Color"))
            {
                original = m.GetColor("_Color");
            }

            float luminance = original.r * 0.2126f + original.g * 0.7152f + original.b * 0.0722f;
            var grayscale = new Color(luminance, luminance, luminance, original.a);
            Color tinted = Color.Lerp(original, grayscale, GothicDesaturate) * GothicDarken;
            tinted.a = original.a;

            m.SetColor("_BaseColor", tinted);
            if (m.HasProperty("_Smoothness"))
            {
                m.SetFloat("_Smoothness", 0.05f);
            }
        }

        private static Material MakeMat(string path, Color color, float smoothness)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", 0f);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mat, path);
                return mat;
            }

            existing.CopyPropertiesFromMaterial(mat);
            return existing;
        }

        /// <summary>
        /// Builds a foliage material: URP Lit with an alpha-cutout leaf texture and
        /// double-sided rendering, so flat leaf cards read as leafy clumps instead of
        /// solid triangles.
        /// </summary>
        private static Material MakeLeafMat(string path, string texturePath, Color tint)
        {
            // Ensure the leaf texture's alpha is read as transparency for cutout.
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer != null && (!importer.alphaIsTransparency || importer.alphaSource != TextureImporterAlphaSource.FromInput))
            {
                importer.alphaIsTransparency = true;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.SaveAndReimport();
            }

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = tint };
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (tex != null)
            {
                mat.SetTexture("_BaseMap", tex);
            }

            // Alpha clipping (cutout) + double-sided.
            mat.SetFloat("_AlphaClip", 1f);
            mat.SetFloat("_Cutoff", 0.45f);
            mat.EnableKeyword("_ALPHATEST_ON");
            mat.SetFloat("_Cull", 0f); // render both faces
            mat.SetFloat("_Smoothness", 0.08f);
            mat.SetFloat("_Metallic", 0f);
            mat.renderQueue = 2450; // AlphaTest

            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mat, path);
                return mat;
            }

            existing.CopyPropertiesFromMaterial(mat);
            return existing;
        }

        private static void ApplyTreeMaterials(GameObject tree, Material bark, Material leaf)
        {
            foreach (Renderer r in tree.GetComponentsInChildren<Renderer>())
            {
                Material[] src = r.sharedMaterials;
                var mats = new Material[src.Length];
                for (int i = 0; i < src.Length; i++)
                {
                    // The imported FBX names slots "*_Leaf" / "*_Bark"; match on that so
                    // foliage always gets the cutout leaf material regardless of slot order.
                    string name = src[i] != null ? src[i].name : string.Empty;
                    bool isLeaf = name.IndexOf("leaf", System.StringComparison.OrdinalIgnoreCase) >= 0;
                    mats[i] = isLeaf ? leaf : bark;
                }

                r.sharedMaterials = mats;
            }
        }

        private static void AddTrunkCollider(GameObject tree, float worldHeight)
        {
            float scale = Mathf.Abs(tree.transform.localScale.x);
            float inv = scale > 0.001f ? 1f / scale : 1f;
            var col = tree.AddComponent<CapsuleCollider>();
            col.radius = 0.4f * inv;
            col.height = Mathf.Max(worldHeight * inv, col.radius * 2f);
            col.center = new Vector3(0f, worldHeight * 0.5f * inv, 0f);
        }

        private static void AddBoundsCollider(GameObject root)
        {
            if (!TryGetCombinedRendererBounds(root, out Bounds bounds))
            {
                return;
            }

            float scale = Mathf.Abs(root.transform.localScale.x);
            float inv = scale > 0.001f ? 1f / scale : 1f;
            var col = root.AddComponent<BoxCollider>();
            col.center = root.transform.InverseTransformPoint(bounds.center);
            col.size = bounds.size * inv;
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
                    house.transform.position = GroundedPosition(pos);
                    house.transform.rotation = Quaternion.LookRotation(VillageCenter - pos);
                }
                else
                {
                    // Fallback block so the village exists even if the pack is missing.
                    var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    block.transform.SetParent(parent);
                    block.transform.position = GroundedPosition(pos + Vector3.up * 2f);
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
            fire.transform.position = GroundedPosition(VillageCenter + Vector3.up * 4f);
            var light = fire.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.45f, 0.15f);
            light.intensity = 6f;
            light.range = 45f;

            // Fire + smoke + embers on each burning building.
            foreach (Transform child in parent)
            {
                if (!child.name.StartsWith("pf_build") && !child.name.StartsWith("Cube"))
                {
                    continue;
                }

                Vector3 top = child.position + Vector3.up * 4f;
                var fx = new GameObject("BurnFX");
                fx.transform.SetParent(parent);
                fx.transform.position = top;
                VfxFactory.BuildFire(fx.transform);
                VfxFactory.BuildSmoke(fx.transform);
                VfxFactory.BuildEmbers(fx.transform);
            }
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
            root.transform.position = GroundedPosition(PlayerStart);

            var cc = root.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            // Visible rigged body (Blink human) with a locomotion animator.
            AnimatorController locomotion = CharacterAnimatorFactory.BuildLocomotionController();
            GameObject body = InstantiateCharacter(root.transform, locomotion, out Animator bodyAnimator);

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

            // Corruption penalty: overusing aether shrinks max HP.
            var corruption = root.AddComponent<CorruptionEffect>();
            SetRef(corruption, "_aetherPool", aetherPool);
            SetRef(corruption, "_vitals", vitals);

            // Aether spark particles on the hand.
            GameObject handParticles = VfxFactory.BuildHandGlow(handGo.transform, new Color(0.4f, 0.9f, 1f));
            var ps = handParticles.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                SetRef(glow, "_handParticles", ps);
            }

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

            // Drive the rigged body's animator from movement state.
            if (bodyAnimator != null)
            {
                var animCtrl = root.AddComponent<PlayerAnimationController>();
                SetRef(animCtrl, "_animator", bodyAnimator);
                SetRef(animCtrl, "_movementController", movement);
            }

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
            var go = new GameObject("AetherCrystal");
            go.transform.position = GroundedPosition(CrystalPos);
            go.transform.rotation = Quaternion.Euler(0f, 30f, 0f);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                color = new Color(0.2f, 0.7f, 0.85f)
            };
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.3f, 1.4f, 1.7f));
            mat.SetFloat("_Smoothness", 0.85f);
            AssetDatabase.CreateAsset(mat, "Assets/Settings/CrystalMaterial.asset");

            // Faceted crystal-cluster model (replaces the placeholder cube).
            var crystalModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Environment/Props/AetherCrystal.fbx");
            if (crystalModel != null)
            {
                var model = (GameObject)PrefabUtility.InstantiatePrefab(crystalModel, go.transform);
                model.name = "Model";
                model.transform.localPosition = Vector3.zero;
                foreach (Renderer r in model.GetComponentsInChildren<Renderer>())
                {
                    r.sharedMaterial = mat;
                }
            }

            // Interaction collider sized to the cluster.
            var col = go.AddComponent<CapsuleCollider>();
            col.height = 3f;
            col.radius = 0.8f;
            col.center = new Vector3(0f, 1.5f, 0f);

            var glowLight = new GameObject("CrystalLight");
            glowLight.transform.SetParent(go.transform, false);
            var l = glowLight.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.3f, 0.9f, 1f);
            l.range = 14f;
            l.intensity = 4f;

            var crystal = go.AddComponent<AetherCrystal>();
            SetRef(crystal, "_aetherPool", pool);

            // Aether shimmer particles rising from the crystal.
            VfxFactory.BuildAetherShimmer(go.transform, new Color(0.35f, 0.9f, 1f));

            // Aether hum ambience localized to the crystal.
            go.AddComponent<Ashenveil.Audio.AetherAmbienceZone>();
            return crystal;
        }

        // ---------------------------------------------------------------- boss

        private static (MutatedWolfBossController, BossArenaTrigger) BuildBoss(PlayerRig rig)
        {
            var bossGo = new GameObject("MutatedWolf");
            bossGo.transform.position = GroundedPosition(BossArenaPos + Vector3.up * 0.2f);
            var cc = bossGo.AddComponent<CharacterController>();
            cc.height = 1.4f;
            cc.radius = 0.6f;
            cc.center = new Vector3(0f, 0.8f, 0f);

            // Mutated wolf model, scaled up and tinted sickly aether-purple.
            GameObject model = AttachAnimalModel(bossGo.transform, "Wolf");
            model.transform.localScale = model.transform.localScale * 1.8f;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                color = new Color(0.22f, 0.14f, 0.26f)
            };
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.18f, 0.05f, 0.28f));
            foreach (Renderer r in model.GetComponentsInChildren<Renderer>())
            {
                r.sharedMaterial = mat;
            }

            var boss = bossGo.AddComponent<MutatedWolfBossController>();

            var arenaGo = new GameObject("BossArena");
            arenaGo.transform.position = GroundedPosition(BossArenaPos);
            var trigger = arenaGo.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 14f;
            var arena = arenaGo.AddComponent<BossArenaTrigger>();
            SetRef(arena, "_boss", boss);

            // Chase + damage the player (damageable resolved from the target at runtime).
            SetRef(boss, "_target", rig.Root.transform);

            return (boss, arena);
        }

        // ---------------------------------------------------------------- wildlife

        private const string AnimalDir = "Assets/Environment/Animals";

        private static void BuildWildlife(GrauwaldContentFactory.Content content, Transform threat)
        {
            WildlifeAgent deerPrefab = BuildWildlifeAgentPrefab("Deer");
            WildlifeAgent boarPrefab = BuildWildlifeAgentPrefab("Boar");

            // Along the forest walk from the wake spot (z=-130) up toward the village.
            SpawnHerd("DeerSpawner", content.Deer, deerPrefab, threat, new Vector3(18f, 0f, -95f), 3);
            SpawnHerd("BoarSpawner", content.Boar, boarPrefab, threat, new Vector3(-22f, 0f, -60f), 2);
        }

        /// <summary>Instantiates a low-poly animal model under a parent, or a capsule fallback.</summary>
        private static GameObject AttachAnimalModel(Transform parent, string species)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{AnimalDir}/{species}.fbx");
            if (prefab == null)
            {
                var cap = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                cap.transform.SetParent(parent, false);
                Object.DestroyImmediate(cap.GetComponent<Collider>());
                return cap;
            }

            var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            model.name = "Model";
            model.transform.localPosition = Vector3.zero;
            float s = model.transform.localScale.x; // FBX unit-compensation
            UrpFixMaterials(model);
            return model;
        }

        private static WildlifeAgent BuildWildlifeAgentPrefab(string species)
        {
            var go = new GameObject($"WildlifeAgent_{species}");
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.1f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.55f, 0f);
            go.AddComponent<WildlifeHealth>();
            var agent = go.AddComponent<WildlifeAgent>();

            AttachAnimalModel(go.transform, species);

            string prefabPath = $"Assets/ScriptableObjects/Wildlife/WildlifeAgent_{species}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<WildlifeAgent>();
        }

        private static void SpawnHerd(string name, WildlifeSpecies species, WildlifeAgent agentPrefab, Transform threat, Vector3 center, int count)
        {
            var spawnerGo = new GameObject(name);
            spawnerGo.transform.position = GroundedPosition(center);
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
                p.transform.position = GroundedPosition(center + new Vector3(i * 4f - count * 2f, 0f, 0f));
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

            // Menus: main menu, pause (ESC), death/respawn.
            var mainMenu = NewChild(uiRoot, "MainMenu").AddComponent<MainMenuController>();
            var pauseMenu = NewChild(uiRoot, "PauseMenu").AddComponent<PauseMenuController>();
            var deathScreen = NewChild(uiRoot, "DeathScreen").AddComponent<DeathScreenController>();

            var pauseInput = rig.Root.AddComponent<PauseInput>();

            var menuBoot = NewChild(uiRoot, "MenuBootstrapper").AddComponent<MenuBootstrapper>();
            SetRef(menuBoot, "_mainMenu", mainMenu);
            SetRef(menuBoot, "_pauseMenu", pauseMenu);
            SetRef(menuBoot, "_pauseInput", pauseInput);

            var gameOver = NewChild(uiRoot, "GameOver").AddComponent<GameOverController>();
            SetRef(gameOver, "_vitals", rig.Vitals);
            SetRef(gameOver, "_deathScreen", deathScreen);

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

        private static List<GameServices.QuestItemLink> BuildQuestItems(GrauwaldContentFactory.Content content, PlayerInventory inventory)
        {
            var root = new GameObject("QuestItems");
            var rng = new System.Random(909);

            // 5 Blutmoos herbs scattered along the forest walk (z between -120 and -30).
            for (int i = 0; i < 5; i++)
            {
                float x = (float)(rng.NextDouble() * 60.0 - 30.0);
                float z = -120f + i * 20f;
                MakePickup(root.transform, "Herb_" + i, content.Herb, 1, new Vector3(x, 0f, z),
                    new Color(0.5f, 0.1f, 0.2f), "Assets/Environment/Props/HerbPlant.fbx");
            }

            // The smith's lost hammer near the rocks by the crystal path.
            MakePickup(root.transform, "Hammer", content.Hammer, 1, new Vector3(-14f, 0.4f, 40f),
                new Color(0.4f, 0.3f, 0.2f), null);

            return new List<GameServices.QuestItemLink>
            {
                new GameServices.QuestItemLink { Item = content.Herb, QuestId = "quest_herbs", ObjectiveId = "collect_herbs" },
                new GameServices.QuestItemLink { Item = content.Hammer, QuestId = "quest_tool", ObjectiveId = "find_hammer" }
            };
        }

        private static void MakePickup(Transform parent, string name, ItemDefinition item, int qty, Vector3 pos, Color color, string modelPath)
        {
            var go = new GameObject("Pickup_" + name);
            go.name = "Pickup_" + name;
            go.transform.SetParent(parent, false);
            go.transform.position = GroundedPosition(pos);

            var trigger = go.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.5f;
            trigger.center = new Vector3(0f, 0.4f, 0f);

            GameObject prefab = string.IsNullOrEmpty(modelPath) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (prefab != null)
            {
                var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, go.transform);
                model.name = "Model";
                model.transform.localPosition = Vector3.zero;
                UrpFixMaterials(model);
            }
            else
            {
                var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.name = "Model";
                visual.transform.SetParent(go.transform, false);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = Vector3.one * 0.5f;
                Object.DestroyImmediate(visual.GetComponent<Collider>());

                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 1.5f);
                visual.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }

            var pickup = go.AddComponent<ItemPickup>();
            SetPickupItem(pickup, item, qty);
        }

        private static void SetPickupItem(ItemPickup pickup, ItemDefinition item, int qty)
        {
            var so = new SerializedObject(pickup);
            SerializedProperty arr = so.FindProperty("_items");
            arr.arraySize = 1;
            SerializedProperty e = arr.GetArrayElementAtIndex(0);
            e.FindPropertyRelative("_item").objectReferenceValue = item;
            e.FindPropertyRelative("_quantity").intValue = qty;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildNpcs(GrauwaldContentFactory.Content content, PlayerRig rig, UiRefs ui, DemoDirector director, List<GameServices.QuestItemLink> questLinks)
        {
            var speakers = new List<Object>();
            var vendors = new List<Object>();

            DialogSpeaker healer = BuildSpeaker("Heilerin", content.HealerDialog, VillageCenter + new Vector3(6f, 0f, 4f),
                new Color(0.75f, 0.9f, 0.8f));
            DialogSpeaker smith = BuildSpeaker("Schmied", content.SmithDialog, VillageCenter + new Vector3(-6f, 0f, 4f),
                new Color(0.7f, 0.55f, 0.5f), 1.08f);
            speakers.Add(healer);
            speakers.Add(smith);

            VendorController vendor = BuildVendor(content, rig.Inventory, VillageCenter + new Vector3(0f, 0f, 8f),
                new Color(0.7f, 0.75f, 0.9f));
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
            SetQuestItemLinks(services, questLinks);
        }

        private static void SetQuestItemLinks(Object target, List<GameServices.QuestItemLink> links)
        {
            var so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty("_questItemLinks");
            prop.arraySize = links.Count;
            for (int i = 0; i < links.Count; i++)
            {
                SerializedProperty e = prop.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("Item").objectReferenceValue = links[i].Item;
                e.FindPropertyRelative("QuestId").stringValue = links[i].QuestId;
                e.FindPropertyRelative("ObjectiveId").stringValue = links[i].ObjectiveId;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject BuildNpcRoot(string name, Vector3 pos, Vector3 facePos, Color? tint = null, float bodyScale = 1f)
        {
            var go = new GameObject(name);
            go.transform.position = GroundedPosition(pos);
            go.transform.rotation = Quaternion.LookRotation(Flatten(facePos - pos));

            // Interaction + physical presence collider on the root.
            var col = go.AddComponent<CapsuleCollider>();
            col.radius = 0.4f;
            col.height = 1.8f;
            col.center = new Vector3(0f, 0.9f, 0f);

            // Rigged body idling (reuses the player's locomotion controller at Speed 0).
            GameObject body = InstantiateCharacter(go.transform, CharacterAnimatorFactory.LoadLocomotionController(), out _, tint);
            body.transform.localScale = body.transform.localScale * bodyScale;
            return go;
        }

        private static DialogSpeaker BuildSpeaker(string npcName, DialogGraph graph, Vector3 pos, Color? tint = null, float bodyScale = 1f)
        {
            GameObject go = BuildNpcRoot("NPC_" + npcName, pos, VillageCenter, tint, bodyScale);
            var speaker = go.AddComponent<DialogSpeaker>();
            SetRef(speaker, "_dialogGraph", graph);
            SetString(speaker, "_npcDisplayName", npcName);
            return speaker;
        }

        private static VendorController BuildVendor(GrauwaldContentFactory.Content content, PlayerInventory inventory, Vector3 pos, Color? tint = null)
        {
            GameObject go = BuildNpcRoot("NPC_Vendor", pos, VillageCenter, tint);
            var vendor = go.AddComponent<VendorController>();
            SetRef(vendor, "_vendorDefinition", content.Vendor);
            SetRef(vendor, "_playerInventory", inventory);
            return vendor;
        }

        private static Vector3 Flatten(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude < 1e-4f ? Vector3.forward : v.normalized;
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

        private static Shader _urpLitCache;

        /// <summary>
        /// Rebuilds a character's materials as URP/Lit, preserving the albedo texture and
        /// tint. Blink ships built-in/HDRP materials that render magenta under URP; this
        /// keeps the look without a full project-wide material upgrade.
        /// </summary>
        private static void UrpFixMaterials(GameObject root)
        {
            if (_urpLitCache == null)
            {
                _urpLitCache = Shader.Find("Universal Render Pipeline/Lit");
            }

            var cache = new Dictionary<Material, Material>();
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] src = r.sharedMaterials;
                var dst = new Material[src.Length];
                for (int i = 0; i < src.Length; i++)
                {
                    Material m = src[i];
                    if (m == null)
                    {
                        continue;
                    }

                    if (m.shader != null && m.shader.name == "Universal Render Pipeline/Lit")
                    {
                        dst[i] = m;
                        continue;
                    }

                    if (!cache.TryGetValue(m, out Material converted))
                    {
                        converted = new Material(_urpLitCache);
                        Texture main = m.HasProperty("_MainTex") ? m.GetTexture("_MainTex") : m.mainTexture;
                        if (main == null && m.HasProperty("_BaseMap"))
                        {
                            main = m.GetTexture("_BaseMap");
                        }

                        if (main != null)
                        {
                            converted.SetTexture("_BaseMap", main);
                        }

                        if (m.HasProperty("_Color"))
                        {
                            converted.SetColor("_BaseColor", m.GetColor("_Color"));
                        }

                        converted.SetFloat("_Smoothness", 0.25f);
                        cache[m] = converted;
                    }

                    dst[i] = converted;
                }

                r.sharedMaterials = dst;
            }
        }

        private static void ApplyCharacterTint(GameObject root, Color tint)
        {
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] src = r.sharedMaterials;
                var dst = new Material[src.Length];
                for (int i = 0; i < src.Length; i++)
                {
                    Material m = src[i];
                    if (m == null)
                    {
                        continue;
                    }

                    var clone = new Material(m);
                    Color baseColor = Color.white;
                    if (clone.HasProperty("_BaseColor"))
                    {
                        baseColor = clone.GetColor("_BaseColor");
                    }
                    else if (clone.HasProperty("_Color"))
                    {
                        baseColor = clone.GetColor("_Color");
                    }
                    var tinted = new Color(
                        baseColor.r * tint.r,
                        baseColor.g * tint.g,
                        baseColor.b * tint.b,
                        baseColor.a);

                    if (clone.HasProperty("_BaseColor"))
                    {
                        clone.SetColor("_BaseColor", tinted);
                    }

                    if (clone.HasProperty("_Color"))
                    {
                        clone.SetColor("_Color", tinted);
                    }

                    dst[i] = clone;
                }

                r.sharedMaterials = dst;
            }
        }

        private const string CharacterPrefab =
            "Assets/Blink/Art/Characters/Stylized/Humans/Prefabs_Humans/HumanMale_Character_Free.prefab";

        /// <summary>
        /// Instantiates the Blink human under a parent, assigns a locomotion controller,
        /// and returns the body plus its Animator. Falls back to a capsule if the prefab
        /// is missing so scene builds never break.
        /// </summary>
        private static GameObject InstantiateCharacter(Transform parent, AnimatorController controller, out Animator animator, Color? tint = null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPrefab);
            if (prefab == null)
            {
                var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                capsule.name = "Body";
                capsule.transform.SetParent(parent, false);
                capsule.transform.localPosition = new Vector3(0f, 0.9f, 0f);
                Object.DestroyImmediate(capsule.GetComponent<Collider>());
                animator = null;
                if (tint.HasValue)
                {
                    ApplyCharacterTint(capsule, tint.Value);
                }

                return capsule;
            }

            var body = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            body.name = "Body";
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;

            animator = body.GetComponent<Animator>();
            if (animator == null)
            {
                animator = body.GetComponentInChildren<Animator>();
            }

            if (animator != null && controller != null)
            {
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
            }

            UrpFixMaterials(body);
            if (tint.HasValue)
            {
                ApplyCharacterTint(body, tint.Value);
            }

            return body;
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
