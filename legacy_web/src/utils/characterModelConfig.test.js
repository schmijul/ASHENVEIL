import { describe, expect, it } from "vitest";
import {
  DEFAULT_MODEL_CONFIG,
  getCharacterModelConfig,
} from "./characterModelConfig";

describe("characterModelConfig", () => {
  it("returns defaults when a model id is missing", () => {
    const config = getCharacterModelConfig({}, "player");

    expect(config).toEqual(DEFAULT_MODEL_CONFIG);
  });

  it("normalizes valid model values", () => {
    const config = getCharacterModelConfig(
      {
        player: {
          path: " /models/characters/player.glb ",
          scale: 1.25,
          yOffset: -0.42,
          rotationY: Math.PI * 0.5,
        },
      },
      "player",
    );

    expect(config.path).toBe("/models/characters/player.glb");
    expect(config.scale).toBe(1.25);
    expect(config.yOffset).toBe(-0.42);
    expect(config.rotationY).toBeCloseTo(Math.PI * 0.5);
  });

  it("guards against invalid values", () => {
    const config = getCharacterModelConfig(
      {
        player: {
          path: 123,
          scale: -4,
          yOffset: Number.NaN,
          rotationY: Infinity,
        },
      },
      "player",
    );

    expect(config.path).toBe("");
    expect(config.scale).toBe(0.01);
    expect(config.yOffset).toBe(0);
    expect(config.rotationY).toBe(0);
  });
});
