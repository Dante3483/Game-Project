using System;
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
    public ushort DeepOceanY;
    public List<TerrainLevelSO> Levels;
    public List<BiomeSO> Biomes;
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
            level.EndY = (ushort)(y + levelHeight - 1);
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

        #region Calculate biomes
        ushort x = 0;
        foreach (BiomeSO biome in Biomes)
        {
            if (biome.Id == BiomesID.NonBiom)
            {
                continue;
            }

            //Calculate couont
            biome.RoundCount(CurrentHorizontalChunksCount * biome.Percentage / 100f);

            //Calculate start and end X coords
            ushort biomeLength = (ushort)(biome.ChunksCount * ChunkSize);
            biome.StartX = x;
            biome.EndX = (ushort)(x + biomeLength - 1);
            x += biomeLength;
        }
        #endregion
    }
    #endregion
}
