using Ashenveil.Player;
using UnityEngine;

namespace Ashenveil.Core
{
    /// <summary>
    /// Plain object passed to interactables so they can access player-facing systems without scene lookups.
    /// </summary>
    public sealed class PlayerContext
    {
        /// <summary>
        /// Creates a player context from the systems currently wired on the player.
        /// </summary>
        public PlayerContext(
            Transform playerTransform,
            PlayerMovementController movementController,
            PlayerVitals vitals,
            StaminaModel stamina)
        {
            PlayerTransform = playerTransform;
            MovementController = movementController;
            Vitals = vitals;
            Stamina = stamina;
        }

        /// <summary>
        /// Player transform for position-sensitive interactions.
        /// </summary>
        public Transform PlayerTransform { get; }

        /// <summary>
        /// Player movement adapter, if one is available.
        /// </summary>
        public PlayerMovementController MovementController { get; }

        /// <summary>
        /// Player health and stamina adapter, if one is available.
        /// </summary>
        public PlayerVitals Vitals { get; }

        /// <summary>
        /// Player stamina model, if one is available.
        /// </summary>
        public StaminaModel Stamina { get; }
    }
}
