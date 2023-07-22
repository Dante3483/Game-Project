using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.XR;

public class GameManager : MonoBehaviour
{
    #region Private fields
    [Header("Main")]
    [SerializeField] private GameState _gameState;
    [SerializeField] private TerrainConfigurationSO _terrainConfiguration;
    private WorldCellData[,] _worldData;
    private Chunk[,] _chunks;

    [Header("World data")]
    [SerializeField] private int _maxTerrainWidth;
    [SerializeField] private int _maxTerrainHeight;
    [SerializeField] private int _currentTerrainWidth;
    [SerializeField] private int _currentTerrainHeight;

    [Header("Session data")]
    [SerializeField] private int _seed;
    [SerializeField] private System.Random _randomVar;
    private string _generalInfo;
    private float _loadingValue;
    private bool _isGameSession;

    [Header("Atlasses")]
    [SerializeField] private ObjectsAtlass _objectsAtlass;

    [Header("Terrain")]
    [SerializeField] private GameObject _terrainGameObject;
    private Terrain _terrain;

    [Header("UI")]
    [SerializeField] private Canvas _mainMenu;
    [SerializeField] private Canvas _loadingProgress;
    [SerializeField] private Slider _loadingSlider;
    [SerializeField] private TextMeshProUGUI _infoText;

    [Header("Conditions")]
    [SerializeField] private bool _isStaticSeed;
    [SerializeField] private bool _isMenuActive;
    [SerializeField] private bool _isLoadingProgressActive;
    #endregion

    #region Public fields
    public static GameManager Instance;
    #endregion

    #region Properties
    public TerrainConfigurationSO TerrainConfiguration
    {
        get
        {
            return _terrainConfiguration;
        }

        set
        {
            _terrainConfiguration = value;
        }
    }

    public ObjectsAtlass ObjectsAtlass
    {
        get
        {
            return _objectsAtlass;
        }

        set
        {
            _objectsAtlass = value;
        }
    }

    public int Seed
    {
        get
        {
            return _seed;
        }

        set
        {
            _seed = value;
        }
    }

    public Terrain Terrain
    {
        get
        {
            return _terrain;
        }

        set
        {
            _terrain = value;
        }
    }

    public int CurrentTerrainWidth
    {
        get
        {
            return _currentTerrainWidth;
        }

        set
        {
            _currentTerrainWidth = value;
        }
    }

    public int CurrentTerrainHeight
    {
        get
        {
            return _currentTerrainHeight;
        }

        set
        {
            _currentTerrainHeight = value;
        }
    }

    public WorldCellData[,] WorldData
    {
        get
        {
            return _worldData;
        }

        set
        {
            _worldData = value;
        }
    }

    public Chunk[,] Chunks
    {
        get
        {
            return _chunks;
        }

        set
        {
            _chunks = value;
        }
    }

    public bool IsStaticSeed
    {
        get
        {
            return _isStaticSeed;
        }

        set
        {
            _isStaticSeed = value;
        }
    }

    public System.Random RandomVar
    {
        get
        {
            return _randomVar;
        }

        set
        {
            _randomVar = value;
        }
    }

    public string GeneralInfo
    {
        get
        {
            return _generalInfo;
        }

        set
        {
            _generalInfo = value;
        }
    }

    public bool IsMenuActive
    {
        get
        {
            return _isMenuActive;
        }

        set
        {
            _isMenuActive = value;
        }
    }

    public bool IsLoadingProgressActive
    {
        get
        {
            return _isLoadingProgressActive;
        }

        set
        {
            _isLoadingProgressActive = value;
        }
    }

    public float LoadingValue
    {
        get
        {
            return _loadingValue;
        }

        set
        {
            _loadingValue = value;
        }
    }

    public bool IsGameSession
    {
        get
        {
            return _isGameSession;
        }

        set
        {
            _isGameSession = value;
        }
    }
    #endregion

    #region Methods

    #region General
    private void OnApplicationQuit()
    {
        IsGameSession = false;
    }

    private void Awake()
    {
        Instance = this;
        _terrain = _terrainGameObject.GetComponent<Terrain>();
        _terrainGameObject.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(PrintDebugInfo());
        StartCoroutine(UpdateObjects());

        UpdateGameState(GameState.GameInitializationState);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            SaveMapToPNG();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Task.Run(() => DisplayChunks());
        }
        Move();
    }

    public void UpdateGameState(object obj)
    {
        _gameState = (GameState) obj;

        switch (_gameState)
        {
            case GameState.GameInitializationState:
                {
                    Task.Run(() => HandleGameInitialization());
                }
                break;
            case GameState.MainMenuState:
                {
                    HandleMainMenu();
                }
                break;
            case GameState.NewGameState:
                {
                    //Set random seed
                    IsGameSession = false;
                    if (!IsStaticSeed)
                    {
                        Seed = UnityEngine.Random.Range(-1000000, 1000000);
                    }
                    _terrainGameObject.SetActive(true);
                    Task.Run(() => HandleNewGameState());
                    _terrain.StartCoroutinesAndThreads();
                }
                break;
            default:
                break;
        }
    }

    private void HandleGameInitialization()
    {
        try
        {
            Debug.Log("Initialization state");
            IsMenuActive = false;
            IsLoadingProgressActive = true;

            //Define Terrain width and height
            _maxTerrainWidth = TerrainConfiguration.DefaultHorizontalChunksCount * TerrainConfiguration.ChunkSize;
            _maxTerrainHeight = TerrainConfiguration.DefaultVerticalChunksCount * TerrainConfiguration.ChunkSize;

            CurrentTerrainWidth = TerrainConfiguration.CurrentHorizontalChunksCount * TerrainConfiguration.ChunkSize;
            CurrentTerrainHeight = TerrainConfiguration.CurrentVerticalChunksCount * TerrainConfiguration.ChunkSize;

            //Initialize 2d world data and chunk array
            var watch = System.Diagnostics.Stopwatch.StartNew();

            WorldData = new WorldCellData[_maxTerrainWidth, _maxTerrainHeight];
            float step = 50f / _maxTerrainWidth;
            for (ushort x = 0; x < _maxTerrainWidth; x++)
            {
                for (ushort y = 0; y < _maxTerrainHeight; y++)
                {
                    WorldData[x, y] = new WorldCellData(x, y);
                }
                LoadingValue += step;
            }

            Chunks = new Chunk[TerrainConfiguration.CurrentHorizontalChunksCount, TerrainConfiguration.CurrentVerticalChunksCount];
            step = 50f / TerrainConfiguration.CurrentHorizontalChunksCount;
            for (byte x = 0; x < TerrainConfiguration.CurrentHorizontalChunksCount; x++)
            {
                for (byte y = 0; y < TerrainConfiguration.CurrentVerticalChunksCount; y++)
                {
                    Chunks[x, y] = new Chunk(x, y);
                }
                LoadingValue += step;
            }

            watch.Stop();
            Debug.Log($"Game initialization: {watch.Elapsed.TotalSeconds}");
            GeneralInfo += $"Game initialization: {watch.Elapsed.TotalSeconds}\n";

            //Initialize atlasses
            ObjectsAtlass.Initialize();

            UpdateGameState(GameState.MainMenuState);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            throw e;
        }
    }

    private void HandleMainMenu()
    {
        Debug.Log("Menu state");
        IsMenuActive = true;
        IsLoadingProgressActive = false;
    }

    private void HandleNewGameState()
    {
        Debug.Log("New game state");

        IsMenuActive = false;
        IsLoadingProgressActive = true;
        LoadingValue = 0;

        //Create new world
        Terrain.CreateNewWorld(ref _worldData);
        IsLoadingProgressActive = false;
    }
    #endregion

    #region Helpful

    #region World data
    public ref WorldCellData GetWorldCellDataRef(int x, int y)
    {
        return ref WorldData[x, y];
    }
    #endregion

    #region Chunk
    private void DisplayChunks()
    {
        foreach (Chunk chunk in Chunks)
        {
            Debug.Log(chunk.ToString());
        }
    }

    public Chunk GetChunk(int x, int y)
    {
        return Chunks[x / TerrainConfiguration.ChunkSize, y / TerrainConfiguration.ChunkSize];
    }

    public BiomeSO GetChunkBiome(int x, int y)
    {
        return GetChunk(x, y).Biome;
    }

    public void SetChunkBiome(int x, int y, BiomeSO biome)
    {
        Chunk chunk = GetChunk(x, y);
        if (chunk.Biome.Id == BiomesID.NonBiom)
        {
            Chunks[x / TerrainConfiguration.ChunkSize, y / TerrainConfiguration.ChunkSize].Biome = biome;
        }
    }

    public void SetChunkActivityByWorldCoords(int x, int y, bool activity)
    {
        Chunks[x / TerrainConfiguration.ChunkSize, y / TerrainConfiguration.ChunkSize].Activity = activity;
    }

    public void SetChunkActivityByChunkCoords(int x, int y, bool activity)
    {
        Chunks[x, y].Activity = activity;
    }

    public bool IsChunkBiomed(int x, int y, BiomesID id)
    {
        return GetChunk(x, y).Biome.Id == id;
    }
    #endregion

    #region Other
    private void SaveMapToPNG()
    {
        Texture2D worldMap = new Texture2D(CurrentTerrainWidth, CurrentTerrainHeight);
        Texture2D biomesMap = new Texture2D(CurrentTerrainWidth, CurrentTerrainHeight);
        for (int x = 0; x < CurrentTerrainWidth; x++)
        {
            for (int y = 0; y < CurrentTerrainHeight; y++)
            {
                Color blockColor = WorldData[x, y].BlockData.ColorOnMap;
                Color gridColor = new Color(blockColor.r - 0.2f, blockColor.g - 0.2f, blockColor.b - 0.2f, blockColor.a);
                Color mapColor = x % 100 == 0 || y % 100 == 0 ? gridColor : blockColor;
                worldMap.SetPixel(x, y, mapColor);

                Color biomeColor = GetChunk(x, y).Biome.ColorOnMap;
                Color gridBiomeColor = new Color(biomeColor.r - 0.2f, biomeColor.g - 0.2f, biomeColor.b - 0.2f, biomeColor.a);
                Color biomeMapColor = x % 100 == 0 || y % 100 == 0 ? gridBiomeColor : biomeColor;
                biomesMap.SetPixel(x, y, biomeMapColor);
            }
        }
        worldMap.Apply();
        biomesMap.Apply();

        byte[] bytesMap = worldMap.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/WorldMap.png", bytesMap);

        byte[] bytesBiome = biomesMap.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/BiomesMap.png", bytesBiome);
    }

    public IEnumerator PrintDebugInfo()
    {
        while (true)
        {
            _infoText.text = GeneralInfo;
            yield return null;
        }
    }

    public IEnumerator UpdateObjects()
    {
        while (true)
        {
            _mainMenu.gameObject.SetActive(IsMenuActive);
            _loadingProgress.gameObject.SetActive(IsLoadingProgressActive);
            _loadingSlider.value = LoadingValue;
            yield return null;
        }
    }

    public void Move()
    {
        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x + Input.GetAxis("Horizontal"), Camera.main.transform.position.y + Input.GetAxis("Vertical"), -10);
    }
    #endregion

    #endregion

    #endregion
}
