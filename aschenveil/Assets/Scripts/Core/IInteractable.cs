namespace Ashenveil.Core
{
    /// <summary>
    /// Contract for world objects the player can activate.
    /// Referenced GDD section: Kernsysteme / UI.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// German prompt shown when the player can interact with this object.
        /// </summary>
        string InteractionPrompt { get; }

        /// <summary>
        /// Performs this interaction for the current player.
        /// </summary>
        void Interact(PlayerContext playerContext);
    }
}
