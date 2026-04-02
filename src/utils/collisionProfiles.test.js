import { describe, expect, it } from "vitest";
import {
  getBuildingCollisionProfile,
  getRockCollisionProfile,
  getTerrainCollisionProfile,
  getTreeCollisionProfile,
} from "./collisionProfiles";

describe("collisionProfiles", () => {
  it("creates a terrain trimesh profile from generated buffers", () => {
    const profile = getTerrainCollisionProfile({
      positions: new Float32Array(9),
      indices: new Uint16Array([0, 1, 2]),
    });

    expect(profile).toEqual({
      type: "trimesh",
      vertexCount: 3,
      indexCount: 3,
    });
  });

  it("describes tree trunk colliders and rock colliders", () => {
    expect(getTreeCollisionProfile({ scale: 1.5 }, "pine")).toEqual({
      type: "cylinder",
      halfHeight: 1.08,
      radius: 0.27,
    });

    expect(getRockCollisionProfile({ scale: 2 })).toEqual({
      type: "cuboid",
      halfExtents: [0.7, 0.56, 0.7],
    });
  });

  it("describes building colliders using footprint dimensions", () => {
    expect(
      getBuildingCollisionProfile({
        width: 5.2,
        wallHeight: 3.2,
        depth: 4.6,
      }),
    ).toEqual({
      type: "cuboid",
      halfExtents: [2.6, 1.6, 2.3],
    });
  });
});
