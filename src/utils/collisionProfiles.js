export const getTerrainCollisionProfile = ({ positions, indices }) => ({
  type: "trimesh",
  vertexCount: positions.length / 3,
  indexCount: indices.length,
});

export const getTreeCollisionProfile = (placement, kind = "pine") => {
  const scale = placement.scale ?? 1;
  const isPine = kind === "pine";

  return {
    type: "cylinder",
    halfHeight: (isPine ? 0.72 : 0.68) * scale,
    radius: (isPine ? 0.18 : 0.22) * scale,
  };
};

export const getRockCollisionProfile = (placement) => {
  const scale = placement.scale ?? 1;
  return {
    type: "cuboid",
    halfExtents: [0.35 * scale, 0.28 * scale, 0.35 * scale],
  };
};

export const getBuildingCollisionProfile = (building) => ({
  type: "cuboid",
  halfExtents: [building.width / 2, building.wallHeight / 2, building.depth / 2],
});
