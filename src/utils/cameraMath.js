const clamp = (value, min, max) => Math.min(max, Math.max(min, value));

export const computeOrbitOffset = ({
  yaw = 0,
  pitch = 0.5,
  distance = 10,
} = {}) => {
  const safeDistance = Math.max(0, distance);
  const clampedPitch = clamp(pitch, 0.1, 1.2);
  const cosPitch = Math.cos(clampedPitch);

  return {
    x: Math.sin(yaw) * cosPitch * safeDistance,
    y: Math.sin(clampedPitch) * safeDistance,
    z: Math.cos(yaw) * cosPitch * safeDistance,
  };
};

export const clampCameraDistance = ({
  desiredDistance = 10,
  minDistance = 3,
  maxDistance = 16,
} = {}) => clamp(desiredDistance, minDistance, maxDistance);

export const resolveCameraCollisionDistance = ({
  hitDistance = null,
  desiredDistance = 10,
  minDistance = 3,
  buffer = 0.35,
} = {}) => {
  if (hitDistance === null || hitDistance === undefined) {
    return clampCameraDistance({ desiredDistance, minDistance });
  }

  return clampCameraDistance({
    desiredDistance: hitDistance - buffer,
    minDistance,
    maxDistance: desiredDistance,
  });
};

export const focusPointFromBody = ({
  x = 0,
  y = 0,
  z = 0,
  eyeHeight = 1.15,
} = {}) => ({
  x,
  y: y + eyeHeight,
  z,
});
