import npcsData from "../data/npcs.json" with { type: "json" };

const npcCatalog = Object.fromEntries(npcsData.npcs.map((npc) => [npc.id, npc]));

const startNodePriority = {
  maren: ["village_destroyed", "hunt_complete", "initial_wakeup"],
  korvin: ["after_first_sale", "greeting"],
  hagen: ["greeting"],
  lotte: ["herbs_collected", "greeting"],
  ren: ["training_complete", "greeting"],
};

const startNodeGuards = {
  maren: {
    village_destroyed: (stores) =>
      Boolean(
        stores.game.world.destroyedVillage ||
          stores.game.world.questFlags?.destroyedVillage ||
          stores.game.world.questFlags?.prolog_complete,
      ),
  },
};

const normalizeItemList = (value) =>
  Array.isArray(value) ? value : value ? [value] : [];

const getQuestState = (stores, questId) =>
  stores.quest.getQuestState?.(questId) ?? stores.quest.quests?.[questId] ?? null;

const hasCompletedQuest = (stores, questId) => {
  const quest = getQuestState(stores, questId);
  return (
    stores.quest.completedQuestIds?.includes(questId) ||
    quest?.status === "completed"
  );
};

const hasInventoryItem = (stores, itemId, quantity = 1) => {
  const inventoryQuantity =
    stores.inventory.inventory?.find((entry) => entry.itemId === itemId)?.quantity ?? 0;
  const equippedQuantity = Object.values(stores.inventory.equipment ?? {}).filter(
    (equippedId) => equippedId === itemId,
  ).length;

  return inventoryQuantity + equippedQuantity >= quantity;
};

const getFactionValue = (stores, factionId) =>
  stores.faction.factions?.[factionId] ?? 0;

const getNodeKey = (npcId, nodeId) => `${npcId}:${nodeId}`;

export const getNpcDefinition = (npcId) => npcCatalog[npcId] ?? null;

export const getDialogueNode = (npcId, nodeId) =>
  getNpcDefinition(npcId)?.dialogue?.[nodeId] ?? null;

export const evaluateDialogueConditions = (conditions, stores) => {
  if (!conditions) {
    return true;
  }

  if (conditions.questFlag) {
    const flags = stores.game.world.questFlags ?? {};
    const requiredFlags = Array.isArray(conditions.questFlag)
      ? conditions.questFlag
      : [conditions.questFlag];
    if (!requiredFlags.every((flag) => Boolean(flags[flag]))) {
      return false;
    }
  }

  if (conditions.questComplete) {
    const quests = Array.isArray(conditions.questComplete)
      ? conditions.questComplete
      : [conditions.questComplete];
    if (!quests.every((questId) => hasCompletedQuest(stores, questId))) {
      return false;
    }
  }

  if (conditions.hasItem) {
    const requiredItems = normalizeItemList(conditions.hasItem);
    if (!requiredItems.every((itemId) => hasInventoryItem(stores, itemId))) {
      return false;
    }
  }

  const reputationConditions =
    conditions.factionRep ?? conditions.reputation ?? conditions.factionReputation;
  if (reputationConditions && typeof reputationConditions === "object") {
    const reputationChecks = Object.entries(reputationConditions);
    if (!reputationChecks.every(([factionId, minimum]) => getFactionValue(stores, factionId) >= minimum)) {
      return false;
    }
  }

  return true;
};

export const getVisibleDialogueOptions = (node, stores) =>
  (node?.options ?? []).filter((option) =>
    evaluateDialogueConditions(option.conditions, stores),
  );

export const getStartNodeId = (npcId, stores) => {
  const npc = getNpcDefinition(npcId);
  if (!npc) {
    return null;
  }

  const preferredNodes = startNodePriority[npcId] ?? [];
  for (const nodeId of preferredNodes) {
    const node = npc.dialogue?.[nodeId];
    const passesGuard =
      startNodeGuards[npcId]?.[nodeId]?.(stores) ?? true;
    if (node && passesGuard && evaluateDialogueConditions(node.conditions, stores)) {
      return nodeId;
    }
  }

  return Object.entries(npc.dialogue ?? {}).find(([, node]) =>
    evaluateDialogueConditions(node.conditions, stores),
  )?.[0] ?? null;
};

const applyItems = (stores, node, mode) => {
  const itemIds = normalizeItemList(node?.[mode === "give" ? "giveItem" : "removeItem"]);
  itemIds.forEach((itemId) => {
    if (mode === "give") {
      const itemDefinition = stores.inventory.getItemDefinition?.(itemId);
      const isStackable = Boolean(itemDefinition?.stackable);
      if (!isStackable && hasInventoryItem(stores, itemId)) {
        return;
      }

      stores.inventory.addItem(itemId, 1);
      if (
        itemDefinition?.type === "weapon" &&
        !stores.inventory.equipment?.weapon
      ) {
        stores.inventory.equipItem("weapon", itemId);
      }
      return;
    }

    if (hasInventoryItem(stores, itemId)) {
      stores.inventory.removeItem(itemId, 1);
    }
  });
};

export const executeDialogueAction = (action, stores, npcId) => {
  if (!action) {
    return { closeDialogue: false, requestTradeNpcId: null };
  }

  if (action === "openTrade") {
    return {
      closeDialogue: true,
      requestTradeNpcId: npcId,
    };
  }

  if (action === "closeDialogue") {
    return { closeDialogue: true, requestTradeNpcId: null };
  }

  if (action.startsWith("startQuest:")) {
    stores.quest.startQuest(action.split(":")[1]);
    return { closeDialogue: false, requestTradeNpcId: null };
  }

  if (action.startsWith("completeQuest:")) {
    stores.quest.completeQuest(action.split(":")[1]);
    return { closeDialogue: false, requestTradeNpcId: null };
  }

  if (action.startsWith("setFlag:")) {
    stores.game.setQuestFlag(action.split(":")[1], true);
    return { closeDialogue: false, requestTradeNpcId: null };
  }

  if (action.startsWith("changeReputation:")) {
    const [, factionId, delta] = action.split(":");
    stores.faction.adjustReputation(factionId, Number(delta ?? 0));
    return { closeDialogue: false, requestTradeNpcId: null };
  }

  console.warn(`Unhandled dialogue action: ${action}`);
  return { closeDialogue: false, requestTradeNpcId: null };
};

const enterNode = ({ npcId, nodeId, stores, visitedNodeKeys = [] }) => {
  const node = getDialogueNode(npcId, nodeId);
  if (!node || !evaluateDialogueConditions(node.conditions, stores)) {
    return {
      currentNodeId: null,
      visitedNodeKeys,
      closeDialogue: true,
      requestTradeNpcId: null,
    };
  }

  const nodeKey = getNodeKey(npcId, nodeId);
  let sideEffects = { closeDialogue: false, requestTradeNpcId: null };
  const nextVisitedNodeKeys = visitedNodeKeys.includes(nodeKey)
    ? visitedNodeKeys
    : [...visitedNodeKeys, nodeKey];

  if (!visitedNodeKeys.includes(nodeKey)) {
    applyItems(stores, node, "give");
    applyItems(stores, node, "remove");
    sideEffects = executeDialogueAction(node.action, stores, npcId);
  }

  return {
    currentNodeId: nodeId,
    visitedNodeKeys: nextVisitedNodeKeys,
    closeDialogue: sideEffects.closeDialogue,
    requestTradeNpcId: sideEffects.requestTradeNpcId,
  };
};

export const openDialogueFlow = ({ npcId, stores }) => {
  const startNodeId = getStartNodeId(npcId, stores);
  if (!startNodeId) {
    return {
      npcId: null,
      currentNodeId: null,
      visitedNodeKeys: [],
      closeDialogue: true,
      requestTradeNpcId: null,
    };
  }

  return enterNode({
    npcId,
    nodeId: startNodeId,
    stores,
    visitedNodeKeys: [],
  });
};

export const advanceDialogueFlow = ({
  npcId,
  nodeId,
  optionIndex,
  stores,
  visitedNodeKeys = [],
}) => {
  const node = getDialogueNode(npcId, nodeId);
  if (!node) {
    return {
      currentNodeId: null,
      visitedNodeKeys,
      closeDialogue: true,
      requestTradeNpcId: null,
    };
  }

  const options = getVisibleDialogueOptions(node, stores);
  const option = options[optionIndex];
  if (!option) {
    return {
      currentNodeId: nodeId,
      visitedNodeKeys,
      closeDialogue: false,
      requestTradeNpcId: null,
    };
  }

  const optionEffects = executeDialogueAction(option.action, stores, npcId);
  if (option.next) {
    const nextNodeState = enterNode({
      npcId,
      nodeId: option.next,
      stores,
      visitedNodeKeys,
    });

    return {
      currentNodeId: nextNodeState.currentNodeId,
      visitedNodeKeys: nextNodeState.visitedNodeKeys,
      closeDialogue: optionEffects.closeDialogue || nextNodeState.closeDialogue,
      requestTradeNpcId:
        optionEffects.requestTradeNpcId ?? nextNodeState.requestTradeNpcId,
    };
  }

  return {
    currentNodeId: nodeId,
    visitedNodeKeys,
    closeDialogue: optionEffects.closeDialogue,
    requestTradeNpcId: optionEffects.requestTradeNpcId,
  };
};
