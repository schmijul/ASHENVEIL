using System.Collections.Generic;
using Ashenveil.Flow;
using NUnit.Framework;

namespace Ashenveil.Tests.EditMode
{
    public sealed class DemoFlowModelTests
    {
        [Test]
        public void AdvanceFrom_OnlyAdvancesWhenExpectedMatches()
        {
            var flow = new DemoFlowModel();
            Assert.IsFalse(flow.AdvanceFrom(DemoPhase.Hunt)); // not current
            Assert.AreEqual(DemoPhase.WakeInForest, flow.Phase);

            Assert.IsTrue(flow.AdvanceFrom(DemoPhase.WakeInForest));
            Assert.AreEqual(DemoPhase.Hunt, flow.Phase);
        }

        [Test]
        public void AdvanceFrom_IsIdempotentAgainstRepeatedSignals()
        {
            var flow = new DemoFlowModel();
            var seen = new List<DemoPhase>();
            flow.PhaseChanged += (_, next) => seen.Add(next);

            Assert.IsTrue(flow.AdvanceFrom(DemoPhase.WakeInForest));
            Assert.IsFalse(flow.AdvanceFrom(DemoPhase.WakeInForest)); // duplicate milestone
            Assert.AreEqual(1, seen.Count);
        }

        [Test]
        public void FullSequence_ReachesEscapeChoice()
        {
            var flow = new DemoFlowModel();
            Assert.IsTrue(flow.AdvanceFrom(DemoPhase.WakeInForest));
            Assert.IsTrue(flow.AdvanceFrom(DemoPhase.Hunt));
            Assert.IsTrue(flow.AdvanceFrom(DemoPhase.EnterVillage));
            Assert.IsTrue(flow.AdvanceFrom(DemoPhase.VillageQuests));
            Assert.IsTrue(flow.AdvanceFrom(DemoPhase.DeepForestCrystal));
            Assert.IsTrue(flow.AdvanceFrom(DemoPhase.BossFight));
            Assert.IsTrue(flow.AdvanceFrom(DemoPhase.BurningVillage));
            Assert.AreEqual(DemoPhase.EscapeChoice, flow.Phase);
            Assert.IsFalse(flow.AdvanceFrom(DemoPhase.EscapeChoice)); // terminal
        }

        [Test]
        public void JumpTo_MovesForwardOnly()
        {
            var flow = new DemoFlowModel();
            Assert.IsTrue(flow.JumpTo(DemoPhase.EnterVillage));
            Assert.AreEqual(DemoPhase.EnterVillage, flow.Phase);
            Assert.IsFalse(flow.JumpTo(DemoPhase.Hunt)); // backward rejected
            Assert.IsFalse(flow.JumpTo(DemoPhase.EnterVillage)); // same rejected
        }

        [Test]
        public void ChooseEscape_OnlyValidInFinalPhaseAndEndsOnce()
        {
            var flow = new DemoFlowModel();
            Assert.IsFalse(flow.ChooseEscape(EscapeDirection.Kernwall)); // too early

            flow.JumpTo(DemoPhase.EscapeChoice);
            int endedCount = 0;
            EscapeDirection? chosen = null;
            flow.DemoEnded += d => { endedCount++; chosen = d; };

            Assert.IsTrue(flow.ChooseEscape(EscapeDirection.Flimmermoor));
            Assert.IsTrue(flow.IsFinished);
            Assert.AreEqual(EscapeDirection.Flimmermoor, chosen);

            Assert.IsFalse(flow.ChooseEscape(EscapeDirection.Hohensang)); // already ended
            Assert.AreEqual(1, endedCount);
        }
    }
}
