import enemyData from "../data/enemies.json" with { type: "json" };
import { sampleTerrainHeight } from "./terrainGeneration";

const enemyCatalog = Object.fromEntries(
  enemyData.enemies.map((enemy) => [enemy.id, enemy]),
);

const hashSeed = (value) =>
  Array.from(value).reduce((total, char) => total + char.charCodeAt(0), 0);

const pseudoRandom = (seed) => {
  const value = Math.sin(seed * 12.9898) * 43758.5453;
  return value - Math.floor(value);
};

const groundedPosition = (x, z) => [x, sampleTerrainHeight(x, z, { seed: 7 }) + 0.62, z];

export const BOAR_SPAWNS = [
  { id: "boar_1", enemyId: "boar", x: -12, z: 19 },
  { id: "boar_2", enemyId: "boar", x: 10, z: 22 },
  { id: "boar_3", enemyId: "boar", x: -18, z: 30 },
  { id: "boar_4", enemyId: "boar", x: 16, z: 31 },
  { id: "boar_5", enemyId: "boar", x: -8, z: 38 },
  { id: "scarred_boar_1", enemyId: "scarred_boar", x: 14, z: 41 },
];

export const createBoarTarget = ({ id, enemyId, x, z }) => {
  const definition = enemyCatalog[enemyId];
  const seed = hashSeed(id);
  return {
    id,
    enemyId,
    kind: "boar",
    label: definition.name,
    position: groundedPosition(x, z),
    spawnPosition: groundedPosition(x, z),
    radius: definition.attackRange * 0.35,
    health: definition.health,
    maxHealth: definition.health,
    damage: definition.damage,
    moveSpeed: definition.moveSpeed,
    aggroRange: definition.aggroRange,
    attackRange: definition.attackRange,
    attackCooldown: definition.attackSpeed + 0.55,
    nextAttackAt: 0,
    alive: true,
    aiState: "idle",
    facing: pseudoRandom(seed) * Math.PI * 2,
    aiStateUntil: 1 + pseudoRandom(seed + 2) * 1.5,
    attackWindupUntil: 0,
    attackResolved: false,
    lootTable: definition.loot,
    lootDrop: [],
    looted: false,
    staggeredUntil: 0,
    hitFlashUntil: 0,
  };
};

const nextGroundedPosition = (x, z) => groundedPosition(x, z);

export const resolveBoarState = ({
  boar,
  playerPosition,
  now,
  delta,
}) => {
  if (!boar.alive) {
    return {};
  }

  const dx = playerPosition[0] - boar.position[0];
  const dz = playerPosition[2] - boar.position[2];
  const distanceToPlayer = Math.hypot(dx, dz);
  const lowHealth = boar.health / boar.maxHealth <= 0.2 && boar.enemyId === "boar";

  if (now < boar.staggeredUntil) {
    return { aiState: "staggered" };
  }

  if (lowHealth) {
    const fleeAngle = Math.atan2(-dx, -dz);
    const nextX =
      boar.position[0] + Math.sin(fleeAngle) * boar.moveSpeed * 1.1 * delta;
    const nextZ =
      boar.position[2] + Math.cos(fleeAngle) * boar.moveSpeed * 1.1 * delta;

    return {
      aiState: "flee",
      facing: fleeAngle,
      position: nextGroundedPosition(nextX, nextZ),
    };
  }

  if (boar.aiState === "attack") {
    if (now >= boar.attackWindupUntil && !boar.attackResolved) {
      return {
        attackResolved: true,
        nextAttackAt: now + boar.attackCooldown,
      };
    }

    if (now >= boar.nextAttackAt && boar.attackResolved) {
      return {
        aiState: "chase",
      };
    }

    return {};
  }

  if (distanceToPlayer <= boar.attackRange + 0.25 && now >= boar.nextAttackAt) {
    return {
      aiState: "attack",
      facing: Math.atan2(dx, dz),
      attackWindupUntil:
        now + (boar.enemyId === "scarred_boar" ? 1 : 1.5),
      attackResolved: false,
    };
  }

  if (distanceToPlayer <= boar.aggroRange) {
    const chaseAngle = Math.atan2(dx, dz);
    const nextX = boar.position[0] + Math.sin(chaseAngle) * boar.moveSpeed * delta;
    const nextZ = boar.position[2] + Math.cos(chaseAngle) * boar.moveSpeed * delta;

    return {
      aiState: distanceToPlayer < boar.attackRange + 1 ? "alert" : "chase",
      facing: chaseAngle,
      position: nextGroundedPosition(nextX, nextZ),
      aiStateUntil: now + 0.4,
    };
  }

  if (boar.aiStateUntil <= now) {
    const wanderSeed = hashSeed(boar.id) + Math.floor(now * 10);
    const wanderAngle = pseudoRandom(wanderSeed) * Math.PI * 2;
    const wanderDistance = 1.1 + pseudoRandom(wanderSeed + 3) * 1.6;
    const nextX =
      boar.position[0] + Math.sin(wanderAngle) * boar.moveSpeed * 0.35 * delta * wanderDistance;
    const nextZ =
      boar.position[2] + Math.cos(wanderAngle) * boar.moveSpeed * 0.35 * delta * wanderDistance;

    return {
      aiState: "idle",
      facing: wanderAngle,
      position: nextGroundedPosition(nextX, nextZ),
      aiStateUntil: now + 1.6 + pseudoRandom(wanderSeed + 5) * 1.3,
    };
  }

  return {};
};

export const rollEnemyLoot = (target) => {
  const seed = hashSeed(target.id) + hashSeed(target.enemyId);
  return target.lootTable.flatMap((entry, index) => {
    const randomValue = pseudoRandom(seed + index * 13);
    if (randomValue > entry.chance) {
      return [];
    }

    const [min, max] = entry.quantity;
    const quantity =
      min + Math.floor(pseudoRandom(seed + index * 17 + 5) * (max - min + 1));

    return {
      itemId: entry.item,
      quantity,
    };
  });
};
