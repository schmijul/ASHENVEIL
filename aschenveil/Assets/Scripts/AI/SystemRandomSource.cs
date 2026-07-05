using System;

namespace Ashenveil.AI
{
    /// <summary>
    /// System.Random-backed implementation of deterministic wildlife random values.
    /// Referenced GDD section: Kernsysteme / Wildlife.
    /// </summary>
    public sealed class SystemRandomSource : IRandomSource
    {
        private readonly Random _random;

        /// <summary>
        /// Creates a random source with a time-based seed.
        /// </summary>
        public SystemRandomSource()
            : this(Environment.TickCount)
        {
        }

        /// <summary>
        /// Creates a random source with an explicit seed.
        /// </summary>
        public SystemRandomSource(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// Returns a value in the range 0..1.
        /// </summary>
        public float Next01()
        {
            return (float)_random.NextDouble();
        }

        /// <summary>
        /// Returns an integer in the inclusive range.
        /// </summary>
        public int RangeInclusive(int min, int max)
        {
            if (max < min)
            {
                int oldMin = min;
                min = max;
                max = oldMin;
            }

            return _random.Next(min, max + 1);
        }
    }
}
