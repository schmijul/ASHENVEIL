using Ashenveil.Core;
using NUnit.Framework;
using UnityEngine;

namespace Ashenveil.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for shared foundation contracts.
    /// </summary>
    public sealed class CoreFoundationTests
    {
        /// <summary>
        /// Verifies damage payload data is preserved.
        /// </summary>
        [Test]
        public void DamageInfo_Constructed_PreservesValues()
        {
            Vector3 source = new Vector3(1f, 2f, 3f);
            DamageInfo damageInfo = new DamageInfo(12f, DamageType.Aether, source, 4f);

            Assert.That(damageInfo.Amount, Is.EqualTo(12f));
            Assert.That(damageInfo.DamageType, Is.EqualTo(DamageType.Aether));
            Assert.That(damageInfo.SourcePosition, Is.EqualTo(source));
            Assert.That(damageInfo.Knockback, Is.EqualTo(4f));
        }

        /// <summary>
        /// Verifies static game signals invoke typed subscribers.
        /// </summary>
        [Test]
        public void GameSignals_PhaseChanged_RaisesTypedEvent()
        {
            int previous = -1;
            int next = -1;

            void OnPhaseChanged(int previousPhase, int nextPhase)
            {
                previous = previousPhase;
                next = nextPhase;
            }

            GameSignals.PhaseChanged += OnPhaseChanged;
            try
            {
                GameSignals.RaisePhaseChanged(2, 3);
            }
            finally
            {
                GameSignals.PhaseChanged -= OnPhaseChanged;
            }

            Assert.That(previous, Is.EqualTo(2));
            Assert.That(next, Is.EqualTo(3));
        }
    }
}
