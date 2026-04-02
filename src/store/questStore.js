import { create } from "zustand";
import questsData from "../data/quests.json" with { type: "json" };

const questCatalog = Object.fromEntries(
  questsData.quests.map((quest) => [quest.id, quest]),
);

const cloneObjective = (objective, index) => ({
  index,
  type: objective.type,
  target: objective.target ?? null,
  item: objective.item ?? null,
  action: objective.action ?? null,
  location: objective.location ?? null,
  count: objective.count ?? 1,
  optional: objective.optional ?? false,
  label: objective.label ?? "",
  progress: 0,
  completed: false,
});

const buildQuestState = (quest) => ({
  id: quest.id,
  title: quest.title,
  description: quest.description,
  phase: quest.phase ?? null,
  giver: quest.giver ?? null,
  optional: quest.optional ?? false,
  prerequisite: quest.prerequisite ?? null,
  autoStart: quest.autoStart ?? false,
  trigger: quest.trigger ?? null,
  status: quest.trigger ? "pending" : quest.autoStart ? "active" : "inactive",
  startedAt: quest.trigger ? null : quest.autoStart ? Date.now() : null,
  completedAt: null,
  objectives: quest.objectives.map(cloneObjective),
  rewards: quest.rewards ?? {},
  onComplete: quest.onComplete ?? null,
});

const initialQuests = Object.fromEntries(
  questsData.quests.map((quest) => [quest.id, buildQuestState(quest)]),
);

const matchesObjective = (objective, event) => {
  if (objective.completed) {
    return false;
  }

  if (objective.type !== event.type) {
    return false;
  }

  if (objective.target && objective.target !== event.target) {
    return false;
  }

  if (objective.item && objective.item !== event.item) {
    return false;
  }

  if (objective.action && objective.action !== event.action) {
    return false;
  }

  if (objective.location && objective.location !== event.location) {
    return false;
  }

  return true;
};

const cloneObjectiveState = (objective) => ({
  ...objective,
});

export const useQuestStore = create((set, get) => ({
  quests: initialQuests,
  activeQuestIds: questsData.quests
    .filter((quest) => !quest.trigger && quest.autoStart)
    .map((quest) => quest.id),
  completedQuestIds: [],
  getQuestDefinition: (questId) => questCatalog[questId] ?? null,
  getQuestState: (questId) => get().quests[questId] ?? null,
  startQuest: (questId) =>
    set((state) => {
      const quest = state.quests[questId];
      if (!quest || quest.status === "completed" || quest.status === "active") {
        return state;
      }

      return {
        quests: {
          ...state.quests,
          [questId]: {
            ...quest,
            status: "active",
            startedAt: Date.now(),
          },
        },
        activeQuestIds: Array.from(new Set([...state.activeQuestIds, questId])),
      };
    }),
  recordObjectiveEvent: (event) =>
    set((state) => {
      let questsChanged = false;
      const nextQuests = { ...state.quests };
      const nextActiveIds = new Set(state.activeQuestIds);

      for (const [questId, quest] of Object.entries(state.quests)) {
        if (quest.status !== "active") {
          continue;
        }

        const nextObjectives = quest.objectives.map((objective) => {
          if (!matchesObjective(objective, event)) {
            return objective;
          }

          questsChanged = true;
          const nextProgress = Math.min(objective.count, objective.progress + (event.count ?? 1));
          return {
            ...objective,
            progress: nextProgress,
            completed: nextProgress >= objective.count,
          };
        });

        const questCompleted = nextObjectives.every(
          (objective) => objective.completed || objective.optional,
        );

        if (questCompleted && quest.status !== "completed") {
          questsChanged = true;
          nextQuests[questId] = {
            ...quest,
            status: "completed",
            completedAt: Date.now(),
            objectives: nextObjectives.map(cloneObjectiveState),
          };
          nextActiveIds.delete(questId);
          continue;
        }

        if (nextObjectives.some((objective, index) => objective !== quest.objectives[index])) {
          questsChanged = true;
          nextQuests[questId] = {
            ...quest,
            objectives: nextObjectives.map(cloneObjectiveState),
          };
        }
      }

      if (!questsChanged) {
        return state;
      }

      return {
        quests: nextQuests,
        activeQuestIds: Array.from(nextActiveIds),
        completedQuestIds: Array.from(
          new Set([
            ...state.completedQuestIds,
            ...Object.values(nextQuests)
              .filter((quest) => quest.status === "completed")
              .map((quest) => quest.id),
          ]),
        ),
      };
    }),
  completeQuest: (questId) =>
    set((state) => {
      const quest = state.quests[questId];
      if (!quest || quest.status === "completed") {
        return state;
      }

      return {
        quests: {
          ...state.quests,
          [questId]: {
            ...quest,
            status: "completed",
            completedAt: Date.now(),
          },
        },
        activeQuestIds: state.activeQuestIds.filter((id) => id !== questId),
        completedQuestIds: Array.from(new Set([...state.completedQuestIds, questId])),
      };
    }),
  updateObjectiveProgress: (questId, objectiveIndex, amount = 1) =>
    set((state) => {
      const quest = state.quests[questId];
      if (!quest || !quest.objectives[objectiveIndex]) {
        return state;
      }

      const objectives = quest.objectives.map((objective, index) => {
        if (index !== objectiveIndex || objective.completed) {
          return objective;
        }

        const nextProgress = Math.min(objective.count, objective.progress + amount);
        return {
          ...objective,
          progress: nextProgress,
          completed: nextProgress >= objective.count,
        };
      });

      return {
        quests: {
          ...state.quests,
          [questId]: {
            ...quest,
            objectives,
          },
        },
      };
    }),
  resetQuestProgress: (questId) =>
    set((state) => {
      const quest = state.quests[questId];
      if (!quest) {
        return state;
      }

      const definition = questCatalog[questId];
      if (!definition) {
        return state;
      }

      return {
        quests: {
          ...state.quests,
          [questId]: buildQuestState(definition),
        },
        activeQuestIds: definition.autoStart
          ? Array.from(new Set([...state.activeQuestIds, questId]))
          : state.activeQuestIds.filter((id) => id !== questId),
        completedQuestIds: state.completedQuestIds.filter((id) => id !== questId),
      };
    }),
  resetAllQuests: () =>
    set({
      quests: initialQuests,
      activeQuestIds: questsData.quests
        .filter((quest) => !quest.trigger && quest.autoStart)
        .map((quest) => quest.id),
      completedQuestIds: [],
    }),
}));

export const questStoreDefaults = {
  questCatalog,
  initialQuests,
};
