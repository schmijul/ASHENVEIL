namespace Ashenveil.AI
{
    /// <summary>
    /// Deterministic random source abstraction for wildlife logic and loot rolls.
    /// Referenced GDD section: Kernsysteme / Wildlife.
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>
        /// Returns a value in the range 0..1.
        /// </summary>
        float Next01();

        /// <summary>
        /// Returns an integer in the inclusive range.
        /// </summary>
        int RangeInclusive(int min, int max);
    }
}
