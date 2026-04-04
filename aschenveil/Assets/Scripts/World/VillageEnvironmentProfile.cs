using UnityEngine;

namespace Ashenveil.World
{
    /// <summary>
    /// Data profile for the Grauweiler village footprint and prefab mapping.
    /// Referenced GDD section: 3.3
    /// </summary>
    [CreateAssetMenu(fileName = "VillageEnvironmentProfile", menuName = "Ashenveil/World/Village Environment Profile")]
    public class VillageEnvironmentProfile : ScriptableObject
    {
        [Header("Layout")]
        [SerializeField] private Vector3 _villageCenter = new Vector3(340f, 0f, 150f);
        [SerializeField] private Vector2 _villageFootprint = new Vector2(150f, 118f);
        [SerializeField] private Vector2 _centralSquareSize = new Vector2(30f, 30f);
        [SerializeField, Min(1f)] private float _wallSegmentLength = 7.5f;
        [SerializeField, Min(1f)] private float _gateWidth = 18f;
        [SerializeField, Min(0f)] private float _terrainBlendDistance = 18f;
        [SerializeField, Min(0)] private int _seed = 2404;

        [Header("Buildings")]
        [SerializeField] private GameObject _elderHousePrefab;
        [SerializeField] private GameObject _traderShopPrefab;
        [SerializeField] private GameObject _blacksmithPrefab;
        [SerializeField] private GameObject _healerHutPrefab;
        [SerializeField] private GameObject _tavernPrefab;
        [SerializeField] private GameObject[] _residencePrefabs = new GameObject[7];
        [SerializeField] private GameObject _storageBarnPrefab;
        [SerializeField] private GameObject _watchTowerPrefab;
        [SerializeField] private GameObject _stablePrefab;

        [Header("Village Structures")]
        [SerializeField] private GameObject _wallPanelPrefab;
        [SerializeField] private GameObject _wallCornerPrefab;
        [SerializeField] private GameObject _gatePrefab;
        [SerializeField] private GameObject _fencePrefab;
        [SerializeField] private GameObject _pathPrefab;
        [SerializeField] private GameObject[] _ambientProps;

        public Vector3 VillageCenter => _villageCenter;
        public Vector2 VillageFootprint => _villageFootprint;
        public Vector2 CentralSquareSize => _centralSquareSize;
        public float WallSegmentLength => _wallSegmentLength;
        public float GateWidth => _gateWidth;
        public float TerrainBlendDistance => _terrainBlendDistance;
        public int Seed => _seed;
        public GameObject ElderHousePrefab => _elderHousePrefab;
        public GameObject TraderShopPrefab => _traderShopPrefab;
        public GameObject BlacksmithPrefab => _blacksmithPrefab;
        public GameObject HealerHutPrefab => _healerHutPrefab;
        public GameObject TavernPrefab => _tavernPrefab;
        public GameObject[] ResidencePrefabs => _residencePrefabs;
        public GameObject StorageBarnPrefab => _storageBarnPrefab;
        public GameObject WatchTowerPrefab => _watchTowerPrefab;
        public GameObject StablePrefab => _stablePrefab;
        public GameObject WallPanelPrefab => _wallPanelPrefab;
        public GameObject WallCornerPrefab => _wallCornerPrefab;
        public GameObject GatePrefab => _gatePrefab;
        public GameObject FencePrefab => _fencePrefab;
        public GameObject PathPrefab => _pathPrefab;
        public GameObject[] AmbientProps => _ambientProps;

        public static VillageEnvironmentProfile CreateRuntimeDefaults()
        {
            return CreateInstance<VillageEnvironmentProfile>();
        }
    }
}
