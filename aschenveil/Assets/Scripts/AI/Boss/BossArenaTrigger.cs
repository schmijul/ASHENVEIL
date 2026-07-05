using System;
using UnityEngine;

namespace Ashenveil.AI.Boss
{
    /// <summary>
    /// Trigger volume that activates the boss when the player enters the deep-forest
    /// arena. Raises <see cref="BossEncounterStarted"/> for UI/flow wiring.
    /// Referenced GDD section: Demo-Ablauf Phase 6.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class BossArenaTrigger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MutatedWolfBossController _boss;

        [Header("Filter")]
        [SerializeField] private string _playerTag = "Player";

        private bool _fired;

        /// <summary>
        /// Raised once when the player first enters the arena. Args: boss controller.
        /// </summary>
        public event Action<MutatedWolfBossController> BossEncounterStarted;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_fired || _boss == null || !other.CompareTag(_playerTag))
            {
                return;
            }

            _fired = true;
            _boss.Activate();
            BossEncounterStarted?.Invoke(_boss);
        }
    }
}
