using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "newTerrainConfiguration", menuName = "Terrain/Terrain Configuration")]
public class TerrainConfigurationSO : ScriptableObject
{
    #region Private fields

    #endregion

    #region Public fields
    [Header("World size")]
    public byte DefaultHorizontalChunksCount;
    public byte DefaultVerticalChunksCount;
    public byte CurrentHorizontalChunksCount;
    public byte CurrentVerticalChunksCount;
    public byte ChunkSize;

    [Header("World data")]
    public ushort Equator;
    public List<TerrainLevelSO> Levels;
    #endregion

    #region Properties

    #endregion

    #region Methods
    private void OnValidate()
    {
        #region Calculate levels
        ushort y = 0;
        Equator = 0;
        foreach (TerrainLevelSO level in Levels.AsEnumerable().Reverse())
        {
            //Calculate start and end Y coords
            ushort levelHeight = (ushort)(level.CountOfVerticalChunks * ChunkSize);
            level.StartY = y;
            level.EndY = (ushort)(y + levelHeight);
            y += levelHeight;

            //Skip if level is Air
            if (level.Name == "Air")
            {
                continue;
            }

            //Calculate equator
            Equator += levelHeight;
        }
        Equator -= (ushort)(ChunkSize / 2);
        #endregion


    }
    #endregion
}
