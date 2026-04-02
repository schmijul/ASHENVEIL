const clamp = (value, min, max) => Math.min(max, Math.max(min, value));

const toBoolean = (value) => Boolean(value);

export const resolveMovementVector = ({
  forward = false,
  backward = false,
  left = false,
  right = false,
  cameraYaw = 0,
} = {}) => {
  const forwardAxis = toBoolean(forward) ? 1 : 0;
  const backwardAxis = toBoolean(backward) ? 1 : 0;
  const leftAxis = toBoolean(left) ? 1 : 0;
  const rightAxis = toBoolean(right) ? 1 : 0;

  const xAxis = rightAxis - leftAxis;
  const zAxis = forwardAxis - backwardAxis;

  if (xAxis === 0 && zAxis === 0) {
    return {
      moving: false,
      x: 0,
      y: 0,
      z: 0,
      magnitude: 0,
      direction: 0,
    };
  }

  const sin = Math.sin(cameraYaw);
  const cos = Math.cos(cameraYaw);

  const worldX = xAxis * cos + zAxis * -sin;
  const worldZ = xAxis * sin + zAxis * -cos;
  const magnitude = Math.hypot(worldX, worldZ) || 1;
  const x = worldX / magnitude;
  const z = worldZ / magnitude;

  return {
    moving: true,
    x,
    y: 0,
    z,
    magnitude,
    direction: Math.atan2(x, z),
  };
};

export const getPlayerMoveSpeed = ({
  isMoving = false,
  isSprinting = false,
  walkSpeed = 4.25,
  sprintSpeed = 6.75,
} = {}) => {
  if (!isMoving) {
    return 0;
  }

  return isSprinting ? sprintSpeed : walkSpeed;
};

export const resolvePlayerVelocity = ({
  input = {},
  cameraYaw = 0,
  walkSpeed = 4.25,
  sprintSpeed = 6.75,
} = {}) => {
  const movement = resolveMovementVector({
    ...input,
    cameraYaw,
  });
  const speed = getPlayerMoveSpeed({
    isMoving: movement.moving,
    isSprinting: input.sprint,
    walkSpeed,
    sprintSpeed,
  });

  return {
    movement,
    speed,
    velocity: {
      x: movement.x * speed,
      y: 0,
      z: movement.z * speed,
    },
  };
};

export const calculateWalkBob = ({
  time = 0,
  speed = 0,
  isMoving = false,
  baseHeight = 0,
  amplitude = 0.055,
  frequency = 10,
}) => {
  if (!isMoving || speed <= 0) {
    return baseHeight;
  }

  const intensity = clamp(speed / 6.75, 0, 1);
  return baseHeight + Math.sin(time * frequency * (0.75 + intensity * 0.6)) * amplitude * intensity;
};
