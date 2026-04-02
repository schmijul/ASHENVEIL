import { describe, expect, it } from "vitest";
import {
  COMBAT_CONSTANTS,
  computeStaminaRegen,
  createDodgeState,
  getHeavyAttackWindow,
  getLightAttackWindow,
  resolveAttackHit,
  resolveBlockDamage,
  shouldResetCombo,
} from "./combatMath";

describe("combatMath", () => {
  it("progresses and resets the light combo window", () => {
    const first = getLightAttackWindow({ comboStep: 0, lastAttackAt: 0, now: 0 });
    const second = getLightAttackWindow({
      comboStep: first.comboStep,
      lastAttackAt: 0.2,
      now: 0.45,
    });

    expect(first.comboStep).toBe(1);
    expect(second.comboStep).toBe(2);
    expect(
      shouldResetCombo({ comboStep: 2, lastAttackAt: 0.1, now: 1.3 }),
    ).toBe(true);
  });

  it("builds heavy attack timing and stamina data", () => {
    const heavy = getHeavyAttackWindow({ now: 2 });

    expect(heavy.type).toBe("heavy");
    expect(heavy.damage).toBeGreaterThan(25);
    expect(heavy.hitAt).toBeGreaterThan(2);
  });

  it("detects targets inside the attack arc", () => {
    expect(
      resolveAttackHit({
        attackerPosition: [0, 0, 0],
        attackerRotation: 0,
        targetPosition: [0.2, 0, 1.8],
        range: 2.2,
      }),
    ).toBe(true);

    expect(
      resolveAttackHit({
        attackerPosition: [0, 0, 0],
        attackerRotation: 0,
        targetPosition: [2.5, 0, 0],
        range: 2.2,
      }),
    ).toBe(false);
  });

  it("applies block reduction and hit stamina cost", () => {
    const guarded = resolveBlockDamage({
      incomingDamage: 10,
      isBlocking: true,
      stamina: 40,
    });

    expect(guarded.damageTaken).toBeCloseTo(3.5);
    expect(guarded.staminaSpent).toBe(COMBAT_CONSTANTS.blockHitDrain);
  });

  it("creates a dodge i-frame window and regens stamina when allowed", () => {
    const dodge = createDodgeState({ direction: [3, 4], now: 1 });

    expect(dodge.dodgeDirection[0]).toBeCloseTo(0.6);
    expect(dodge.invulnerableUntil).toBeCloseTo(
      1 + COMBAT_CONSTANTS.dodgeIFrameDuration,
    );
    expect(
      computeStaminaRegen({
        currentStamina: 40,
        maxStamina: 100,
        delta: 1,
        canRegen: true,
      }),
    ).toBeCloseTo(58);
  });
});
