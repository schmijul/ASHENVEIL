using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ashenveil.AI.Boss;
using Ashenveil.Aether;
using Ashenveil.Combat;
using Ashenveil.Flow;
using Ashenveil.Player;
using Ashenveil.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ashenveil.World.Editor
{
    /// <summary>
    /// Opens the built Grauwald scene and asserts that critical serialized references are
    /// wired (a wrong SerializedProperty field name would silently leave them null). Logs
    /// a PASS/FAIL summary and sets a non-zero exit via failure count. Run with
    /// -executeMethod Ashenveil.World.Editor.GrauwaldSceneValidator.Validate.
    /// </summary>
    public static class GrauwaldSceneValidator
    {
        private static readonly List<string> Failures = new List<string>();

        public static void Validate()
        {
            Failures.Clear();
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Grauwald.unity", OpenSceneMode.Single);

            var player = Find<PlayerMovementController>();
            RequireRef(player, "_inputConfig");
            RequireRef(player, "_cameraTransform");
            RequireRef(player, "_vitals");

            var combat = Find<CombatController>();
            RequireRef(combat, "_weaponDefinition");
            RequireRef(combat, "_weaponHitbox");
            RequireRef(combat, "_movementController");

            var bridge = Find<AetherCombatBridge>();
            RequireRef(bridge, "_aetherPool");
            RequireRef(bridge, "_combatController");

            var crystal = Find<AetherCrystal>();
            RequireRef(crystal, "_aetherPool");

            var director = Find<DemoDirector>();
            RequireRef(director, "_hud");
            RequireRef(director, "_fade");
            RequireRef(director, "_endScreen");
            RequireRef(director, "_villageNormalRoot");
            RequireRef(director, "_villageBurningRoot");

            var services = Find<GameServices>();
            RequireRef(services, "_playerInventory");
            RequireRef(services, "_dialogScreen");
            RequireRef(services, "_tradeScreen");
            RequireList(services, "_quests", 2);
            RequireList(services, "_speakers", 2);
            RequireList(services, "_vendors", 1);

            var bossBar = Find<BossBarController>();
            RequireRef(bossBar, "_boss");
            RequireRef(bossBar, "_arenaTrigger");

            var arena = Find<BossArenaTrigger>();
            RequireRef(arena, "_boss");

            var hud = Find<HudController>();
            RequireRef(hud, "_vitals");
            RequireRef(hud, "_aetherPool");

            // Population sanity.
            RequireCount<MutatedWolfBossController>(1);
            RequireCount<Ashenveil.Trade.VendorController>(1);
            RequireCount<Ashenveil.Dialog.DialogSpeaker>(2);
            RequireCount<Ashenveil.AI.WildlifeSpawner>(2);

            if (Failures.Count == 0)
            {
                Debug.Log("[GrauwaldSceneValidator] PASS — all critical references wired.");
            }
            else
            {
                Debug.LogError($"[GrauwaldSceneValidator] FAIL — {Failures.Count} problem(s):\n - " + string.Join("\n - ", Failures));
                EditorApplication.Exit(1);
            }
        }

        private static T Find<T>() where T : Component
        {
            var found = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
            if (found.Length == 0)
            {
                Failures.Add($"missing component {typeof(T).Name}");
                return null;
            }

            return found[0];
        }

        private static void RequireCount<T>(int expected) where T : Component
        {
            int n = Object.FindObjectsByType<T>(FindObjectsSortMode.None).Length;
            if (n < expected)
            {
                Failures.Add($"{typeof(T).Name}: expected >= {expected}, found {n}");
            }
        }

        private static void RequireRef(Component c, string field)
        {
            if (c == null)
            {
                return;
            }

            var so = new SerializedObject(c);
            SerializedProperty p = so.FindProperty(field);
            if (p == null)
            {
                Failures.Add($"{c.GetType().Name}.{field}: no such serialized field");
            }
            else if (p.objectReferenceValue == null)
            {
                Failures.Add($"{c.GetType().Name}.{field}: null reference");
            }
        }

        private static void RequireList(Component c, string field, int minCount)
        {
            if (c == null)
            {
                return;
            }

            var so = new SerializedObject(c);
            SerializedProperty p = so.FindProperty(field);
            if (p == null || !p.isArray)
            {
                Failures.Add($"{c.GetType().Name}.{field}: not an array");
            }
            else if (p.arraySize < minCount)
            {
                Failures.Add($"{c.GetType().Name}.{field}: expected >= {minCount}, found {p.arraySize}");
            }
        }
    }
}
