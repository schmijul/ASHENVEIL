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
) => {
  // Serpentine wobble on path edges for organic feel
  const wobble = Math.sin(z * 0.08) * 1.5;
  return Math.abs(x - wobble) <= halfWidth && z >= zMin && z <= zMax;
};

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
    pathDepth = 0.12,
  } = {},
) => {
  const broadNoise =
    valueNoise2D(x * detailScale, z * detailScale, seed) * 0.55 +
    valueNoise2D(x * detailScale * 2.7, z * detailScale * 2.7, seed + 13) * 0.25;
  const ridgeNoise = Math.sin(x * ridgeScale + seed * 0.31) * 0.12;
  const lowlands = Math.cos(z * ridgeScale * 0.85 + seed * 0.19) * 0.08;
  // Micro-detail noise for organic surface feel
  const microNoise = valueNoise2D(x * 0.15, z * 0.15, seed + 37) * 0.06;
  const pathInfluence = isOnForestPath(x, z) ? 1 : 0;

  return clamp(
    baseHeight +
      (broadNoise + ridgeNoise + lowlands + microNoise) * amplitude -
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
  segments = 128,
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

      // Per-vertex noise variation (+/-8%) for organic look
      const variation = (hash2(x * 7.3, z * 5.1, seed + 99) - 0.5) * 0.08;

      let color;
      if (isOnForestPath(x, z)) {
        // Worn earth path — brown, path edges blend to green
        const edgeBlend = clamp(1 - (Math.abs(x - Math.sin(z * 0.08) * 1.5) / 6.75), 0, 1);
        color = mixColor("#5d7548", "#7a6035", edgeBlend * 0.9 + variation);
      } else if (isInsideClearing(x, z)) {
        // Bright clearing — lighter, sunlit grass
        color = mixColor("#6b8450", "#8aa868", 0.35 + height * 0.7 + variation);
      } else if (height > 0.18) {
        // Rocky high ground
        color = mixColor("#5d7548", "#5a5a58", (height - 0.18) / 0.18 + variation);
      } else {
        // Forest floor — deep green with warm variation
        color = mixColor("#3d6438", "#4a6340", (height + 0.05) / 0.25 + variation);
      }

      colors[offset] = clamp(color[0], 0, 1);
      colors[offset + 1] = clamp(color[1], 0, 1);
      colors[offset + 2] = clamp(color[2], 0, 1);
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
    indices: new Uint32Array(indices),
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
  treeSpacing = 6.5,
  grassSpacing = 2.0,
  rockSpacing = 10,
  bushSpacing = 5.0,
  logSpacing = 18,
} = {}) => {
  const halfSize = size / 2;
  const pines = [];
  const oaks = [];
  const grass = [];
  const rocks = [];
  const bushes = [];
  const logs = [];

  for (let z = -halfSize + 4; z <= halfSize - 4; z += treeSpacing) {
    for (let x = -halfSize + 4; x <= halfSize - 4; x += treeSpacing) {
      const noise = randomFromSeed(x, z, seed);
      const terrainHeight = sampleTerrainHeight(x, z, { seed });

      if (isOnForestPath(x, z) || isInsideClearing(x, z)) {
        continue;
      }

      if (noise > 0.68) {
        const scale = 0.85 + randomFromSeed(x + 11, z + 3, seed + 19) * 0.5;
        const placement = createInstancePlacement(
          x,
          z,
          scale,
          terrainHeight,
          randomFromSeed(x - 7, z + 2, seed + 5) * Math.PI * 2,
        );

        if (noise > 0.84) {
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
      if (noise < 0.35 || isOnForestPath(x, z) || isInsideClearing(x, z)) {
        continue;
      }

      grass.push(
        createInstancePlacement(
          x,
          z,
          0.5 + noise * 0.4,
          sampleTerrainHeight(x, z, { seed }),
          noise * Math.PI * 2,
        ),
      );
    }
  }

  for (let z = -halfSize + 5; z <= halfSize - 5; z += rockSpacing) {
    for (let x = -halfSize + 5; x <= halfSize - 5; x += rockSpacing) {
      const noise = randomFromSeed(x - 11, z + 6, seed + 31);
      if (noise < 0.72 || isOnForestPath(x, z) || isInsideClearing(x, z)) {
        continue;
      }

      const terrainHeight = sampleTerrainHeight(x, z, { seed });
      rocks.push(
        createInstancePlacement(
          x,
          z,
          0.6 + noise * 0.95,
          terrainHeight,
          noise * Math.PI,
        ),
      );
    }
  }

  for (let z = -halfSize + 5; z <= halfSize - 5; z += bushSpacing) {
    for (let x = -halfSize + 5; x <= halfSize - 5; x += bushSpacing) {
      const noise = randomFromSeed(x + 3, z - 4, seed + 47);
      if (noise < 0.55 || isOnForestPath(x, z) || isInsideClearing(x, z)) {
        continue;
      }

      bushes.push(
        createInstancePlacement(
          x,
          z,
          0.6 + noise * 0.5,
          sampleTerrainHeight(x, z, { seed }),
          noise * Math.PI * 2,
        ),
      );
    }
  }

  for (let z = -halfSize + 8; z <= halfSize - 8; z += logSpacing) {
    for (let x = -halfSize + 8; x <= halfSize - 8; x += logSpacing) {
      const noise = randomFromSeed(x - 3, z + 9, seed + 61);
      if (noise < 0.6 || isOnForestPath(x, z) || isInsideClearing(x, z)) {
        continue;
      }

      const terrainHeight = sampleTerrainHeight(x, z, { seed });
      logs.push({
        position: [x, terrainHeight + 0.1, z],
        rotation: noise * Math.PI * 2,
        scale: 0.8 + noise * 0.5,
      });
    }
  }

  return { pines, oaks, grass, rocks, bushes, logs };
};
