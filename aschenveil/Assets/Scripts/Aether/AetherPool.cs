using System;
using UnityEngine;

namespace Ashenveil.Aether
{
    /// <summary>
    /// Scene-level owner of the player's single <see cref="AetherModel"/> and
    /// <see cref="CorruptionModel"/> so multiple adapters (crystal, glow, combat bridge,
    /// HUD) share one reserve without singletons or scene lookups. Wired by the scene
    /// builder and handed to interested components via serialized references.
    /// Referenced GDD section: Kernsysteme / Äther.
    /// </summary>
    public sealed class AetherPool : MonoBehaviour
    {
        [Header("Charge")]
        [SerializeField] private float _maxCharge = 100f;
        [SerializeField] private float _passiveDecayPerSecond = 1f;
        [SerializeField] private float _corruptionPerChargeSpent = 1f;

        [Header("Corruption thresholds")]
        [SerializeField] private float[] _corruptionThresholds = { 25f, 50f, 75f, 100f };
        [SerializeField] private float[] _maxHealthPenaltyPerStage = { 0.1f, 0.2f, 0.35f, 0.5f };

        private AetherModel _model;
        private CorruptionModel _corruption;

        /// <summary>
        /// Shared aether model. Never null after Awake.
        /// </summary>
        public AetherModel Model
        {
            get
            {
                EnsureInitialized();
                return _model;
            }
        }

        /// <summary>
        /// Shared corruption model. Never null after Awake.
        /// </summary>
        public CorruptionModel Corruption
        {
            get
            {
                EnsureInitialized();
                return _corruption;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void Update()
        {
            _model?.Tick(Time.deltaTime);
        }

        private void EnsureInitialized()
        {
            if (_model != null)
            {
                return;
            }

            _corruption = new CorruptionModel(_corruptionThresholds, _maxHealthPenaltyPerStage);
            _model = new AetherModel(_maxCharge, _passiveDecayPerSecond, _corruptionPerChargeSpent, _corruption);
        }
    }
}
