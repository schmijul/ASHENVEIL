import { useGameStore } from "../../store/gameStore";
import { getNpcDefinition, getNpcPreviewText } from "../../utils/npcInteraction";

export default function NpcDialoguePreview() {
  const activeNpcId = useGameStore((state) => state.interaction.activeNpcId);
  const dialogueOpen = useGameStore((state) => state.interaction.dialogueOpen);

  if (!dialogueOpen || !activeNpcId) {
    return null;
  }

  const npc = getNpcDefinition(activeNpcId);
  if (!npc) {
    return null;
  }

  return (
    <div className="dialogue-preview">
      <div className="dialogue-preview__header">
        <span className="dialogue-preview__name">{npc.name}</span>
        <span className="dialogue-preview__role">{npc.role}</span>
      </div>
      <p className="dialogue-preview__text">{getNpcPreviewText(activeNpcId)}</p>
      <p className="dialogue-preview__hint">Press E or Escape to close.</p>
    </div>
  );
}
