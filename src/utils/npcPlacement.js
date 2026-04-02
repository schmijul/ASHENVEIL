import npcsData from "../data/npcs.json" with { type: "json" };
import { sampleTerrainHeight } from "./terrainGeneration";
import { buildVillageLayout } from "./villageLayout";

export const NPC_INTERACTION_RADIUS = 2.9;

const placementRules = {
  maren: {
    anchorId: "maren_house",
    offset: [2.2, 2.8],
    lookAt: [0, -4],
  },
  korvin: {
    anchorId: "korvin_stall",
    offset: [0, 2.7],
    lookAt: [0, -6],
  },
  hagen: {
    anchorId: "hagen_forge",
    offset: [-1.6, 2.9],
    lookAt: [9, -3],
  },
  lotte: {
    anchorId: "lotte_house",
    offset: [1.9, 2.5],
    lookAt: [-8, -2],
  },
  ren: {
    anchorId: "ren_camp",
    offset: [-2.2, 2.4],
    lookAt: [17, 4],
  },
};

const getFacingAngle = (position, lookAt) =>
  Math.atan2(lookAt[0] - position[0], lookAt[1] - position[2]);

export const buildNpcPlacements = ({ seed = 7 } = {}) => {
  const villageLayout = buildVillageLayout({ seed });
  const buildings = Object.fromEntries(
    villageLayout.buildings.map((building) => [building.id, building]),
  );

  return npcsData.npcs.map((npc) => {
    const rule = placementRules[npc.id];
    const anchor = buildings[rule?.anchorId];

    if (!rule || !anchor) {
      throw new Error(`Missing NPC placement rule for ${npc.id}`);
    }

    const x = anchor.position[0] + rule.offset[0];
    const z = anchor.position[2] + rule.offset[1];
    const y = sampleTerrainHeight(x, z, { seed }) + 0.02;
    const position = [x, y, z];

    return {
      id: npc.id,
      name: npc.name,
      role: npc.role,
      location: npc.location,
      position,
      anchorId: rule.anchorId,
      defaultYaw: getFacingAngle(position, rule.lookAt),
    };
  });
};

export const npcPlacementDefaults = {
  placementRules,
};
