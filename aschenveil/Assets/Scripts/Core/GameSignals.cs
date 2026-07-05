using System;
using UnityEngine;

namespace Ashenveil.Core
{
    /// <summary>
    /// Lightweight typed signal hub for cross-system notifications.
    /// Referenced GDD section: Kernsysteme.
    /// </summary>
    public static class GameSignals
    {
        /// <summary>
        /// Raised when an animal dies. Args: animal id, world position.
        /// </summary>
        public static event Action<string, Vector3> AnimalKilled;

        /// <summary>
        /// Raised when an item is added through loot. Args: item definition, amount.
        /// </summary>
        public static event Action<ItemDefinition, int> ItemLooted;

        /// <summary>
        /// Raised when a vendor transaction completes. Args: item, amount, gold delta, was purchase.
        /// </summary>
        public static event Action<ItemDefinition, int, int, bool> VendorTransaction;

        /// <summary>
        /// Raised when a dialog conversation finishes. Args: dialog id.
        /// </summary>
        public static event Action<string> DialogFinished;

        /// <summary>
        /// Raised when a quest state changes. Args: quest id, state id.
        /// </summary>
        public static event Action<string, string> QuestStateChanged;

        /// <summary>
        /// Raised when the player touches aether. Args: world position, amount.
        /// </summary>
        public static event Action<Vector3, float> AetherTouched;

        /// <summary>
        /// Raised when a boss is defeated. Args: boss id.
        /// </summary>
        public static event Action<string> BossDefeated;

        /// <summary>
        /// Raised when the demo flow phase changes. Args: previous phase, next phase.
        /// </summary>
        public static event Action<int, int> PhaseChanged;

        /// <summary>
        /// Notifies listeners that an animal died.
        /// </summary>
        public static void RaiseAnimalKilled(string animalId, Vector3 worldPosition)
        {
            AnimalKilled?.Invoke(animalId, worldPosition);
        }

        /// <summary>
        /// Notifies listeners that loot entered the player's inventory.
        /// </summary>
        public static void RaiseItemLooted(ItemDefinition item, int amount)
        {
            ItemLooted?.Invoke(item, amount);
        }

        /// <summary>
        /// Notifies listeners that a buy or sell action completed.
        /// </summary>
        public static void RaiseVendorTransaction(ItemDefinition item, int amount, int goldDelta, bool wasPurchase)
        {
            VendorTransaction?.Invoke(item, amount, goldDelta, wasPurchase);
        }

        /// <summary>
        /// Notifies listeners that a dialog ended.
        /// </summary>
        public static void RaiseDialogFinished(string dialogId)
        {
            DialogFinished?.Invoke(dialogId);
        }

        /// <summary>
        /// Notifies listeners that a quest changed state.
        /// </summary>
        public static void RaiseQuestStateChanged(string questId, string stateId)
        {
            QuestStateChanged?.Invoke(questId, stateId);
        }

        /// <summary>
        /// Notifies listeners that aether was touched or collected.
        /// </summary>
        public static void RaiseAetherTouched(Vector3 worldPosition, float amount)
        {
            AetherTouched?.Invoke(worldPosition, amount);
        }

        /// <summary>
        /// Notifies listeners that a boss died.
        /// </summary>
        public static void RaiseBossDefeated(string bossId)
        {
            BossDefeated?.Invoke(bossId);
        }

        /// <summary>
        /// Notifies listeners that the demo flow phase changed.
        /// </summary>
        public static void RaisePhaseChanged(int previousPhase, int nextPhase)
        {
            PhaseChanged?.Invoke(previousPhase, nextPhase);
        }
    }
}
