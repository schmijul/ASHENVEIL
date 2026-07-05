using System;
using Ashenveil.Core;
using Ashenveil.Inventory;
using UnityEngine;

namespace Ashenveil.Trade
{
    /// <summary>
    /// Thin interactable adapter that opens and closes a trade model session.
    /// Referenced GDD section: Kernsysteme / Inventar &amp; Handel.
    /// </summary>
    public sealed class VendorController : MonoBehaviour, IInteractable
    {
        [Header("Configuration")]
        [SerializeField] private VendorDefinition _vendorDefinition;

        [Header("References")]
        [SerializeField] private PlayerInventory _playerInventory;

        public event Action<TradeModel> SessionStarted;

        public event Action SessionEnded;

        public string InteractionPrompt => "Handeln (E)";

        public TradeModel ActiveSession { get; private set; }

        private void Awake()
        {
            if (_vendorDefinition == null)
            {
                Debug.LogError("VendorController needs a VendorDefinition.", this);
            }
        }

        public void Interact(PlayerContext playerContext)
        {
            PlayerInventory inventory = ResolveInventory(playerContext);
            if (inventory == null)
            {
                Debug.LogError("VendorController needs a PlayerInventory reference or a PlayerInventory on the interacting player.", this);
                return;
            }

            StartSession(inventory.Model);
        }

        public TradeModel StartSession(InventoryModel playerInventory)
        {
            if (_vendorDefinition == null || playerInventory == null)
            {
                return null;
            }

            ActiveSession = new TradeModel(_vendorDefinition, playerInventory);
            SessionStarted?.Invoke(ActiveSession);
            return ActiveSession;
        }

        public void EndSession()
        {
            if (ActiveSession == null)
            {
                return;
            }

            ActiveSession = null;
            SessionEnded?.Invoke();
        }

        private PlayerInventory ResolveInventory(PlayerContext playerContext)
        {
            if (_playerInventory != null)
            {
                return _playerInventory;
            }

            if (playerContext?.PlayerTransform != null
                && playerContext.PlayerTransform.TryGetComponent(out PlayerInventory inventory))
            {
                return inventory;
            }

            return null;
        }
    }
}
