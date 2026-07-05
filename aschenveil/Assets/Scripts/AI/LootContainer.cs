using System.Collections.Generic;
using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.AI
{
    /// <summary>
    /// Interactable container created from rolled wildlife loot.
    /// Referenced GDD section: Kernsysteme / Wildlife.
    /// </summary>
    public sealed class LootContainer : MonoBehaviour, IInteractable
    {
        private readonly List<LootStack> _loot = new List<LootStack>();
        private bool _looted;

        /// <summary>
        /// German prompt shown for this loot container.
        /// </summary>
        public string InteractionPrompt => _looted ? "Leer" : "Plündern (F)";

        /// <summary>
        /// Rolled loot still held by this container.
        /// </summary>
        public IReadOnlyList<LootStack> Loot => _loot;

        /// <summary>
        /// Initializes this container with rolled loot stacks.
        /// </summary>
        public void Initialize(IReadOnlyList<LootStack> loot)
        {
            _loot.Clear();
            if (loot == null)
            {
                return;
            }

            for (int i = 0; i < loot.Count; i++)
            {
                if (loot[i].Item != null && loot[i].Amount > 0)
                {
                    _loot.Add(loot[i]);
                }
            }

            _looted = false;
        }

        /// <summary>
        /// Loots every stack and raises item-looted signals for inventory integration.
        /// </summary>
        public void Interact(PlayerContext playerContext)
        {
            if (_looted)
            {
                return;
            }

            for (int i = 0; i < _loot.Count; i++)
            {
                GameSignals.RaiseItemLooted(_loot[i].Item, _loot[i].Amount);
            }

            _loot.Clear();
            _looted = true;
        }
    }
}
