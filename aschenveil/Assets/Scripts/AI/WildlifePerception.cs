namespace Ashenveil.AI
{
    /// <summary>
    /// Immutable perception snapshot for the wildlife runtime state machine.
    /// </summary>
    public struct WildlifePerception
    {
        public bool PlayerDetected;
        public float PlayerDistance;
        public bool IsPlayerBehindTarget;
        public bool IsInsideHomeRadius;
        public bool HasLineOfSight;
        public float HealthRatio;

        public static WildlifePerception CreateDefault()
        {
            return new WildlifePerception
            {
                PlayerDetected = false,
                PlayerDistance = float.PositiveInfinity,
                IsPlayerBehindTarget = false,
                IsInsideHomeRadius = true,
                HasLineOfSight = false,
                HealthRatio = 1f
            };
        }
    }
}
