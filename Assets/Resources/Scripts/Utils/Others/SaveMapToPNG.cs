using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveMapToPNG : MonoBehaviour
{
    #region Private fields

    #endregion

    #region Public fields

    #endregion

    #region Properties

    #endregion

    #region Methods
    private void Update()
    {
        if (GameManager.Instance.IsGameSession && Input.GetKeyDown(KeyCode.M))
        {
            SaveMap();
        }
    }

    private void SaveMap()
    {
        int terrainWidth = GameManager.Instance.CurrentTerrainWidth;
        int terrainHeight = GameManager.Instance.CurrentTerrainHeight;
        Texture2D worldMap = new Texture2D(terrainWidth, terrainHeight);
        Texture2D biomesMap = new Texture2D(terrainWidth, terrainHeight);
        Color cellColor;
        Color biomeColor;
        Color gridColor;
        Color colorOnMap;

        for (int x = 0; x < terrainWidth; x++)
        {
            for (int y = 0; y < terrainHeight; y++)
            {
                cellColor = WorldDataManager.Instance.WorldData[x, y].BlockData.ColorOnMap;
                if (WorldDataManager.Instance.WorldData[x, y].IsEmpty())
                {
                    cellColor = WorldDataManager.Instance.WorldData[x, y].BackgroundData.ColorOnMap;
                }
                if (WorldDataManager.Instance.WorldData[x, y].IsLiquid())
                {
                    cellColor = GameManager.Instance.BlocksAtlas.GetBlockById(WorldDataManager.Instance.WorldData[x, y].LiquidId).ColorOnMap;
                }

                gridColor = new Color(cellColor.r - 0.2f, cellColor.g - 0.2f, cellColor.b - 0.2f, 1f);
                colorOnMap = x % GameManager.Instance.TerrainConfiguration.ChunkSize == 0 || y % GameManager.Instance.TerrainConfiguration.ChunkSize == 0 ? gridColor : cellColor;
                worldMap.SetPixel(x, y, colorOnMap);

                biomeColor = ChunksManager.Instance.GetChunk(x, y).Biome.ColorOnMap;
                gridColor = new Color(cellColor.r - 0.2f, cellColor.g - 0.2f, cellColor.b - 0.2f, 1f);
                colorOnMap = x % GameManager.Instance.TerrainConfiguration.ChunkSize == 0 || y % GameManager.Instance.TerrainConfiguration.ChunkSize == 0 ? gridColor : biomeColor;
                biomesMap.SetPixel(x, y, colorOnMap);
            }
        }
        worldMap.Apply();
        biomesMap.Apply();

        byte[] bytesMap = worldMap.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/WorldMap.png", bytesMap);

        byte[] bytesBiome = biomesMap.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/BiomesMap.png", bytesBiome);
    }
    #endregion
}