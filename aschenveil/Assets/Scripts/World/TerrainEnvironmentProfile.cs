using UnityEngine;

namespace Ashenveil.World
{
    /// <summary>
    /// Terrain and forest tuning values for the Grauwald demo area.
    /// Referenced GDD section: 3.2
    /// </summary>
    [CreateAssetMenu(fileName = "TerrainEnvironmentProfile", menuName = "Ashenveil/World/Terrain Environment Profile")]
    public class TerrainEnvironmentProfile : ScriptableObject
    {
        [Header("Terrain")]
        [SerializeField] private Vector3 _terrainSize = new Vector3(500f, 100f, 500f);
        [SerializeField, Min(33)] private int _heightmapResolution = 513;
        [SerializeField, Min(16)] private int _alphamapResolution = 512;
        [SerializeField, Min(1)] private int _baseDetailResolution = 1024;
        [SerializeField, Min(1)] private int _detailResolutionPerPatch = 8;

        [Header("Forest")]
        [SerializeField, Min(0f)] private float _detailDensity = 0.8f;
        [SerializeField, Min(0f)] private float _detailDistance = 80f;
        [SerializeField, Min(0)] private int _seed = 1337;
        [SerializeField, Min(1)] private int _treeCount = 72;
        [SerializeField, Min(0)] private int _rockCount = 18;
        [SerializeField, Min(0f)] private float _treeRadius = 210f;
        [SerializeField, Min(0f)] private float _rockRadius = 220f;
        [SerializeField, Min(0f)] private float _minTreeScale = 0.8f;
        [SerializeField, Min(0f)] private float _maxTreeScale = 1.35f;
        [SerializeField, Min(0f)] private float _minRockScale = 0.9f;
        [SerializeField, Min(0f)] private float _maxRockScale = 1.6f;
        [SerializeField, Min(0)] private int _foliageCount = 120;
        [SerializeField, Min(0f)] private float _foliageRadius = 235f;
        [SerializeField, Min(0f)] private float _minFoliageScale = 0.65f;
        [SerializeField, Min(0f)] private float _maxFoliageScale = 1.1f;

        [Header("Terrain Layers")]
        [SerializeField] private TerrainLayer[] _terrainLayers = new TerrainLayer[4];

        [Header("Placement")]
        [SerializeField] private GameObject[] _treePrefabs;
        [SerializeField] private GameObject[] _rockPrefabs;
        [SerializeField] private GameObject[] _foliagePrefabs;

        public Vector3 TerrainSize => _terrainSize;
        public int HeightmapResolution => _heightmapResolution;
        public int AlphamapResolution => _alphamapResolution;
        public int BaseDetailResolution => _baseDetailResolution;
        public int DetailResolutionPerPatch => _detailResolutionPerPatch;
        public float DetailDensity => _detailDensity;
        public float DetailDistance => _detailDistance;
        public int Seed => _seed;
        public int TreeCount => _treeCount;
        public int RockCount => _rockCount;
        public float TreeRadius => _treeRadius;
        public float RockRadius => _rockRadius;
        public float MinTreeScale => _minTreeScale;
        public float MaxTreeScale => _maxTreeScale;
        public float MinRockScale => _minRockScale;
        public float MaxRockScale => _maxRockScale;
        public int FoliageCount => _foliageCount;
        public float FoliageRadius => _foliageRadius;
        public float MinFoliageScale => _minFoliageScale;
        public float MaxFoliageScale => _maxFoliageScale;
        public TerrainLayer[] TerrainLayers => _terrainLayers;
        public GameObject[] TreePrefabs => _treePrefabs;
        public GameObject[] RockPrefabs => _rockPrefabs;
        public GameObject[] FoliagePrefabs => _foliagePrefabs;

        public static TerrainEnvironmentProfile CreateRuntimeDefaults()
        {
            return CreateInstance<TerrainEnvironmentProfile>();
        }

        public bool MatchesGddTerrainTarget()
        {
            return TerrainEnvironmentValidation.MatchesGddTerrainTarget(this);
        }

        public bool HasRequiredLayerCount()
        {
            return TerrainEnvironmentValidation.HasMinimumLayerCount(this, 4);
        }
    }
}
