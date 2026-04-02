import { describe, expect, it } from "vitest";
import {
  clampCameraDistance,
  computeOrbitOffset,
  focusPointFromBody,
  resolveCameraCollisionDistance,
} from "./cameraMath";

describe("cameraMath", () => {
  it("computes an orbit offset from yaw, pitch, and distance", () => {
    const offset = computeOrbitOffset({ yaw: 0, pitch: 0.5, distance: 10 });

    expect(offset.x).toBeCloseTo(0);
    expect(offset.y).toBeCloseTo(4.794);
    expect(offset.z).toBeCloseTo(8.775);
  });

  it("clamps camera distance to the configured range", () => {
    expect(clampCameraDistance({ desiredDistance: 2, minDistance: 4.5, maxDistance: 16 })).toBe(4.5);
    expect(clampCameraDistance({ desiredDistance: 20, minDistance: 4.5, maxDistance: 16 })).toBe(16);
  });

  it("backs the camera off when a raycast hit is present", () => {
    expect(
      resolveCameraCollisionDistance({
        hitDistance: 6,
        desiredDistance: 10,
        minDistance: 4.5,
      }),
    ).toBe(5.65);
  });

  it("raises the focus point above the body", () => {
    const focus = focusPointFromBody({ x: 2, y: 3, z: 4, eyeHeight: 1.1 });

    expect(focus).toEqual({ x: 2, y: 4.1, z: 4 });
  });
});
