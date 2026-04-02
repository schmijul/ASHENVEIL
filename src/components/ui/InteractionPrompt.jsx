import { useGameStore } from "../../store/gameStore";
import { getNpcDefinition } from "../../utils/npcInteraction";

export default function InteractionPrompt() {
  const focusedNpcId = useGameStore((state) => state.interaction.focusedNpcId);
  const dialogueOpen = useGameStore((state) => state.interaction.dialogueOpen);

  if (!focusedNpcId || dialogueOpen) {
    return null;
  }

  const npc = getNpcDefinition(focusedNpcId);
  if (!npc) {
    return null;
  }

  return (
    <div className="interaction-prompt">
      <span className="interaction-prompt__key">E</span>
      <span>Talk to {npc.name}</span>
    </div>
  );
}
