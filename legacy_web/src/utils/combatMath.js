const clamp = (value, min, max) => Math.min(max, Math.max(min, value));

export const COMBAT_CONSTANTS = {
  comboResetWindow: 0.85,
  heavyHoldThreshold: 0.35,
  staminaRegenDelay: 1.1,
  staminaRegenRate: 18,
  blockDrainPerSecond: 14,
  blockHitDrain: 9,
  blockReduction: 0.65,
  dodgeCost: 22,
  dodgeSpeed: 10.5,
  dodgeDuration: 0.42,
  dodgeIFrameDuration: 0.28,
};

const LIGHT_ATTACKS = [
  {
    comboStep: 1,
    damage: 16,
    staminaCost: 16,
    range: 2.2,
    arc: Math.PI * 0.82,
    hitTime: 0.18,
    duration: 0.44,
    staggerDuration: 0.18,
  },
  {
    comboStep: 2,
    damage: 18,
    staminaCost: 16,
    range: 2.35,
    arc: Math.PI * 0.88,
    hitTime: 0.17,
    duration: 0.42,
    staggerDuration: 0.2,
  },
  {
    comboStep: 3,
    damage: 22,
    staminaCost: 18,
    range: 2.55,
    arc: Math.PI * 0.94,
    hitTime: 0.2,
    duration: 0.48,
    staggerDuration: 0.3,
  },
];

export const getLightAttackWindow = ({
  comboStep = 0,
  lastAttackAt = 0,
  now = 0,
} = {}) => {
  const shouldResetCombo =
    comboStep <= 0 || now - lastAttackAt > COMBAT_CONSTANTS.comboResetWindow;
  const attackIndex = shouldResetCombo
    ? 0
    : Math.min(comboStep, LIGHT_ATTACKS.length - 1);
  const attack = LIGHT_ATTACKS[attackIndex];

  return {
    type: "light",
    comboStep: attack.comboStep,
    damage: attack.damage,
    staminaCost: attack.staminaCost,
    range: attack.range,
    arc: attack.arc,
    hitAt: now + attack.hitTime,
    endsAt: now + attack.duration,
    staggerDuration: attack.staggerDuration,
    hitResolved: false,
  };
};

export const getHeavyAttackWindow = ({ now = 0 } = {}) => ({
  type: "heavy",
  comboStep: 0,
  damage: 30,
  staminaCost: 28,
  range: 2.8,
  arc: Math.PI * 0.95,
  hitAt: now + 0.14,
  endsAt: now + 0.72,
  staggerDuration: 0.95,
  hitResolved: false,
});

export const shouldResetCombo = ({
  comboStep = 0,
  lastAttackAt = 0,
  now = 0,
} = {}) =>
  comboStep > 0 && now - lastAttackAt > COMBAT_CONSTANTS.comboResetWindow;

export const resolveAttackHit = ({
  attackerPosition = [0, 0, 0],
  attackerRotation = 0,
  targetPosition = [0, 0, 0],
  range = 2,
  arc = Math.PI * 0.8,
  targetRadius = 0.6,
} = {}) => {
  const dx = targetPosition[0] - attackerPosition[0];
  const dz = targetPosition[2] - attackerPosition[2];
  const distance = Math.hypot(dx, dz);

  if (distance > range + targetRadius) {
    return false;
  }

  const targetAngle = Math.atan2(dx, dz);
  const angleDelta = Math.atan2(
    Math.sin(targetAngle - attackerRotation),
    Math.cos(targetAngle - attackerRotation),
  );

  return Math.abs(angleDelta) <= arc / 2;
};

export const computeStaminaRegen = ({
  currentStamina = 0,
  maxStamina = 100,
  delta = 0,
  canRegen = false,
} = {}) => {
  if (!canRegen) {
    return currentStamina;
  }

  return clamp(
    currentStamina + COMBAT_CONSTANTS.staminaRegenRate * delta,
    0,
    maxStamina,
  );
};

export const resolveBlockDamage = ({
  incomingDamage = 0,
  isBlocking = false,
  stamina = 0,
} = {}) => {
  if (!isBlocking || stamina <= 0) {
    return {
      damageTaken: incomingDamage,
      staminaSpent: 0,
    };
  }

  return {
    damageTaken: incomingDamage * (1 - COMBAT_CONSTANTS.blockReduction),
    staminaSpent: COMBAT_CONSTANTS.blockHitDrain,
  };
};

export const createDodgeState = ({
  direction = [0, 1],
  now = 0,
} = {}) => {
  const length = Math.hypot(direction[0], direction[1]) || 1;
  const normalizedDirection = [direction[0] / length, direction[1] / length];

  return {
    dodgeDirection: normalizedDirection,
    dodgeEndsAt: now + COMBAT_CONSTANTS.dodgeDuration,
    invulnerableUntil: now + COMBAT_CONSTANTS.dodgeIFrameDuration,
  };
};
