using System;
using Ashenveil.Dialog;
using UnityEngine;
using UnityEngine.AI;

namespace Ashenveil.NPC
{
    /// <summary>
    /// Scene-facing NPC runtime controller that applies schedules and exposes dialog requests.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NpcController : MonoBehaviour
    {
        private const string IsMovingParameter = "isMoving";
        private const string IsWorkingParameter = "isWorking";
        private const string IsFleeingParameter = "isFleeing";

        [Header("References")]
        [SerializeField] private NPCData _npcData;
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private Animator _animator;

        private NPCScheduleEntry _currentScheduleEntry;

        public event Action<NpcController, DialogTree> ConversationRequested;
        public event Action<NPCScheduleEntry> ScheduleChanged;

        public NPCData NpcData => _npcData;

        public NPCScheduleEntry CurrentScheduleEntry => _currentScheduleEntry;

        public NPCAction CurrentAction => _currentScheduleEntry != null ? _currentScheduleEntry.Action : NPCAction.Idle;

        public DialogTree DialogTree => _npcData != null ? _npcData.DialogTree : null;

        public string InteractionPrompt => _npcData != null && !string.IsNullOrWhiteSpace(_npcData.NpcName)
            ? $"F: Talk to {_npcData.NpcName}"
            : "F: Talk";

        private void Awake()
        {
            ResolveReferences();
            ApplyBaseMovementSettings();
        }

        private void OnValidate()
        {
            ResolveReferences();
            ApplyBaseMovementSettings();
        }

        public void ApplyTimeOfDay(float hour)
        {
            if (_npcData == null)
            {
                return;
            }

            ApplyScheduleEntry(_npcData.ResolveSchedule(hour));
        }

        public void ApplyScheduleEntry(NPCScheduleEntry entry)
        {
            _currentScheduleEntry = entry;

            if (_navMeshAgent != null)
            {
                ApplyAgentDestination(entry);
            }

            ApplyAnimatorState(entry);
            ScheduleChanged?.Invoke(entry);
        }

        public void RequestConversation()
        {
            DialogTree dialogTree = _npcData != null ? _npcData.DialogTree : null;
            if (dialogTree == null)
            {
                return;
            }

            ConversationRequested?.Invoke(this, dialogTree);
        }

        private void ResolveReferences()
        {
            if (_navMeshAgent == null)
            {
                TryGetComponent(out _navMeshAgent);
            }

            if (_animator == null)
            {
                TryGetComponent(out _animator);
            }
        }

        private void ApplyBaseMovementSettings()
        {
            if (_navMeshAgent == null || _npcData == null)
            {
                return;
            }

            _navMeshAgent.speed = _npcData.MoveSpeed;
            _navMeshAgent.stoppingDistance = _npcData.StoppingDistance;
        }

        private void ApplyAgentDestination(NPCScheduleEntry entry)
        {
            if (_navMeshAgent == null)
            {
                return;
            }

            _navMeshAgent.speed = _npcData != null ? _npcData.MoveSpeed : _navMeshAgent.speed;
            _navMeshAgent.stoppingDistance = _npcData != null ? _npcData.StoppingDistance : _navMeshAgent.stoppingDistance;

            if (entry == null || entry.Waypoint == null)
            {
                _navMeshAgent.ResetPath();
                _navMeshAgent.isStopped = true;
                return;
            }

            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(entry.Waypoint.position);
        }

        private void ApplyAnimatorState(NPCScheduleEntry entry)
        {
            if (_animator == null)
            {
                return;
            }

            NPCAction action = entry != null ? entry.Action : NPCAction.Idle;
            _animator.SetBool(IsMovingParameter, NpcScheduleResolver.IsMoving(action));
            _animator.SetBool(IsWorkingParameter, NpcScheduleResolver.IsWorking(action));
            _animator.SetBool(IsFleeingParameter, NpcScheduleResolver.IsFleeing(action));
        }
    }
}
