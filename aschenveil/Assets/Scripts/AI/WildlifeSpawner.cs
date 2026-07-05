using System.Collections.Generic;
using UnityEngine;

namespace Ashenveil.AI
{
    /// <summary>
    /// Simple wildlife spawner for one species with max-alive and respawn timing.
    /// Referenced GDD section: Kernsysteme / Wildlife.
    /// </summary>
    public sealed class WildlifeSpawner : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private WildlifeSpecies _species;
        [SerializeField] private WildlifeAgent _agentPrefab;
        [SerializeField] private int _maxAlive = 3;
        [SerializeField] private float _respawnTimer = 18f;

        [Header("References")]
        [SerializeField] private Transform _threat;
        [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();

        private readonly List<WildlifeAgent> _alive = new List<WildlifeAgent>();
        private float _respawnRemaining;
        private int _nextSpawnIndex;

        private void Awake()
        {
            if (_species == null)
            {
                Debug.LogError("WildlifeSpawner requires a WildlifeSpecies.", this);
                enabled = false;
                return;
            }

            if (_agentPrefab == null)
            {
                Debug.LogError("WildlifeSpawner requires a WildlifeAgent prefab.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            RemoveDeadEntries();
            if (_alive.Count >= Mathf.Max(0, _maxAlive) || _spawnPoints.Count == 0)
            {
                return;
            }

            _respawnRemaining -= Time.deltaTime;
            if (_respawnRemaining > 0f)
            {
                return;
            }

            SpawnOne();
            _respawnRemaining = Mathf.Max(0.1f, _respawnTimer);
        }

        private void SpawnOne()
        {
            Transform spawnPoint = _spawnPoints[_nextSpawnIndex % _spawnPoints.Count];
            _nextSpawnIndex++;
            if (spawnPoint == null)
            {
                return;
            }

            WildlifeAgent agent = Instantiate(_agentPrefab, spawnPoint.position, spawnPoint.rotation);
            agent.Initialize(_species, _threat);
            _alive.Add(agent);
        }

        private void RemoveDeadEntries()
        {
            for (int i = _alive.Count - 1; i >= 0; i--)
            {
                if (_alive[i] == null || _alive[i].Health == null || !_alive[i].Health.IsAlive)
                {
                    _alive.RemoveAt(i);
                }
            }
        }
    }
}
