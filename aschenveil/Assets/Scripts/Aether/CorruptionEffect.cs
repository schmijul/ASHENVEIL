using Ashenveil.Player;
using UnityEngine;

namespace Ashenveil.Aether
{
    /// <summary>
    /// Applies the aether corruption penalty to the player's vitals: as corruption stages
    /// rise from overusing aether, the player's maximum health shrinks. This is the teeth
    /// of the risk/reward mechanic. Referenced GDD section: Kernsysteme / Äther.
    /// </summary>
    public sealed class CorruptionEffect : MonoBehaviour
    {
        [SerializeField] private AetherPool _aetherPool;
        [SerializeField] private PlayerVitals _vitals;

        private CorruptionModel _corruption;

        private void OnEnable()
        {
            if (_aetherPool == null || _vitals == null)
            {
                enabled = false;
                return;
            }

            _corruption = _aetherPool.Corruption;
            _corruption.StageChanged += OnStageChanged;
        }

        private void OnDisable()
        {
            if (_corruption != null)
            {
                _corruption.StageChanged -= OnStageChanged;
            }
        }

        private void OnStageChanged(int stage, float maxHealthPenalty)
        {
            _vitals.SetCorruptionPenalty(maxHealthPenalty);
        }
    }
}
