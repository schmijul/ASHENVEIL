import { Color } from "three";

const clamp = (value, min, max) => Math.min(max, Math.max(min, value));

const lerp = (a, b, t) => a + (b - a) * t;

const fade = (t) => t * t * (3 - 2 * t);

const hash2 = (x, z, seed) => {
  const value = Math.sin(x * 127.1 + z * 311.7 + seed * 74.7) * 43758.5453123;
  return value - Math.floor(value);
};

export const valueNoise2D = (x, z, seed = 1) => {
  const x0 = Math.floor(x);
  const z0 = Math.floor(z);
  const x1 = x0 + 1;
  const z1 = z0 + 1;
  const sx = fade(x - x0);
  const sz = fade(z - z0);

  const n00 = hash2(x0, z0, seed);
  const n10 = hash2(x1, z0, seed);
  const n01 = hash2(x0, z1, seed);
  const n11 = hash2(x1, z1, seed);
  const ix0 = lerp(n00, n10, sx);
  const ix1 = lerp(n01, n11, sx);

  return lerp(ix0, ix1, sz) * 2 - 1;
};

export const isOnForestPath = (
  x,
  z,
  {
    halfWidth = 6.75,
    zMin = -24,
    zMax = 42,
  } = {},
) => Math.abs(x) <= halfWidth && z >= zMin && z <= zMax;

export const isInsideClearing = (
  x,
  z,
  clearings = [
    { x: 0, z: -2, radius: 10 },
    { x: 16, z: 18, radius: 8 },
  ],
) =>
  clearings.some((clearing) => {
    const dx = x - clearing.x;
    const dz = z - clearing.z;
    return Math.hypot(dx, dz) <= clearing.radius;
  });

export const sampleTerrainHeight = (
  x,
  z,
  {
    seed = 7,
    baseHeight = -0.02,
    amplitude = 0.28,
    detailScale = 0.06,
    ridgeScale = 0.02,
    pathDepth = 0.08,
  } = {},
) => {
  const broadNoise =
    valueNoise2D(x * detailScale, z * detailScale, seed) * 0.55 +
    valueNoise2D(x * detailScale * 2.7, z * detailScale * 2.7, seed + 13) * 0.25;
  const ridgeNoise = Math.sin(x * ridgeScale + seed * 0.31) * 0.12;
  const lowlands = Math.cos(z * ridgeScale * 0.85 + seed * 0.19) * 0.08;
  const pathInfluence = isOnForestPath(x, z) ? 1 : 0;

  return clamp(
    baseHeight +
      (broadNoise + ridgeNoise + lowlands) * amplitude -
      pathInfluence * pathDepth,
    -0.22,
    0.36,
  );
};

const mixColor = (a, b, t) => {
  const color = new Color(a);
  color.lerp(new Color(b), clamp(t, 0, 1));
  return [color.r, color.g, color.b];
};

export const buildTerrainData = ({
  size = 120,
  segments = 48,
  seed = 7,
} = {}) => {
  const vertexCount = (segments + 1) * (segments + 1);
  const positions = new Float32Array(vertexCount * 3);
  const colors = new Float32Array(vertexCount * 3);
  const indices = [];

  let offset = 0;
  const halfSize = size / 2;

  for (let row = 0; row <= segments; row += 1) {
    for (let col = 0; col <= segments; col += 1) {
      const x = (col / segments) * size - halfSize;
      const z = (row / segments) * size - halfSize;
      const height = sampleTerrainHeight(x, z, { seed });

      positions[offset] = x;
      positions[offset + 1] = height;
      positions[offset + 2] = z;

      let color;
      if (isOnForestPath(x, z)) {
        color = mixColor("#8b6914", "#a1823a", 0.35 + Math.abs(height) * 0.8);
      } else if (isInsideClearing(x, z)) {
        color = mixColor("#5c7a4a", "#7b9a62", 0.35 + height * 0.7);
      } else if (height > 0.18) {
        color = mixColor("#5c7a4a", "#7a7a7a", (height - 0.18) / 0.18);
      } else {
        color = mixColor("#3b5f3b", "#5c7a4a", (height + 0.05) / 0.25);
      }

      colors[offset] = color[0];
      colors[offset + 1] = color[1];
      colors[offset + 2] = color[2];
      offset += 3;
    }
  }

  for (let row = 0; row < segments; row += 1) {
    for (let col = 0; col < segments; col += 1) {
      const topLeft = row * (segments + 1) + col;
      const topRight = topLeft + 1;
      const bottomLeft = topLeft + segments + 1;
      const bottomRight = bottomLeft + 1;

      indices.push(topLeft, bottomLeft, topRight);
      indices.push(topRight, bottomLeft, bottomRight);
    }
  }

  return {
    positions,
    colors,
    indices: new Uint16Array(indices),
    segmentCount: segments,
    size,
  };
};

const createInstancePlacement = (x, z, scale, terrainHeight, rotation) => ({
  position: [x, terrainHeight, z],
  rotation,
  scale,
});

const randomFromSeed = (x, z, seed) => {
  const value = hash2(x * 13.17, z * 9.71, seed);
  return value;
};

export const buildForestPlacements = ({
  size = 120,
  seed = 7,
  treeSpacing = 8,
  grassSpacing = 4.5,
  rockSpacing = 11,
} = {}) => {
  const halfSize = size / 2;
  const pines = [];
  const oaks = [];
  const grass = [];
  const rocks = [];

  for (let z = -halfSize + 4; z <= halfSize - 4; z += treeSpacing) {
    for (let x = -halfSize + 4; x <= halfSize - 4; x += treeSpacing) {
      const noise = randomFromSeed(x, z, seed);
      const terrainHeight = sampleTerrainHeight(x, z, { seed });

      if (isOnForestPath(x, z) || isInsideClearing(x, z)) {
        continue;
      }

      if (noise > 0.72) {
        const scale = 0.85 + randomFromSeed(x + 11, z + 3, seed + 19) * 0.5;
        const placement = createInstancePlacement(
          x,
          z,
          scale,
          terrainHeight,
          randomFromSeed(x - 7, z + 2, seed + 5) * Math.PI * 2,
        );

        if (noise > 0.86) {
          oaks.push(placement);
        } else {
          pines.push(placement);
        }
      }
    }
  }

  for (let z = -halfSize + 3; z <= halfSize - 3; z += grassSpacing) {
    for (let x = -halfSize + 3; x <= halfSize - 3; x += grassSpacing) {
      const noise = randomFromSeed(x + 5, z - 2, seed + 23);
      if (noise < 0.42 || isOnForestPath(x, z) || isInsideClearing(x, z)) {
        continue;
      }

      grass.push(
        createInstancePlacement(
          x,
          z,
          0.45 + noise * 0.35,
          sampleTerrainHeight(x, z, { seed }),
          noise * Math.PI * 2,
        ),
      );
    }
  }

  for (let z = -halfSize + 5; z <= halfSize - 5; z += rockSpacing) {
    for (let x = -halfSize + 5; x <= halfSize - 5; x += rockSpacing) {
      const noise = randomFromSeed(x - 11, z + 6, seed + 31);
      if (noise < 0.76 || isOnForestPath(x, z) || isInsideClearing(x, z)) {
        continue;
      }

      const terrainHeight = sampleTerrainHeight(x, z, { seed });
      rocks.push(
        createInstancePlacement(
          x,
          z,
          0.55 + noise * 0.85,
          terrainHeight,
          noise * Math.PI,
        ),
      );
    }
  }

  return { pines, oaks, grass, rocks };
};
