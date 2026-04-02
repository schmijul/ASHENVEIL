const DEFAULT_MODEL_CONFIG = Object.freeze({
  path: "",
  scale: 1,
  yOffset: 0,
  rotationY: 0,
});

const toFiniteNumber = (value, fallback) => {
  if (typeof value !== "number" || Number.isNaN(value) || !Number.isFinite(value)) {
    return fallback;
  }
  return value;
};

export function getCharacterModelConfig(models, id) {
  const model = models?.[id];

  if (!model || typeof model !== "object") {
    return DEFAULT_MODEL_CONFIG;
  }

  const path = typeof model.path === "string" ? model.path.trim() : "";
  const scale = Math.max(0.01, toFiniteNumber(model.scale, 1));
  const yOffset = toFiniteNumber(model.yOffset, 0);
  const rotationY = toFiniteNumber(model.rotationY, 0);

  return {
    path,
    scale,
    yOffset,
    rotationY,
  };
}

export { DEFAULT_MODEL_CONFIG };
