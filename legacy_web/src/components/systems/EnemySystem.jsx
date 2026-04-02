import { useEffect } from "react";
import { useFrame } from "@react-three/fiber";
import { useGameStore } from "../../store/gameStore";
import { BOAR_SPAWNS, createBoarTarget, resolveBoarState } from "../../utils/boarAI";

const DUMMY_KIND = "dummy";

export default function EnemySystem() {
  useEffect(() => {
    const { upsertCombatTargets } = useGameStore.getState();
    upsertCombatTargets(BOAR_SPAWNS.map(createBoarTarget));
  }, []);

  useFrame((_, delta) => {
    const now = performance.now() / 1000;
    const store = useGameStore.getState();
    const { player, combat, setCombatTargetState, applyIncomingDamage, setTargetNextAttackAt } =
      store;

    Object.values(combat.targets).forEach((target) => {
      if (target.kind !== "boar" || !target.alive) {
        return;
      }

      const update = resolveBoarState({
        boar: target,
        playerPosition: player.position,
        now,
        delta,
      });

      if (Object.keys(update).length > 0) {
        setCombatTargetState(target.id, update);
      }

      const liveTarget = useGameStore.getState().combat.targets[target.id];
      if (
        liveTarget.aiState === "attack" &&
        !liveTarget.attackResolved &&
        now >= liveTarget.attackWindupUntil
      ) {
        applyIncomingDamage(liveTarget.damage, now);
        setCombatTargetState(liveTarget.id, { attackResolved: true });
        setTargetNextAttackAt(liveTarget.id, now + liveTarget.attackCooldown);
      }
    });

    const activeHostiles = Object.values(combat.targets).some(
      (target) =>
        target.kind !== DUMMY_KIND &&
        target.alive &&
        target.aiState !== "idle" &&
        target.aiState !== "flee",
    );

    if (activeHostiles && !combat.combatMode) {
      store.setCombatState({ combatMode: true });
    }
  });

  return null;
}
