import npcsData from "../data/npcs.json" with { type: "json" };
import { NPC_INTERACTION_RADIUS } from "./npcPlacement";

const npcCatalog = Object.fromEntries(npcsData.npcs.map((npc) => [npc.id, npc]));

export const getNpcDefinition = (npcId) => npcCatalog[npcId] ?? null;

export const findNearestNpc = (
  playerPosition,
  placements,
  interactionRadius = NPC_INTERACTION_RADIUS,
) => {
  let nearestNpc = null;
  let nearestDistance = interactionRadius;

  placements.forEach((placement) => {
    const distance = Math.hypot(
      placement.position[0] - playerPosition[0],
      placement.position[2] - playerPosition[2],
    );

    if (distance <= nearestDistance) {
      nearestNpc = placement;
      nearestDistance = distance;
    }
  });

  return nearestNpc;
};

export const getFacingYaw = (fromPosition, targetPosition) =>
  Math.atan2(targetPosition[0] - fromPosition[0], targetPosition[2] - fromPosition[2]);
