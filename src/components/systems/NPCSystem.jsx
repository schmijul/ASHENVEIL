import { useMemo } from "react";
import { useFrame } from "@react-three/fiber";
import { useDialogueStore } from "../../store/dialogueStore";
import { useGameStore } from "../../store/gameStore";
import { findNearestNpc } from "../../utils/npcInteraction";
import { buildNpcPlacements, NPC_INTERACTION_RADIUS } from "../../utils/npcPlacement";

const CONVERSATION_BREAK_RADIUS = 4.8;

export default function NPCSystem() {
  const placements = useMemo(() => buildNpcPlacements({ seed: 7 }), []);

  useFrame(() => {
    const { player } = useGameStore.getState();
    const dialogue = useDialogueStore.getState();

    const nearestNpc = findNearestNpc(
      player.position,
      placements,
      NPC_INTERACTION_RADIUS,
    );
    dialogue.setFocusedNpc(nearestNpc?.id ?? null);

    if (!dialogue.activeNpcId || !dialogue.isOpen) {
      return;
    }

    const activeNpc = placements.find((npc) => npc.id === dialogue.activeNpcId);
    if (!activeNpc) {
      dialogue.closeDialogue();
      return;
    }

    const distance = Math.hypot(
      activeNpc.position[0] - player.position[0],
      activeNpc.position[2] - player.position[2],
    );

    if (distance > CONVERSATION_BREAK_RADIUS) {
      dialogue.closeDialogue();
    }
  });

  return null;
}
