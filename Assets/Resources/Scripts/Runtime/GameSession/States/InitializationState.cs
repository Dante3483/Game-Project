using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public class InitializationState : GameStateBase
{
    #region Private fields
    private Coroutine _waitUntilInitializedCoroutine;
    #endregion

    #region Public fields

    #endregion

    #region Properties

    #endregion

    #region Methods
    public override void Enter()
    {
        Task.Run(InitializeGame);
    }

    public override void Exit()
    {
        UIManager.Instance.MainMenuProgressBarUI.IsActive = false;
    }

    private void InitializeGame()
    {
        try
        {
            var watch = Stopwatch.StartNew();
            List<Action> initializationSteps = new()
            {
                InitializeAtlases,
                InitializePlayers,
                InitializeWorlds,
                InitializeData,
            };

            float loadingStep = 100f / initializationSteps.Count;

            UIManager.Instance.MainMenuUI.IsActive = false;
            UIManager.Instance.MainMenuProgressBarUI.IsActive = true;

            foreach (Action initializationStep in initializationSteps)
            {
                initializationStep?.Invoke();
                _gameManager.LoadingValue += loadingStep;
            }

            watch.Stop();
            Debug.Log($"Game initialization: {watch.Elapsed.TotalSeconds}");
            _gameManager.PhasesInfo += $"Game initialization: {watch.Elapsed.TotalSeconds}\n";

            ActionInMainThreadUtil.Instance.Invoke(() => _gameManager.ChangeState(_gameManager.MainMenuState));
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void InitializeAtlases()
    {
        GameManager.Instance.BlocksAtlas.InitializeAtlas();
        GameManager.Instance.TreesAtlas.InitializeAtlas();
        GameManager.Instance.PickUpItemsAtlas.InitializeAtlas();
        GameManager.Instance.ItemsAtlas.InitializeAtlas();
    }

    private void InitializePlayers()
    {
        GetAllPlayerNames();
    }

    private void InitializeWorlds()
    {
        GetAllWorldNames();
    }

    private void InitializeData()
    {
        WorldDataManager.Instance.Initialize();
        ChunksManager.Instance.Initialize();
        _gameManager.Terrain.Initialize();
    }

    private void GetAllPlayerNames()
    {
        DirectoryInfo directoryInfo = new(StaticInfo.PlayersDirectory);
        FileInfo[] filesInfo = directoryInfo.GetFiles("*.sw.player");
        GameManager.Instance.PlayerNames = new List<string>();
        foreach (FileInfo fileInfo in filesInfo)
        {
            GameManager.Instance.PlayerNames.Add(fileInfo.Name.Replace(".sw.player", ""));
        }
    }

    private void GetAllWorldNames()
    {
        DirectoryInfo directoryInfo = new(StaticInfo.WorldsDirectory);
        DirectoryInfo[] directoriesInfo = directoryInfo.GetDirectories();
        GameManager.Instance.WorldNames = new List<string>();
        foreach (DirectoryInfo directoryIndo in directoriesInfo)
        {
            GameManager.Instance.WorldNames.Add(directoryIndo.Name);
        }
    }
    #endregion
}