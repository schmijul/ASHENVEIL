import { describe, expect, it } from "vitest";
import {
  calculateWalkBob,
  getPlayerMoveSpeed,
  resolveMovementVector,
  resolvePlayerVelocity,
} from "./playerMovement";

describe("playerMovement", () => {
  it("maps forward movement relative to camera yaw", () => {
    const movement = resolveMovementVector({ forward: true, cameraYaw: 0 });

    expect(movement.moving).toBe(true);
    expect(movement.x).toBeCloseTo(0);
    expect(movement.z).toBeCloseTo(-1);
  });

  it("rotates movement with camera yaw", () => {
    const movement = resolveMovementVector({
      forward: true,
      right: true,
      cameraYaw: Math.PI / 2,
    });

    expect(movement.x).toBeCloseTo(-0.7071, 3);
    expect(movement.z).toBeCloseTo(0.7071, 3);
  });

  it("uses sprint speed only while moving", () => {
    expect(getPlayerMoveSpeed({ isMoving: false, isSprinting: true })).toBe(0);
    expect(getPlayerMoveSpeed({ isMoving: true, isSprinting: false })).toBe(4.25);
    expect(getPlayerMoveSpeed({ isMoving: true, isSprinting: true })).toBe(6.75);
  });

  it("builds a velocity vector from input and camera yaw", () => {
    const result = resolvePlayerVelocity({
      input: { forward: true, sprint: true },
      cameraYaw: 0,
    });

    expect(result.speed).toBe(6.75);
    expect(result.velocity.x).toBeCloseTo(0);
    expect(result.velocity.z).toBeCloseTo(-6.75);
  });

  it("adds bob only when moving", () => {
    expect(calculateWalkBob({ time: 1, isMoving: false, speed: 0, baseHeight: -0.02 })).toBeCloseTo(-0.02);

    const bob = calculateWalkBob({
      time: 1,
      isMoving: true,
      speed: 4.25,
      baseHeight: -0.02,
    });

    expect(bob).not.toBeCloseTo(-0.02);
  });
});
