using System.Collections.Generic;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public class TerrainGeneration
{
    #region Private fields
    private List<IWorldProcessingPhase> _generationPhases;
    #endregion

    #region Public fields
    public static List<Vector2Int> SurfaceCoords;
    #endregion

    #region Properties

    #endregion

    #region Methods
    public TerrainGeneration()
    {
        TerrainConfigurationSO terrainConfiguration = GameManager.Instance.TerrainConfiguration;
        _generationPhases = new List<IWorldProcessingPhase>();
        if (terrainConfiguration.FlatWorldGeneration)
        {
            _generationPhases.Add(new FlatWorldGenerationPhase());
        }
        if (terrainConfiguration.LandscapeGeneration)
        {
            _generationPhases.Add(new LandscapeGenerationPhase());
        }
        if (terrainConfiguration.BiomesGeneration)
        {
            _generationPhases.Add(new BiomesSettingPhase());
        }
        if (terrainConfiguration.ClustersGeneration)
        {
            _generationPhases.Add(new ClustersGenerationPhase());
        }
        if (terrainConfiguration.CavesGeneration)
        {
            _generationPhases.Add(new CavesGenerationPhase());
        }
        if (terrainConfiguration.StarterCavesGeneration)
        {
            _generationPhases.Add(new StarterCavesGenerationPhase());
        }
        if (terrainConfiguration.LakesGeneration)
        {
            _generationPhases.Add(new LakesGenerationPhase());
        }
        if (terrainConfiguration.OasisesGeneration)
        {
            _generationPhases.Add(new OasisesGenerationPhase());
        }
        if (terrainConfiguration.GrassSeeding)
        {
            _generationPhases.Add(new GrassSeedingPhase());
        }
        if (terrainConfiguration.PlantsGeneration)
        {
            _generationPhases.Add(new PlantsGenerationPhase());
        }
        if (terrainConfiguration.TreesGeneration)
        {
            _generationPhases.Add(new TreesGenerationPhase());
        }
        if (terrainConfiguration.PickUpItemsGeneration)
        {
            _generationPhases.Add(new PickUpItemsGenerationPhase());
        }
        if (terrainConfiguration.SetRandomTiles)
        {
            _generationPhases.Add(new SetRandomTilesPhase());
        }
        if (terrainConfiguration.BlockProcessing)
        {
            _generationPhases.Add(new UpdateDataPhase());
        }
        if (terrainConfiguration.SetPhysicsShapes)
        {
            _generationPhases.Add(new SetPhysicsShapesPhase());
        }
        if (terrainConfiguration.SaveLoadTest)
        {
            _generationPhases.Clear();
            _generationPhases.Add(new SaveLoadTestPhase());
        }
        SurfaceCoords = new List<Vector2Int>();
    }

    public void StartTerrainGeneration()
    {
        double totalTime = 0f;
        float step = 100f / _generationPhases.Count;

        Stopwatch watch = Stopwatch.StartNew();
        foreach (IWorldProcessingPhase generationPhase in _generationPhases)
        {
            watch.Restart();
            generationPhase.StartPhase();
            watch.Stop();
            Debug.Log($"{generationPhase.Name}: {watch.Elapsed.TotalSeconds}");
            GameManager.Instance.PhasesInfo += $"{generationPhase.Name}: {watch.Elapsed.TotalSeconds}\n";
            totalTime += watch.Elapsed.TotalSeconds;
            GameManager.Instance.LoadingValue += step;
        }

        Debug.Log($"Total time: {totalTime}");
        GameManager.Instance.PhasesInfo += $"Total time: {totalTime}\n";
    }
    #endregion
}