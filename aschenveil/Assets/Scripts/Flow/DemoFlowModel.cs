using System;

namespace Ashenveil.Flow
{
    /// <summary>
    /// Pure state machine sequencing the eight demo phases. It only advances forward and
    /// only one phase at a time, so gameplay systems (hunt, trade, crystal, boss, escape)
    /// can request advancement when their milestone is met and the model guarantees a
    /// single, ordered progression. Referenced GDD section: Demo-Ablauf.
    /// </summary>
    public sealed class DemoFlowModel
    {
        private DemoPhase _phase = DemoPhase.WakeInForest;
        private EscapeDirection? _chosenDirection;
        private bool _finished;

        /// <summary>
        /// Raised when the phase advances. Args: previous phase, next phase.
        /// </summary>
        public event Action<DemoPhase, DemoPhase> PhaseChanged;

        /// <summary>
        /// Raised when the player picks an escape direction and the demo ends.
        /// </summary>
        public event Action<EscapeDirection> DemoEnded;

        /// <summary>
        /// Current phase.
        /// </summary>
        public DemoPhase Phase => _phase;

        /// <summary>
        /// Whether the demo has ended.
        /// </summary>
        public bool IsFinished => _finished;

        /// <summary>
        /// The chosen escape direction, if any.
        /// </summary>
        public EscapeDirection? ChosenDirection => _chosenDirection;

        /// <summary>
        /// Advances to the next phase if <paramref name="expected"/> is the current phase.
        /// Guards against double-advancing from repeated milestone signals.
        /// </summary>
        /// <returns>True if the phase advanced.</returns>
        public bool AdvanceFrom(DemoPhase expected)
        {
            if (_finished || _phase != expected || _phase == DemoPhase.EscapeChoice)
            {
                return false;
            }

            DemoPhase previous = _phase;
            _phase = (DemoPhase)((int)_phase + 1);
            PhaseChanged?.Invoke(previous, _phase);
            return true;
        }

        /// <summary>
        /// Unconditionally advances to a later phase (never backward). Used when a player
        /// skips ahead (e.g. reaches the village before finishing the hunt tutorial).
        /// </summary>
        /// <returns>True if the phase advanced.</returns>
        public bool JumpTo(DemoPhase target)
        {
            if (_finished || (int)target <= (int)_phase)
            {
                return false;
            }

            DemoPhase previous = _phase;
            _phase = target;
            PhaseChanged?.Invoke(previous, _phase);
            return true;
        }

        /// <summary>
        /// Records the escape direction, ending the demo. Only valid in the final phase.
        /// </summary>
        /// <returns>True if the choice was accepted.</returns>
        public bool ChooseEscape(EscapeDirection direction)
        {
            if (_finished || _phase != DemoPhase.EscapeChoice)
            {
                return false;
            }

            _chosenDirection = direction;
            _finished = true;
            DemoEnded?.Invoke(direction);
            return true;
        }
    }
}
