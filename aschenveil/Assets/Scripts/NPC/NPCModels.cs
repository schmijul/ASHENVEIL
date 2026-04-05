using System;
using System.Collections.Generic;
using System.Linq;
using Ashenveil.Dialog;
using UnityEngine;

namespace Ashenveil.NPC
{
    /// <summary>
    /// NPC schedule actions used by animation and navigation.
    /// </summary>
    public enum NPCAction
    {
        Idle = 0,
        Walk = 1,
        Work = 2,
        Sleep = 3,
        Flee = 4
    }

    /// <summary>
    /// A time window and location/action for one NPC schedule entry.
    /// </summary>
    [Serializable]
    public sealed class NPCScheduleEntry
    {
        [Header("Timing")]
        [SerializeField, Min(0f)] private float _startHour;
        [SerializeField, Min(0f)] private float _endHour = 24f;

        [Header("Destination")]
        [SerializeField] private Transform _waypoint;

        [Header("Behavior")]
        [SerializeField] private NPCAction _action = NPCAction.Idle;

        public float StartHour => _startHour;

        public float EndHour => _endHour;

        public Transform Waypoint => _waypoint;

        public NPCAction Action => _action;

        public static NPCScheduleEntry Create(float startHour, float endHour, NPCAction action, Transform waypoint = null)
        {
            return new NPCScheduleEntry
            {
                _startHour = Mathf.Clamp(startHour, 0f, 24f),
                _endHour = Mathf.Clamp(endHour, 0f, 24f),
                _action = action,
                _waypoint = waypoint
            };
        }

        public bool ContainsHour(float hour)
        {
            float normalizedHour = NormalizeHour(hour);
            float normalizedStart = NormalizeHour(_startHour);
            float normalizedEnd = NormalizeHour(_endHour);

            if (Mathf.Approximately(normalizedStart, normalizedEnd))
            {
                return true;
            }

            if (normalizedStart < normalizedEnd)
            {
                return normalizedHour >= normalizedStart && normalizedHour < normalizedEnd;
            }

            return normalizedHour >= normalizedStart || normalizedHour < normalizedEnd;
        }

        private static float NormalizeHour(float hour)
        {
            float normalizedHour = hour % 24f;
            if (normalizedHour < 0f)
            {
                normalizedHour += 24f;
            }

            return normalizedHour;
        }
    }

    /// <summary>
    /// Resolves the active schedule entry for a given in-game hour.
    /// </summary>
    public static class NpcScheduleResolver
    {
        public static NPCScheduleEntry Resolve(IReadOnlyList<NPCScheduleEntry> schedule, float hour)
        {
            if (schedule == null || schedule.Count == 0)
            {
                return null;
            }

            float normalizedHour = NormalizeHour(hour);
            NPCScheduleEntry fallback = null;
            float fallbackStart = float.NegativeInfinity;

            for (int index = 0; index < schedule.Count; index++)
            {
                NPCScheduleEntry entry = schedule[index];
                if (entry == null)
                {
                    continue;
                }

                if (entry.ContainsHour(normalizedHour))
                {
                    return entry;
                }

                float entryStart = NormalizeHour(entry.StartHour);
                if (entryStart <= normalizedHour && entryStart >= fallbackStart)
                {
                    fallback = entry;
                    fallbackStart = entryStart;
                }
            }

            if (fallback != null)
            {
                return fallback;
            }

            return schedule.FirstOrDefault(entry => entry != null);
        }

        public static NPCAction ResolveAction(IReadOnlyList<NPCScheduleEntry> schedule, float hour)
        {
            NPCScheduleEntry entry = Resolve(schedule, hour);
            return entry != null ? entry.Action : NPCAction.Idle;
        }

        public static bool IsMoving(NPCAction action)
        {
            return action == NPCAction.Walk;
        }

        public static bool IsWorking(NPCAction action)
        {
            return action == NPCAction.Work;
        }

        public static bool IsFleeing(NPCAction action)
        {
            return action == NPCAction.Flee;
        }

        private static float NormalizeHour(float hour)
        {
            float normalizedHour = hour % 24f;
            if (normalizedHour < 0f)
            {
                normalizedHour += 24f;
            }

            return normalizedHour;
        }
    }

    /// <summary>
    /// ScriptableObject data for a village NPC.
    /// </summary>
    [CreateAssetMenu(fileName = "NPCData", menuName = "Ashenveil/NPC/NPC Data")]
    public sealed class NPCData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _npcName = "NPC";
        [SerializeField] private string _title = string.Empty;
        [SerializeField] private Sprite _portrait;

        [Header("Schedule")]
        [SerializeField] private NPCScheduleEntry[] _schedule = Array.Empty<NPCScheduleEntry>();
        [SerializeField] private DialogTree _dialogTree;
        [SerializeField] private bool _isEssential = true;

        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float _moveSpeed = 1.5f;
        [SerializeField, Min(0f)] private float _stoppingDistance = 0.5f;

        public string NpcName => _npcName;

        public string Title => _title;

        public Sprite Portrait => _portrait;

        public IReadOnlyList<NPCScheduleEntry> Schedule => _schedule ?? Array.Empty<NPCScheduleEntry>();

        public DialogTree DialogTree => _dialogTree;

        public bool IsEssential => _isEssential;

        public float MoveSpeed => _moveSpeed;

        public float StoppingDistance => _stoppingDistance;

        public static NPCData CreateRuntimeDefaults()
        {
            NPCData npcData = CreateInstance<NPCData>();
            npcData.hideFlags = HideFlags.HideAndDontSave;
            npcData._schedule = Array.Empty<NPCScheduleEntry>();
            return npcData;
        }

        public void SetSchedule(IEnumerable<NPCScheduleEntry> entries)
        {
            _schedule = entries?.ToArray() ?? Array.Empty<NPCScheduleEntry>();
        }

        public void SetDialogTree(DialogTree dialogTree)
        {
            _dialogTree = dialogTree;
        }

        public NPCScheduleEntry ResolveSchedule(float hour)
        {
            return NpcScheduleResolver.Resolve(_schedule, hour);
        }

        public NPCAction ResolveAction(float hour)
        {
            return NpcScheduleResolver.ResolveAction(_schedule, hour);
        }
    }
}
