using System.Collections.Generic;
using Ashenveil.AI;
using Ashenveil.Combat;
using Ashenveil.Data;
using Ashenveil.Dialog;
using Ashenveil.Inventory;
using Ashenveil.NPC;
using Ashenveil.Player;
using Ashenveil.UI;
using UnityEngine;

namespace Ashenveil.World
{
    /// <summary>
    /// Creates the minimum integrated opening loop content inside Grauwald for playtesting.
    /// Referenced GDD sections: 2.3, 5.10, 5.11
    /// </summary>
    public class OpeningLoopBootstrapper : MonoBehaviour
    {
        private const string WildlifeRootName = "Opening Loop Wildlife";
        private const string NpcRootName = "Opening Loop NPCs";
        private const string HitboxName = "PlayerWeaponHitbox";

        [Header("References")]
        [SerializeField] private Transform _playerRoot;
        [SerializeField] private Terrain _terrain;

        [Header("Spawn Points")]
        [SerializeField] private Vector3 _boarSpawnPoint = new Vector3(172f, 0f, 189f);
        [SerializeField] private Vector3 _traderSpawnPoint = new Vector3(352f, 0f, 166f);
        [SerializeField] private Vector3 _elderSpawnPoint = new Vector3(338f, 0f, 182f);

        [Header("Interaction")]
        [SerializeField, Min(0.5f)] private float _interactionRadius = 2.5f;

        private readonly Dictionary<string, ItemData> _itemsById = new Dictionary<string, ItemData>(System.StringComparer.OrdinalIgnoreCase);

        private PlayerInventoryRuntime _inventory;
        private PlayerInteractionController _interactionController;

        private void Awake()
        {
            ResolveReferences();
            EnsureRuntimeItems();
            EnsurePlayerLoop();
            EnsureVillageNpcs();
            EnsureWildlifeEncounter();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            if (_playerRoot == null)
            {
                Transform player = transform.Find("Player");
                if (player != null)
                {
                    _playerRoot = player;
                }
            }

            if (_terrain == null)
            {
                Transform environment = transform.Find("Environment");
                if (environment != null)
                {
                    _terrain = environment.GetComponentInChildren<Terrain>(true);
                }
            }
        }

        private void EnsureRuntimeItems()
        {
            RegisterItem("Boar Pelt", ItemType.Material, 10, 10, 5);
            RegisterItem("Boar Meat", ItemType.Material, 10, 6, 3);
            RegisterItem("Boar Tusk", ItemType.Material, 5, 16, 8);
        }

        private void EnsurePlayerLoop()
        {
            if (_playerRoot == null)
            {
                Debug.LogError($"{nameof(OpeningLoopBootstrapper)} on {name} requires a player root.", this);
                enabled = false;
                return;
            }

            CharacterController characterController = GetOrAddComponent<CharacterController>(_playerRoot.gameObject);
            characterController.detectCollisions = true;

            WeaponHitbox hitbox = EnsureWeaponHitbox();
            if (hitbox == null)
            {
                Debug.LogError($"{nameof(OpeningLoopBootstrapper)} could not create a weapon hitbox for {_playerRoot.name}.", this);
                enabled = false;
                return;
            }

            CombatController combatController = GetOrAddComponent<CombatController>(_playerRoot.gameObject);
            combatController.RefreshRuntimeReferences();
            GetOrAddComponent<PlayerCombatInputDriver>(_playerRoot.gameObject);
            _inventory = GetOrAddComponent<PlayerInventoryRuntime>(_playerRoot.gameObject);
            _interactionController = GetOrAddComponent<PlayerInteractionController>(_playerRoot.gameObject);
            OpeningLoopDebugHud hud = GetOrAddComponent<OpeningLoopDebugHud>(_playerRoot.gameObject);
            _interactionController.ConfigureInteractionRadius(_interactionRadius);

            hud.enabled = true;
            combatController.enabled = true;
            _interactionController.enabled = true;
        }

        private WeaponHitbox EnsureWeaponHitbox()
        {
            Transform existing = _playerRoot.Find(HitboxName);
            GameObject hitboxObject = existing != null ? existing.gameObject : new GameObject(HitboxName);
            if (existing == null)
            {
                hitboxObject.transform.SetParent(_playerRoot, false);
                hitboxObject.transform.localPosition = new Vector3(0f, 1f, 1f);
                hitboxObject.transform.localRotation = Quaternion.identity;
            }

            SphereCollider sphereCollider = GetOrAddComponent<SphereCollider>(hitboxObject);
            sphereCollider.isTrigger = true;
            sphereCollider.radius = 0.7f;

            Rigidbody rigidbody = GetOrAddComponent<Rigidbody>(hitboxObject);
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            return GetOrAddComponent<WeaponHitbox>(hitboxObject);
        }

        private void EnsureVillageNpcs()
        {
            Transform npcRoot = EnsureRootChild(NpcRootName);
            EnsureNpc(npcRoot, "Trader", _traderSpawnPoint, CreateTraderData());
            EnsureNpc(npcRoot, "Elder", _elderSpawnPoint, CreateElderData());
        }

        private void EnsureWildlifeEncounter()
        {
            Transform wildlifeRoot = EnsureRootChild(WildlifeRootName);
            Transform boarTransform = wildlifeRoot.Find("BoarEncounter");
            if (boarTransform != null)
            {
                return;
            }

            GameObject boar = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            boar.name = "BoarEncounter";
            boar.transform.SetParent(wildlifeRoot, false);
            boar.transform.position = SampleTerrainPosition(_boarSpawnPoint, 0.9f);
            boar.transform.localScale = new Vector3(1f, 0.7f, 1.4f);

            WildlifeHealth health = GetOrAddComponent<WildlifeHealth>(boar);
            WildlifeController controller = GetOrAddComponent<WildlifeController>(boar);
            controller.SetPlayerTarget(_playerRoot);
            controller.LootDropped += HandleLootDropped;

            if (health.Profile == null)
            {
                // WildlifeHealth falls back to boar defaults, so forcing a reset is enough here.
                health.ResetToFullHealth();
            }
        }

        private void EnsureNpc(Transform parent, string npcName, Vector3 spawnPoint, NPCData npcData)
        {
            Transform existing = parent.Find(npcName);
            if (existing != null)
            {
                return;
            }

            GameObject npcObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npcObject.name = npcName;
            npcObject.transform.SetParent(parent, false);
            npcObject.transform.position = SampleTerrainPosition(spawnPoint, 0.9f);

            CapsuleCollider collider = npcObject.GetComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 0.9f, 0f);
            collider.height = 1.8f;
            collider.radius = 0.35f;

            NpcController controller = GetOrAddComponent<NpcController>(npcObject);
            controller.Initialize(npcData);
        }

        private void HandleLootDropped(IReadOnlyList<WildlifeLootResult> lootResults, Vector3 origin)
        {
            if (lootResults == null)
            {
                return;
            }

            for (int index = 0; index < lootResults.Count; index++)
            {
                WildlifeLootResult lootResult = lootResults[index];
                if (!_itemsById.TryGetValue(lootResult.ItemId, out ItemData item))
                {
                    continue;
                }

                GameObject pickupObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pickupObject.name = $"Loot_{lootResult.ItemId}_{index}";
                pickupObject.transform.SetParent(transform, true);
                pickupObject.transform.position = origin + new Vector3((index - 0.5f) * 0.6f, 0.35f, 0.4f);
                pickupObject.transform.localScale = Vector3.one * 0.3f;

                InventoryPickup pickup = GetOrAddComponent<InventoryPickup>(pickupObject);
                pickup.Configure(item, Mathf.Max(1, lootResult.Amount));
            }
        }

        private void RegisterItem(string itemId, ItemType itemType, int maxStack, int buyPrice, int sellPrice)
        {
            if (_itemsById.ContainsKey(itemId))
            {
                return;
            }

            ItemData item = ScriptableObject.CreateInstance<ItemData>();
            item.hideFlags = HideFlags.HideAndDontSave;
            item.name = itemId.Replace(' ', '_');
            item.itemName = itemId;
            item.type = itemType;
            item.maxStack = maxStack;
            item.buyPrice = buyPrice;
            item.sellPrice = sellPrice;
            _itemsById[itemId] = item;
        }

        private NPCData CreateTraderData()
        {
            DialogTree dialogTree = DialogTree.CreateRuntimeDefaults();
            dialogTree.SetNodes(new[]
            {
                DialogNode.Create(
                    0,
                    "Trader",
                    "Welcome! Looking to buy or sell?",
                    -1,
                    false,
                    null,
                    DialogChoice.Create("Show me what you have.", 1),
                    DialogChoice.Create("Just passing through.", 2)),
                DialogNode.Create(1, "Trader", "Trade is the next integration step. For now, bring me pelts and meat once the loop is stable.", nextNode: -1),
                DialogNode.Create(2, "Trader", "Come back once you've brought down that boar.", nextNode: -1)
            });

            NPCData npcData = NPCData.CreateRuntimeDefaults();
            npcData.SetIdentity("Trader", "Village Trader", true);
            npcData.SetDialogTree(dialogTree);
            return npcData;
        }

        private NPCData CreateElderData()
        {
            DialogTree dialogTree = DialogTree.CreateRuntimeDefaults();
            dialogTree.SetNodes(new[]
            {
                DialogNode.Create(0, "Elder", "The hunt starts south of the village. Bring back proof and we'll speak again.", nextNode: 1),
                DialogNode.Create(1, "Elder", "Movement, combat, loot, and village interaction are wired now. The rest of the prolog still needs real quest flow.", nextNode: -1)
            });

            NPCData npcData = NPCData.CreateRuntimeDefaults();
            npcData.SetIdentity("Elder", "Village Elder", true);
            npcData.SetDialogTree(dialogTree);
            return npcData;
        }

        private Vector3 SampleTerrainPosition(Vector3 worldPosition, float heightOffset)
        {
            if (_terrain == null)
            {
                return worldPosition;
            }

            float sampledHeight = _terrain.SampleHeight(worldPosition) + _terrain.transform.position.y;
            return new Vector3(worldPosition.x, sampledHeight + heightOffset, worldPosition.z);
        }

        private Transform EnsureRootChild(string childName)
        {
            Transform existing = transform.Find(childName);
            if (existing != null)
            {
                return existing;
            }

            GameObject child = new GameObject(childName);
            child.transform.SetParent(transform, false);
            return child.transform;
        }

        private static T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            if (!target.TryGetComponent(out T component))
            {
                component = target.AddComponent<T>();
            }

            return component;
        }
    }
}
