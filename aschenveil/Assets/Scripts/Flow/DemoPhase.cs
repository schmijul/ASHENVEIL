namespace Ashenveil.Flow
{
    /// <summary>
    /// The eight scripted phases of the demo, in order.
    /// Referenced GDD section: Demo-Ablauf (8 Phasen).
    /// </summary>
    public enum DemoPhase
    {
        /// <summary>Phase 1 — waking in the forest, heading toward the village.</summary>
        WakeInForest = 0,

        /// <summary>Phase 2 — hunting wildlife; movement + combat tutorial.</summary>
        Hunt = 1,

        /// <summary>Phase 3 — entering the village; trade + inventory tutorial.</summary>
        EnterVillage = 2,

        /// <summary>Phase 4 — NPC dialogs and side quests.</summary>
        VillageQuests = 3,

        /// <summary>Phase 5 — deep forest; touching the aether crystal.</summary>
        DeepForestCrystal = 4,

        /// <summary>Phase 6 — the mutated wolf boss fight.</summary>
        BossFight = 5,

        /// <summary>Phase 7 — returning to the burning village.</summary>
        BurningVillage = 6,

        /// <summary>Phase 8 — escape and direction choice; demo end.</summary>
        EscapeChoice = 7
    }

    /// <summary>
    /// The three escape directions offered at the end of the demo.
    /// </summary>
    public enum EscapeDirection
    {
        /// <summary>Toward Kernwall.</summary>
        Kernwall,

        /// <summary>Toward Flimmermoor.</summary>
        Flimmermoor,

        /// <summary>Toward Hohensang.</summary>
        Hohensang
    }
}
