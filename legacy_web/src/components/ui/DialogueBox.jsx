import { useEffect, useMemo, useState } from "react";
import { useDialogueStore } from "../../store/dialogueStore";
import { useFactionStore } from "../../store/factionStore";
import { useGameStore } from "../../store/gameStore";
import { useInventoryStore } from "../../store/inventoryStore";
import { useQuestStore } from "../../store/questStore";
import {
  getDialogueNode,
  getNpcDefinition,
  getVisibleDialogueOptions,
} from "../../utils/dialogueEngine";

const TYPEWRITER_INTERVAL_MS = 18;

export default function DialogueBox() {
  const activeNpcId = useDialogueStore((state) => state.activeNpcId);
  const currentNodeId = useDialogueStore((state) => state.currentNodeId);
  const isOpen = useDialogueStore((state) => state.isOpen);
  const selectOption = useDialogueStore((state) => state.selectOption);
  const closeDialogue = useDialogueStore((state) => state.closeDialogue);
  const questFlags = useGameStore((state) => state.world.questFlags);
  const quests = useQuestStore((state) => state.quests);
  const completedQuestIds = useQuestStore((state) => state.completedQuestIds);
  const inventory = useInventoryStore((state) => state.inventory);
  const equipment = useInventoryStore((state) => state.equipment);
  const factions = useFactionStore((state) => state.factions);
  const [visibleCharacters, setVisibleCharacters] = useState(0);

  const stores = useMemo(
    () => ({
      game: { world: { questFlags } },
      quest: { quests, completedQuestIds },
      inventory: { inventory, equipment },
      faction: { factions },
    }),
    [completedQuestIds, equipment, factions, inventory, questFlags, quests],
  );

  const npc = activeNpcId ? getNpcDefinition(activeNpcId) : null;
  const node = activeNpcId && currentNodeId
    ? getDialogueNode(activeNpcId, currentNodeId)
    : null;
  const options = useMemo(
    () => getVisibleDialogueOptions(node, stores),
    [node, stores],
  );
  const dialogueText = node?.text ?? "";

  useEffect(() => {
    setVisibleCharacters(0);
  }, [activeNpcId, currentNodeId]);

  useEffect(() => {
    if (!isOpen || visibleCharacters >= dialogueText.length) {
      return undefined;
    }

    const timeoutId = window.setTimeout(() => {
      setVisibleCharacters((current) =>
        Math.min(dialogueText.length, current + 2),
      );
    }, TYPEWRITER_INTERVAL_MS);

    return () => window.clearTimeout(timeoutId);
  }, [dialogueText.length, isOpen, visibleCharacters]);

  if (!isOpen || !npc || !node) {
    return null;
  }

  const typedText = dialogueText.slice(0, visibleCharacters);
  const isFinishedTyping = visibleCharacters >= dialogueText.length;

  return (
    <div className="dialogue-box">
      <div className="dialogue-box__portrait" aria-hidden="true">
        {npc.name.slice(0, 1)}
      </div>
      <div className="dialogue-box__content">
        <div className="dialogue-box__header">
          <span className="dialogue-box__name">{npc.name}</span>
          <span className="dialogue-box__role">{npc.role}</span>
        </div>
        <p className="dialogue-box__text">
          {typedText}
          {!isFinishedTyping ? <span className="dialogue-box__cursor">|</span> : null}
        </p>
        <div className="dialogue-box__options">
          {options.length > 0 ? (
            options.map((option, index) => (
              <button
                key={`${currentNodeId}-${index}`}
                className="dialogue-box__option"
                onClick={() => selectOption(index)}
                type="button"
              >
                {option.text}
              </button>
            ))
          ) : (
            <button
              className="dialogue-box__option"
              onClick={closeDialogue}
              type="button"
            >
              Continue
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
