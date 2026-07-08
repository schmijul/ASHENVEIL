using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.Aether
{
    /// <summary>
    /// A crystallized aether node the player can touch. Grants charge to the shared
    /// <see cref="AetherPool"/>, triggers the first-touch story beat (hand glow), and
    /// raises <see cref="GameSignals.AetherTouched"/>. Referenced GDD section:
    /// Demo-Ablauf Phase 5.
    /// </summary>
    public sealed class AetherCrystal : MonoBehaviour, IInteractable
    {
        [Header("References")]
        [SerializeField] private AetherPool _aetherPool;

        [Header("Absorption")]
        [SerializeField] private float _chargePerTouch = 40f;
        [SerializeField] private bool _rechargeable = true;
        [SerializeField] private float _rechargeSeconds = 8f;

        [Header("VFX anchors")]
        [Tooltip("Optional transforms where the scene builder attaches particle emitters.")]
        [SerializeField] private Transform[] _vfxAnchors;

        private float _depletedUntil = -1f;

        /// <inheritdoc />
        public string InteractionPrompt => IsAvailable ? "Äther berühren (E)" : "Äther erschöpft";

        /// <summary>
        /// Whether the crystal currently holds absorbable charge.
        /// </summary>
        public bool IsAvailable => Time.time >= _depletedUntil;

        /// <summary>
        /// VFX anchor transforms for presentation wiring.
        /// </summary>
        public Transform[] VfxAnchors => _vfxAnchors;

        /// <inheritdoc />
        public void Interact(PlayerContext playerContext)
        {
            if (!IsAvailable || _aetherPool == null)
            {
                return;
            }

            _aetherPool.Model.Absorb(_chargePerTouch);
            GameSignals.RaiseAetherTouched(transform.position, _chargePerTouch);

            _depletedUntil = _rechargeable ? Time.time + _rechargeSeconds : float.MaxValue;
        }
    }
}
