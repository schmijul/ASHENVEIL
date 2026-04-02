import { useDialogueStore } from "../../store/dialogueStore";
import { getNpcDefinition } from "../../utils/dialogueEngine";

export default function InteractionPrompt() {
  const focusedNpcId = useDialogueStore((state) => state.focusedNpcId);
  const dialogueOpen = useDialogueStore((state) => state.isOpen);

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
