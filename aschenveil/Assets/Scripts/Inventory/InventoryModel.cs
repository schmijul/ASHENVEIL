using System;
using System.Collections.Generic;
using Ashenveil.Core;

namespace Ashenveil.Inventory
{
    /// <summary>
    /// Deterministic slot-less inventory model with stack limits, carry weight, and gold.
    /// Referenced GDD section: Kernsysteme / Inventar &amp; Handel.
    /// </summary>
    public sealed class InventoryModel
    {
        private const float WeightTolerance = 0.0001f;

        private readonly List<InventoryStack> _stacks = new List<InventoryStack>();

        private int _gold;

        public InventoryModel(float maxCarryWeight, int initialGold = 0)
        {
            MaxCarryWeight = Math.Max(0f, maxCarryWeight);
            _gold = Math.Max(0, initialGold);
        }

        public event Action Changed;

        public IReadOnlyList<InventoryStack> Stacks => _stacks;

        public float MaxCarryWeight { get; }

        public int Gold => _gold;

        public float TotalWeight
        {
            get
            {
                float total = 0f;
                for (int i = 0; i < _stacks.Count; i++)
                {
                    total += _stacks[i].Item.Weight * _stacks[i].Quantity;
                }

                return total;
            }
        }

        public int GetQuantity(ItemDefinition item)
        {
            if (item == null)
            {
                return 0;
            }

            int quantity = 0;
            for (int i = 0; i < _stacks.Count; i++)
            {
                if (_stacks[i].Item == item)
                {
                    quantity += _stacks[i].Quantity;
                }
            }

            return quantity;
        }

        public bool CanAdd(ItemDefinition item, int quantity)
        {
            if (item == null || quantity <= 0)
            {
                return false;
            }

            float addedWeight = item.Weight * quantity;
            return TotalWeight + addedWeight <= MaxCarryWeight + WeightTolerance;
        }

        public bool Add(ItemDefinition item, int quantity)
        {
            if (!CanAdd(item, quantity))
            {
                return false;
            }

            int remaining = quantity;
            int stackLimit = item.MaxStack;

            for (int i = 0; i < _stacks.Count && remaining > 0; i++)
            {
                InventoryStack stack = _stacks[i];
                if (stack.Item != item || stack.Quantity >= stackLimit)
                {
                    continue;
                }

                int added = Math.Min(stackLimit - stack.Quantity, remaining);
                _stacks[i] = new InventoryStack(item, stack.Quantity + added);
                remaining -= added;
            }

            while (remaining > 0)
            {
                int stackQuantity = Math.Min(stackLimit, remaining);
                _stacks.Add(new InventoryStack(item, stackQuantity));
                remaining -= stackQuantity;
            }

            RaiseChanged();
            return true;
        }

        public bool Remove(ItemDefinition item, int quantity)
        {
            if (item == null || quantity <= 0 || GetQuantity(item) < quantity)
            {
                return false;
            }

            int remaining = quantity;
            for (int i = _stacks.Count - 1; i >= 0 && remaining > 0; i--)
            {
                InventoryStack stack = _stacks[i];
                if (stack.Item != item)
                {
                    continue;
                }

                if (stack.Quantity <= remaining)
                {
                    remaining -= stack.Quantity;
                    _stacks.RemoveAt(i);
                }
                else
                {
                    _stacks[i] = new InventoryStack(item, stack.Quantity - remaining);
                    remaining = 0;
                }
            }

            RaiseChanged();
            return true;
        }

        public bool EarnGold(int amount)
        {
            if (amount < 0)
            {
                return false;
            }

            _gold += amount;
            RaiseChanged();
            return true;
        }

        public bool SpendGold(int amount)
        {
            if (amount < 0 || amount > _gold)
            {
                return false;
            }

            _gold -= amount;
            RaiseChanged();
            return true;
        }

        private void RaiseChanged()
        {
            Changed?.Invoke();
        }
    }
}
