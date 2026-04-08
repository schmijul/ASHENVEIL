using UnityEngine;

namespace Ashenveil.AI
{
    /// <summary>
    /// Random source abstraction so the wildlife runtime state can be tested deterministically.
    /// </summary>
    public interface IWildlifeRandomSource
    {
        float Value();

        float Range(float minInclusive, float maxInclusive);

        int Range(int minInclusive, int maxExclusive);
    }

    /// <summary>
    /// Unity-backed random source for live gameplay.
    /// </summary>
    public sealed class UnityWildlifeRandomSource : IWildlifeRandomSource
    {
        public float Value()
        {
            return Random.value;
        }

        public float Range(float minInclusive, float maxInclusive)
        {
            return Random.Range(minInclusive, maxInclusive);
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            return Random.Range(minInclusive, maxExclusive);
        }
    }
}
