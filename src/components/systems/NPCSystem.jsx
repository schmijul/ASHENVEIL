import { useMemo } from "react";
import { useFrame } from "@react-three/fiber";
import { useGameStore } from "../../store/gameStore";
import { findNearestNpc } from "../../utils/npcInteraction";
import { buildNpcPlacements, NPC_INTERACTION_RADIUS } from "../../utils/npcPlacement";

const CONVERSATION_BREAK_RADIUS = 4.8;

export default function NPCSystem() {
  const placements = useMemo(() => buildNpcPlacements({ seed: 7 }), []);

  useFrame(() => {
    const { player, interaction, setFocusedNpc, endNpcInteraction } =
      useGameStore.getState();

    const nearestNpc = findNearestNpc(
      player.position,
      placements,
      NPC_INTERACTION_RADIUS,
    );
    setFocusedNpc(nearestNpc?.id ?? null);

    if (!interaction.activeNpcId || !interaction.dialogueOpen) {
      return;
    }

    const activeNpc = placements.find((npc) => npc.id === interaction.activeNpcId);
    if (!activeNpc) {
      endNpcInteraction();
      return;
    }

    const distance = Math.hypot(
      activeNpc.position[0] - player.position[0],
      activeNpc.position[2] - player.position[2],
    );

    if (distance > CONVERSATION_BREAK_RADIUS) {
      endNpcInteraction();
    }
  });

  return null;
}
