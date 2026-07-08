using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.AI
{
    /// <summary>
    /// Serialized loot table entry for one wildlife item.
    /// Referenced GDD section: Kernsysteme / Wildlife.
    /// </summary>
    [System.Serializable]
    public struct WildlifeLootEntry
    {
        [SerializeField] private ItemDefinition _item;
        [SerializeField] private int _min;
        [SerializeField] private int _max;
        [Range(0f, 1f)]
        [SerializeField] private float _chance;

        /// <summary>
        /// Item definition to add when this entry rolls successfully.
        /// </summary>
        public ItemDefinition Item => _item;

        /// <summary>
        /// Minimum amount awarded.
        /// </summary>
        public int Min => Mathf.Max(0, _min);

        /// <summary>
        /// Maximum amount awarded.
        /// </summary>
        public int Max => Mathf.Max(Min, _max);

        /// <summary>
        /// Chance from 0..1 that this entry drops.
        /// </summary>
        public float Chance => Mathf.Clamp01(_chance);
    }
}
