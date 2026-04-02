import { useFrame } from "@react-three/fiber";
import { useGameStore } from "../../store/gameStore";
import {
  COMBAT_CONSTANTS,
  computeStaminaRegen,
  resolveAttackHit,
} from "../../utils/combatMath";

const DUMMY_ATTACK_RANGE = 2.35;

export default function CombatSystem() {
  useFrame((_, delta) => {
    const now = performance.now() / 1000;
    const store = useGameStore.getState();
    const {
      combat,
      player,
      setCombatState,
      stopBlocking,
      finishAttack,
      finishDodge,
      markAttackResolved,
      damageTarget,
      setTargetNextAttackAt,
      applyIncomingDamage,
      setStamina,
    } = store;

    if (combat.isBlocking) {
      const nextStamina = Math.max(
        0,
        player.stamina - COMBAT_CONSTANTS.blockDrainPerSecond * delta,
      );
      setStamina(nextStamina);
      if (nextStamina <= 0) {
        stopBlocking();
      }
    } else {
      const nextStamina = computeStaminaRegen({
        currentStamina: player.stamina,
        maxStamina: player.maxStamina,
        delta,
        canRegen:
          !combat.isAttacking &&
          !combat.isDodging &&
          now >= combat.staminaRegenBlockedUntil,
      });

      if (nextStamina !== player.stamina) {
        setStamina(nextStamina);
      }
    }

    if (combat.isDodging && now >= combat.dodgeEndsAt) {
      finishDodge();
    }

    if (combat.isAttacking && combat.attackWindow) {
      if (!combat.attackWindow.hitResolved && now >= combat.attackWindow.hitAt) {
        Object.values(combat.targets).forEach((target) => {
          if (!target.alive) {
            return;
          }

          const didHit = resolveAttackHit({
            attackerPosition: player.position,
            attackerRotation: player.rotation,
            targetPosition: target.position,
            range: combat.attackWindow.range,
            arc: combat.attackWindow.arc,
            targetRadius: target.radius,
          });

          if (didHit) {
            damageTarget(
              target.id,
              combat.attackWindow.damage,
              now,
              combat.attackWindow.staggerDuration,
            );
          }
        });

        markAttackResolved();
      }

      if (now >= combat.attackWindow.endsAt) {
        finishAttack(now);
      }
    }

    const dummy = combat.targets.training_dummy;
    if (
      dummy?.alive &&
      now >= dummy.nextAttackAt &&
      now >= dummy.staggeredUntil
    ) {
      const dx = dummy.position[0] - player.position[0];
      const dz = dummy.position[2] - player.position[2];
      const distance = Math.hypot(dx, dz);

      if (distance <= DUMMY_ATTACK_RANGE) {
        applyIncomingDamage(dummy.attackDamage, now);
        setTargetNextAttackAt(dummy.id, now + dummy.attackCooldown);
      }
    }

    if (
      !combat.isAttacking &&
      !combat.isBlocking &&
      !combat.isDodging &&
      combat.comboStep > 0 &&
      now - combat.lastAttackAt > COMBAT_CONSTANTS.comboResetWindow
    ) {
      setCombatState({ comboStep: 0, combatMode: false });
    }
  });

  return null;
}
