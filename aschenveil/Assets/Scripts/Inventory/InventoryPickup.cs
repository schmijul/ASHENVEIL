using Ashenveil.Data;
using UnityEngine;

namespace Ashenveil.Inventory
{
    /// <summary>
    /// World pickup that visually rotates and bobs while exposing item collection.
    /// </summary>
    public class InventoryPickup : MonoBehaviour
    {
        [Header("Item")]
        [SerializeField] private ItemData _item;
        [SerializeField] private int _quantity = 1;

        [Header("Motion")]
        [SerializeField] private float _rotationSpeed = 30f;
        [SerializeField] private float _bobAmplitude = 0.1f;
        [SerializeField] private float _bobPeriod = 1f;

        [Header("Pickup")]
        [SerializeField] private bool _destroyOnPickup = true;

        private Vector3 _basePosition;
        private bool _hasBasePosition;

        public ItemData Item => _item;

        public int Quantity => _quantity;

        public string PickupPrompt => _item != null ? $"F: Pick Up [{_item.DisplayName}]" : "F: Pick Up";

        private void Awake()
        {
            if (_item == null)
            {
                Debug.LogError($"{nameof(InventoryPickup)} on {name} is missing an item reference.", this);
            }

            _basePosition = transform.position;
            _hasBasePosition = true;
        }

        private void OnValidate()
        {
            if (_quantity < 1)
            {
                _quantity = 1;
            }

            if (_rotationSpeed < 0f)
            {
                _rotationSpeed = 0f;
            }

            if (_bobAmplitude < 0f)
            {
                _bobAmplitude = 0f;
            }

            if (_bobPeriod <= 0f)
            {
                _bobPeriod = 1f;
            }
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World);

            if (!_hasBasePosition || _bobPeriod <= Mathf.Epsilon)
            {
                return;
            }

            float cycle = Time.time * Mathf.PI * 2f / _bobPeriod;
            float bobOffset = Mathf.Sin(cycle) * _bobAmplitude;
            transform.position = _basePosition + Vector3.up * bobOffset;
        }

        public bool TryCollect(InventoryModel inventory)
        {
            if (inventory == null || _item == null || _quantity <= 0)
            {
                return false;
            }

            bool collected = inventory.TryAddItem(_item, _quantity);
            if (!collected || !_destroyOnPickup)
            {
                return collected;
            }

            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }

            return true;
        }
    }
}
