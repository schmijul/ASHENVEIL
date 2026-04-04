using System.Collections.Generic;
using UnityEngine;

namespace Ashenveil.Combat
{
    /// <summary>
    /// Pure combat timing and state logic for attacks, blocks, dodges, and guard breaks.
    /// </summary>
    public sealed class CombatRuntimeState
    {
        private readonly IReadOnlyList<AttackStepDefinition> _lightAttackCombo;
        private readonly AttackStepDefinition _heavyAttack;
        private readonly BlockSettings _blockSettings;
        private readonly DodgeSettings _dodgeSettings;

        private float _time;
        private float _actionEndsAt;
        private float _comboWindowEndsAt;
        private float _blockStartedAt = -1f;
        private float _dodgeEndsAt = -1f;
        private float _dodgeCooldownEndsAt = -1f;
        private float _guardBreakEndsAt = -1f;
        private int _currentComboStepIndex;
        private CombatActionKind _currentAction = CombatActionKind.Idle;
        private AttackStepDefinition _currentAttackStep;

        public CombatRuntimeState(
            IReadOnlyList<AttackStepDefinition> lightAttackCombo,
            AttackStepDefinition heavyAttack,
            BlockSettings blockSettings,
            DodgeSettings dodgeSettings)
        {
            _lightAttackCombo = lightAttackCombo ?? new AttackStepDefinition[0];
            _heavyAttack = heavyAttack ?? AttackStepDefinition.CreateHeavyAttackStep();
            _blockSettings = blockSettings ?? BlockSettings.CreateDefault();
            _dodgeSettings = dodgeSettings ?? DodgeSettings.CreateDefault();
        }

        public float Time => _time;

        public CombatActionKind CurrentAction => _currentAction;

        public int CurrentComboStepIndex => _currentComboStepIndex;

        public AttackStepDefinition CurrentAttackStep => _currentAttackStep;

        public bool IsAttacking => _currentAction == CombatActionKind.LightAttack || _currentAction == CombatActionKind.HeavyAttack;

        public bool IsBlocking => _currentAction == CombatActionKind.Blocking;

        public bool IsDodging => _currentAction == CombatActionKind.Dodging;

        public bool IsGuardBroken => _currentAction == CombatActionKind.GuardBroken;

        public bool IsComboWindowOpen => _currentComboStepIndex > 0 && _currentAction == CombatActionKind.Idle && _time <= _comboWindowEndsAt;

        public bool CanDodge => _currentAction != CombatActionKind.Dodging && _currentAction != CombatActionKind.GuardBroken && _time >= _dodgeCooldownEndsAt;

        public float ComboWindowRemaining => Mathf.Max(0f, _comboWindowEndsAt - _time);

        public float DodgeCooldownRemaining => Mathf.Max(0f, _dodgeCooldownEndsAt - _time);

        public float BlockHeldDuration => IsBlocking && _blockStartedAt >= 0f ? Mathf.Max(0f, _time - _blockStartedAt) : 0f;

        public bool IsPerfectBlock => IsBlocking && BlockHeldDuration <= _blockSettings.PerfectBlockWindow;

        public float DodgeIFrameRemaining => IsDodging ? Mathf.Max(0f, _dodgeEndsAt - _time) : 0f;

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            _time += deltaTime;

            if (_currentAction == CombatActionKind.LightAttack || _currentAction == CombatActionKind.HeavyAttack)
            {
                if (_time >= _actionEndsAt)
                {
                    CombatActionKind finishedAction = _currentAction;
                    AttackStepDefinition finishedAttackStep = _currentAttackStep;
                    _currentAction = CombatActionKind.Idle;
                    _currentAttackStep = null;
                    _actionEndsAt = _time;

                    if (finishedAction == CombatActionKind.HeavyAttack)
                    {
                        ResetComboInternal();
                    }
                    else
                    {
                        float comboWindow = finishedAttackStep != null ? Mathf.Max(0f, finishedAttackStep.ComboWindow) : 0.6f;
                        _comboWindowEndsAt = _time + comboWindow;
                    }
                }
            }

            if (_currentAction == CombatActionKind.Dodging && _time >= _dodgeEndsAt)
            {
                _currentAction = CombatActionKind.Idle;
                _currentAttackStep = null;
            }

            if (_currentAction == CombatActionKind.GuardBroken && _time >= _guardBreakEndsAt)
            {
                _currentAction = CombatActionKind.Idle;
            }
        }

        public bool TryBeginLightAttack(float currentStamina, out AttackStepDefinition attackStep, out float staminaCost)
        {
            attackStep = null;
            staminaCost = 0f;

            if (!CanStartOffensiveAction())
            {
                return false;
            }

            if (!HasLightCombo())
            {
                return false;
            }

            if (!IsComboWindowOpen)
            {
                _currentComboStepIndex = 0;
            }

            int nextComboStep = _currentComboStepIndex + 1;
            if (nextComboStep > _lightAttackCombo.Count)
            {
                nextComboStep = 1;
            }

            attackStep = GetLightAttackStep(nextComboStep - 1);
            if (attackStep == null)
            {
                return false;
            }

            staminaCost = Mathf.Max(0f, attackStep.StaminaCost);
            if (currentStamina < staminaCost)
            {
                return false;
            }

            CommitAttackStart(CombatActionKind.LightAttack, attackStep);
            _currentComboStepIndex = nextComboStep;
            _comboWindowEndsAt = _actionEndsAt + Mathf.Max(0f, attackStep.ComboWindow);
            return true;
        }

        public bool TryBeginHeavyAttack(float currentStamina, out AttackStepDefinition attackStep, out float staminaCost)
        {
            attackStep = _heavyAttack ?? AttackStepDefinition.CreateHeavyAttackStep();
            staminaCost = Mathf.Max(0f, attackStep.StaminaCost);

            if (!CanStartOffensiveAction() || currentStamina < staminaCost)
            {
                return false;
            }

            ResetComboInternal();
            CommitAttackStart(CombatActionKind.HeavyAttack, attackStep);
            _comboWindowEndsAt = _actionEndsAt;
            return true;
        }

        public bool TryBeginBlock()
        {
            if (_currentAction == CombatActionKind.Dodging || _currentAction == CombatActionKind.GuardBroken)
            {
                return false;
            }

            if (IsBlocking)
            {
                return true;
            }

            ResetAttackInternal();
            ResetComboInternal();
            _currentAction = CombatActionKind.Blocking;
            _blockStartedAt = _time;
            return true;
        }

        public bool EndBlock()
        {
            if (!IsBlocking)
            {
                return false;
            }

            _currentAction = CombatActionKind.Idle;
            _blockStartedAt = -1f;
            return true;
        }

        public bool TryBeginDodge(float currentStamina, out DodgeSettings dodgeSettings, out float staminaCost)
        {
            dodgeSettings = _dodgeSettings ?? DodgeSettings.CreateDefault();
            staminaCost = Mathf.Max(0f, dodgeSettings.StaminaCost);

            if (_currentAction == CombatActionKind.Dodging || _currentAction == CombatActionKind.GuardBroken || _time < _dodgeCooldownEndsAt || currentStamina < staminaCost)
            {
                return false;
            }

            ResetComboInternal();
            ResetAttackInternal();
            _currentAction = CombatActionKind.Dodging;
            _currentAttackStep = null;
            _dodgeEndsAt = _time + Mathf.Max(0f, dodgeSettings.Duration);
            _dodgeCooldownEndsAt = _dodgeEndsAt + Mathf.Max(0f, dodgeSettings.Cooldown);
            return true;
        }

        public bool TryResolveBlockHit(
            float currentStamina,
            out float staminaDrain,
            out float damageMultiplier,
            out float perfectBlockStaggerDuration,
            out float guardBreakStaggerDuration)
        {
            staminaDrain = 0f;
            damageMultiplier = 1f;
            perfectBlockStaggerDuration = 0f;
            guardBreakStaggerDuration = 0f;

            if (!IsBlocking)
            {
                return false;
            }

            if (IsPerfectBlock)
            {
                damageMultiplier = 0f;
                perfectBlockStaggerDuration = Mathf.Max(0f, _blockSettings.PerfectBlockStaggerDuration);
                return true;
            }

            staminaDrain = Mathf.Max(0f, _blockSettings.StaminaDrainPerHit);
            damageMultiplier = Mathf.Clamp01(1f - Mathf.Clamp01(_blockSettings.DamageReduction));

            if (currentStamina <= staminaDrain)
            {
                guardBreakStaggerDuration = Mathf.Max(0f, _blockSettings.BlockBreakStaggerDuration);
            }

            return true;
        }

        public void TriggerGuardBreak(float duration)
        {
            ResetAttackInternal();
            ResetComboInternal();
            _currentAction = CombatActionKind.GuardBroken;
            _blockStartedAt = -1f;
            _dodgeEndsAt = -1f;
            _guardBreakEndsAt = _time + Mathf.Max(0f, duration);
        }

        public void Reset()
        {
            _time = 0f;
            _actionEndsAt = 0f;
            _comboWindowEndsAt = 0f;
            _blockStartedAt = -1f;
            _dodgeEndsAt = -1f;
            _dodgeCooldownEndsAt = -1f;
            _guardBreakEndsAt = -1f;
            _currentComboStepIndex = 0;
            _currentAction = CombatActionKind.Idle;
            _currentAttackStep = null;
        }

        private bool HasLightCombo()
        {
            return _lightAttackCombo != null && _lightAttackCombo.Count > 0;
        }

        private bool CanStartOffensiveAction()
        {
            return _currentAction != CombatActionKind.Dodging && _currentAction != CombatActionKind.GuardBroken && _currentAction != CombatActionKind.Blocking;
        }

        private float GetCurrentComboWindow()
        {
            if (_currentAttackStep == null)
            {
                return 0.6f;
            }

            return Mathf.Max(0f, _currentAttackStep.ComboWindow);
        }

        private AttackStepDefinition GetLightAttackStep(int index)
        {
            if (!HasLightCombo())
            {
                return AttackStepDefinition.CreateLightAttackStep(0);
            }

            int clampedIndex = Mathf.Clamp(index, 0, _lightAttackCombo.Count - 1);
            AttackStepDefinition attackStep = _lightAttackCombo[clampedIndex];
            return attackStep ?? AttackStepDefinition.CreateLightAttackStep(clampedIndex);
        }

        private void CommitAttackStart(CombatActionKind actionKind, AttackStepDefinition attackStep)
        {
            _currentAction = actionKind;
            _currentAttackStep = attackStep;
            _actionEndsAt = _time + Mathf.Max(0f, attackStep.WindUp) + Mathf.Max(0f, attackStep.Recovery);
            _blockStartedAt = -1f;
            _dodgeEndsAt = -1f;
        }

        private void ResetAttackInternal()
        {
            _currentAttackStep = null;
            _actionEndsAt = _time;
            _blockStartedAt = -1f;
        }

        private void ResetComboInternal()
        {
            _currentComboStepIndex = 0;
            _comboWindowEndsAt = _time;
        }
    }
}
