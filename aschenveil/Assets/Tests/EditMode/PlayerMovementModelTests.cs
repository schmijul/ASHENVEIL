using Ashenveil.Player;
using NUnit.Framework;
using UnityEngine;

namespace Ashenveil.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for player movement logic.
    /// </summary>
    public sealed class PlayerMovementModelTests
    {
        /// <summary>
        /// Verifies walk, run, and sprint speed selection.
        /// </summary>
        [Test]
        public void Tick_MoveInput_SelectsWalkRunAndSprintSpeeds()
        {
            PlayerMovementModel.Settings settings = PlayerMovementModel.Settings.Default;
            settings.Acceleration = 100f;

            PlayerMovementModel walkModel = new PlayerMovementModel(settings);
            PlayerMovementModel runModel = new PlayerMovementModel(settings);
            PlayerMovementModel sprintModel = new PlayerMovementModel(settings);

            PlayerMovementModel.State walk = walkModel.Tick(new PlayerMovementModel.Input
            {
                MoveDirection = Vector3.forward,
                WalkHeld = true,
                IsGrounded = true
            }, 1f);

            PlayerMovementModel.State run = runModel.Tick(new PlayerMovementModel.Input
            {
                MoveDirection = Vector3.forward,
                IsGrounded = true
            }, 1f);

            PlayerMovementModel.State sprint = sprintModel.Tick(new PlayerMovementModel.Input
            {
                MoveDirection = Vector3.forward,
                SprintHeld = true,
                IsGrounded = true
            }, 1f);

            Assert.That(walk.CurrentSpeed, Is.EqualTo(settings.WalkSpeed).Within(0.001f));
            Assert.That(run.CurrentSpeed, Is.EqualTo(settings.RunSpeed).Within(0.001f));
            Assert.That(sprint.CurrentSpeed, Is.EqualTo(settings.SprintSpeed).Within(0.001f));
        }

        /// <summary>
        /// Verifies jumping is still accepted during coyote time.
        /// </summary>
        [Test]
        public void Tick_JumpPressedDuringCoyoteTime_AppliesUpwardVelocity()
        {
            PlayerMovementModel.Settings settings = PlayerMovementModel.Settings.Default;
            settings.CoyoteTime = 0.12f;
            PlayerMovementModel model = new PlayerMovementModel(settings);

            model.Tick(new PlayerMovementModel.Input
            {
                IsGrounded = true
            }, 0.02f);

            model.Tick(new PlayerMovementModel.Input
            {
                IsGrounded = false
            }, 0.05f);

            PlayerMovementModel.State state = model.Tick(new PlayerMovementModel.Input
            {
                JumpPressed = true,
                IsGrounded = false
            }, 0.01f);

            Assert.That(state.Velocity.y, Is.GreaterThan(0f));
        }

        /// <summary>
        /// Verifies dodge invulnerability is only active inside its configured window.
        /// </summary>
        [Test]
        public void Tick_DodgeRoll_ReportsIFramesOnlyInsideWindow()
        {
            PlayerMovementModel.Settings settings = PlayerMovementModel.Settings.Default;
            settings.DodgeDuration = 0.5f;
            settings.DodgeIFrameStart = 0.1f;
            settings.DodgeIFrameEnd = 0.3f;
            PlayerMovementModel model = new PlayerMovementModel(settings);

            PlayerMovementModel.State start = model.Tick(new PlayerMovementModel.Input
            {
                MoveDirection = Vector3.forward,
                DodgePressed = true,
                IsGrounded = true
            }, 0.05f);

            PlayerMovementModel.State active = model.Tick(new PlayerMovementModel.Input
            {
                MoveDirection = Vector3.forward,
                IsGrounded = true
            }, 0.1f);

            PlayerMovementModel.State expired = model.Tick(new PlayerMovementModel.Input
            {
                MoveDirection = Vector3.forward,
                IsGrounded = true
            }, 0.25f);

            Assert.That(start.IsDodging, Is.True);
            Assert.That(start.HasIFrames, Is.False);
            Assert.That(active.HasIFrames, Is.True);
            Assert.That(expired.HasIFrames, Is.False);
            Assert.That(active.Velocity.z, Is.EqualTo(settings.DodgeSpeed).Within(0.001f));
        }
    }
}
