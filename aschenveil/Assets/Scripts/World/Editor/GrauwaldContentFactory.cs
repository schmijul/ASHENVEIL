using System.Collections.Generic;
using Ashenveil.AI;
using Ashenveil.Combat;
using Ashenveil.Core;
using Ashenveil.Dialog;
using Ashenveil.Quests;
using Ashenveil.Trade;
using UnityEditor;
using UnityEngine;

namespace Ashenveil.World.Editor
{
    /// <summary>
    /// Creates every ScriptableObject the demo needs (items, weapon, wildlife species,
    /// vendor, quests, dialogs) as project assets, authored via SerializedObject so no
    /// runtime Configure methods are required. Returns strongly-typed handles the scene
    /// builder wires into components. Referenced GDD section: Kernsysteme.
    /// </summary>
    public static class GrauwaldContentFactory
    {
        private const string Root = "Assets/ScriptableObjects";

        /// <summary>Bundle of authored content handed back to the scene builder.</summary>
        public sealed class Content
        {
            public ItemDefinition Meat;
            public ItemDefinition Hide;
            public ItemDefinition Herb;
            public ItemDefinition Hammer;
            public ItemDefinition Potion;
            public WeaponDefinition Sword;
            public WildlifeSpecies Deer;
            public WildlifeSpecies Boar;
            public VendorDefinition Vendor;
            public QuestDefinition HerbQuest;
            public QuestDefinition ToolQuest;
            public DialogGraph HealerDialog;
            public DialogGraph SmithDialog;
            public List<QuestDefinition> AllQuests;
        }

        /// <summary>
        /// Builds and saves all content assets, returning handles.
        /// </summary>
        public static Content Build()
        {
            EnsureFolders();
            var c = new Content();

            c.Meat = Item("item_meat", "Rohes Fleisch", "Frisches Wildfleisch.", ItemCategory.Food, 0.5f, 6);
            c.Hide = Item("item_hide", "Tierfell", "Ein robustes Fell.", ItemCategory.Material, 0.8f, 10);
            c.Herb = Item("item_herb", "Blutmoos", "Ein heilkräftiges Moos.", ItemCategory.Material, 0.1f, 4);
            c.Hammer = Item("item_hammer", "Schmiedehammer", "Das verlorene Werkzeug des Schmieds.", ItemCategory.Tool, 2.0f, 0);
            c.Potion = Item("item_potion", "Heiltrank", "Stellt Gesundheit wieder her.", ItemCategory.Food, 0.3f, 25);

            c.Sword = Sword();
            c.Deer = DeerSpecies(c.Meat, c.Hide);
            c.Boar = BoarSpecies(c.Meat, c.Hide);
            c.Vendor = Vendor(c.Potion, c.Meat, c.Hide);

            c.HerbQuest = HerbQuest();
            c.ToolQuest = ToolQuest();
            c.AllQuests = new List<QuestDefinition> { c.HerbQuest, c.ToolQuest };

            c.HealerDialog = HealerDialog(c.HerbQuest.Id);
            c.SmithDialog = SmithDialog(c.ToolQuest.Id);

            AssetDatabase.SaveAssets();
            return c;
        }

        private static void EnsureFolders()
        {
            CreateFolder("Assets", "ScriptableObjects");
            foreach (string sub in new[] { "Items", "Combat", "Wildlife", "Trade", "Quests", "Dialog" })
            {
                CreateFolder(Root, sub);
            }
        }

        private static void CreateFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + name))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static T CreateAsset<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return existing;
            }

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void Set(Object obj, System.Action<SerializedObject> apply)
        {
            var so = new SerializedObject(obj);
            apply(so);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(obj);
        }

        private static ItemDefinition Item(string id, string name, string desc, ItemCategory cat, float weight, int gold)
        {
            var item = CreateAsset<ItemDefinition>($"{Root}/Items/{id}.asset");
            Set(item, so =>
            {
                so.FindProperty("_id").stringValue = id;
                so.FindProperty("_displayName").stringValue = name;
                so.FindProperty("_description").stringValue = desc;
                so.FindProperty("_category").enumValueIndex = (int)cat;
                so.FindProperty("_weight").floatValue = weight;
                so.FindProperty("_goldValue").intValue = gold;
                so.FindProperty("_stackable").boolValue = cat != ItemCategory.Tool;
                so.FindProperty("_maxStack").intValue = cat == ItemCategory.Tool ? 1 : 20;
            });
            return item;
        }

        private static WeaponDefinition Sword()
        {
            var w = CreateAsset<WeaponDefinition>($"{Root}/Combat/weapon_sword.asset");
            Set(w, so =>
            {
                so.FindProperty("_damage").floatValue = 22f;
                so.FindProperty("_damageType").enumValueIndex = (int)DamageType.Physical;
                so.FindProperty("_aetherDamageBonus").floatValue = 20f;
                so.FindProperty("_knockback").floatValue = 3f;
                so.FindProperty("_lightStaminaCost").floatValue = 8f;
                so.FindProperty("_heavyStaminaCost").floatValue = 18f;
                so.FindProperty("_maxCombatStamina").floatValue = 100f;
                so.FindProperty("_lightWindupDuration").floatValue = 0.18f;
                so.FindProperty("_lightActiveDuration").floatValue = 0.12f;
                so.FindProperty("_lightRecoveryDuration").floatValue = 0.3f;
                so.FindProperty("_heavyWindupDuration").floatValue = 0.4f;
                so.FindProperty("_heavyActiveDuration").floatValue = 0.16f;
                so.FindProperty("_heavyRecoveryDuration").floatValue = 0.5f;
                so.FindProperty("_range").floatValue = 2.4f;
                so.FindProperty("_blockDamageReduction").floatValue = 0.6f;
                so.FindProperty("_blockStaminaDrainPerDamage").floatValue = 0.5f;
                so.FindProperty("_comboWindowDuration").floatValue = 0.5f;
            });
            return w;
        }

        private static WildlifeSpecies DeerSpecies(ItemDefinition meat, ItemDefinition hide)
        {
            var s = CreateAsset<WildlifeSpecies>($"{Root}/Wildlife/species_deer.asset");
            Set(s, so =>
            {
                so.FindProperty("_speciesId").enumValueIndex = 0; // Deer
                so.FindProperty("_walkSpeed").floatValue = 2.0f;
                so.FindProperty("_runSpeed").floatValue = 6.5f;
                so.FindProperty("_health").floatValue = 30f;
                so.FindProperty("_temperament").enumValueIndex = 0; // Flee
                SetLoot(so, meat, hide);
            });
            return s;
        }

        private static WildlifeSpecies BoarSpecies(ItemDefinition meat, ItemDefinition hide)
        {
            var s = CreateAsset<WildlifeSpecies>($"{Root}/Wildlife/species_boar.asset");
            Set(s, so =>
            {
                so.FindProperty("_speciesId").enumValueIndex = 2; // Boar
                so.FindProperty("_walkSpeed").floatValue = 1.8f;
                so.FindProperty("_runSpeed").floatValue = 5.5f;
                so.FindProperty("_health").floatValue = 55f;
                so.FindProperty("_temperament").enumValueIndex = 1; // Aggressive
                SetLoot(so, meat, hide);
            });
            return s;
        }

        private static void SetLoot(SerializedObject so, ItemDefinition meat, ItemDefinition hide)
        {
            SerializedProperty loot = so.FindProperty("_lootTable");
            loot.arraySize = 2;
            AssignLoot(loot.GetArrayElementAtIndex(0), meat, 1, 3, 1f);
            AssignLoot(loot.GetArrayElementAtIndex(1), hide, 1, 2, 0.8f);
        }

        private static void AssignLoot(SerializedProperty element, ItemDefinition item, int min, int max, float chance)
        {
            element.FindPropertyRelative("_item").objectReferenceValue = item;
            element.FindPropertyRelative("_min").intValue = min;
            element.FindPropertyRelative("_max").intValue = max;
            element.FindPropertyRelative("_chance").floatValue = chance;
        }

        private static VendorDefinition Vendor(ItemDefinition potion, ItemDefinition meat, ItemDefinition hide)
        {
            var v = CreateAsset<VendorDefinition>($"{Root}/Trade/vendor_trader.asset");
            Set(v, so =>
            {
                so.FindProperty("_vendorName").stringValue = "Händlerin Ingrid";
                so.FindProperty("_goldReserve").intValue = 300;
                so.FindProperty("_buybackMultiplier").floatValue = 0.5f;
                SerializedProperty stock = so.FindProperty("_stock");
                stock.arraySize = 1;
                SerializedProperty entry = stock.GetArrayElementAtIndex(0);
                entry.FindPropertyRelative("_item").objectReferenceValue = potion;
                entry.FindPropertyRelative("_quantity").intValue = 5;
                entry.FindPropertyRelative("_priceMultiplier").floatValue = 1f;
            });
            return v;
        }

        private static QuestDefinition HerbQuest()
        {
            var q = CreateAsset<QuestDefinition>($"{Root}/Quests/quest_herbs.asset");
            var reward = new QuestRewardDefinition(40, new List<QuestRewardItem>());
            var objectives = new List<QuestObjectiveDefinition>
            {
                new QuestObjectiveDefinition("collect_herbs", "Sammle Blutmoos", 5)
            };
            q.Configure("quest_herbs", "Kräuter für die Heilerin",
                "Die Heilerin braucht fünf Büschel Blutmoos aus dem Wald.", objectives, reward);
            EditorUtility.SetDirty(q);
            return q;
        }

        private static QuestDefinition ToolQuest()
        {
            var q = CreateAsset<QuestDefinition>($"{Root}/Quests/quest_tool.asset");
            var reward = new QuestRewardDefinition(30, new List<QuestRewardItem>());
            var objectives = new List<QuestObjectiveDefinition>
            {
                new QuestObjectiveDefinition("find_hammer", "Finde den Schmiedehammer am Fluss", 1)
            };
            q.Configure("quest_tool", "Das verlorene Werkzeug",
                "Der Schmied hat seinen Hammer am Fluss verloren.", objectives, reward);
            EditorUtility.SetDirty(q);
            return q;
        }

        private static DialogGraph HealerDialog(string herbQuestId)
        {
            var graph = CreateAsset<DialogGraph>($"{Root}/Dialog/dialog_healer.asset");
            var start = new DialogNode("start", "Heilerin",
                "Sei gegrüßt, Fremder. Der Wald ist krank... bringst du mir Blutmoos?",
                new List<DialogChoice>
                {
                    MakeChoice("Ich helfe dir. (Auftrag annehmen)", "accept", QuestActionType.StartQuest, herbQuestId),
                    MakeChoice("Vielleicht später.", "end", QuestActionType.None, null)
                });
            var accept = new DialogNode("accept", "Heilerin", "Hab Dank. Fünf Büschel genügen.", new List<DialogChoice>());
            var end = new DialogNode("end", "Heilerin", "Der Äther wartet nicht.", new List<DialogChoice>());
            graph.Configure("dialog_healer", "start", new List<DialogNode> { start, accept, end });
            EditorUtility.SetDirty(graph);
            return graph;
        }

        private static DialogGraph SmithDialog(string toolQuestId)
        {
            var graph = CreateAsset<DialogGraph>($"{Root}/Dialog/dialog_smith.asset");
            var start = new DialogNode("start", "Schmied",
                "Verflucht! Mein Hammer liegt irgendwo am Fluss. Findest du ihn?",
                new List<DialogChoice>
                {
                    MakeChoice("Ich suche ihn. (Auftrag annehmen)", "accept", QuestActionType.StartQuest, toolQuestId),
                    MakeChoice("Keine Zeit.", "end", QuestActionType.None, null)
                });
            var accept = new DialogNode("accept", "Schmied", "Am Ufer, bei den Felsen. Danke dir!", new List<DialogChoice>());
            var end = new DialogNode("end", "Schmied", "Dann eben nicht.", new List<DialogChoice>());
            graph.Configure("dialog_smith", "start", new List<DialogNode> { start, accept, end });
            EditorUtility.SetDirty(graph);
            return graph;
        }

        private static DialogChoice MakeChoice(string text, string target, QuestActionType action, string questId)
        {
            var questAction = action == QuestActionType.None
                ? new DialogQuestAction()
                : new DialogQuestAction(action, questId);
            return new DialogChoice(text, target, questAction, null);
        }
    }
}
