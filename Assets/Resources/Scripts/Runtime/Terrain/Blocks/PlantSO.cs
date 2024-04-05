using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newPlant", menuName = "Blocks/Plant")]
public class PlantSO : BlockSO
{
    #region Private fields
    [SerializeField] private PlantsID _id;
    [SerializeField] private BiomesID _biomeId;
    [SerializeField] private List<BlockSO> _allowedToSpawnOn;
    [SerializeField] private bool _canGrow = false;
    [SerializeField] private bool _isBottomBlockSolid = true;
    [SerializeField] private bool _isTopBlockSolid = false;
    [SerializeField] private int _chanceToSpawn;
    [SerializeField] private int _chanceToGrow;
    [SerializeField] private bool _isBiomeSpecific;
    #endregion

    #region Public fields

    #endregion

    #region Properties
    public PlantsID Id
    {
        get
        {
            return _id;
        }

        set
        {
            _id = value;
        }
    }

    public List<BlockSO> AllowedToSpawnOn
    {
        get
        {
            return _allowedToSpawnOn;
        }

        set
        {
            _allowedToSpawnOn = value;
        }
    }

    public bool CanGrow
    {
        get
        {
            return _canGrow;
        }

        set
        {
            _canGrow = value;
        }
    }

    public bool IsBottomBlockSolid
    {
        get
        {
            return _isBottomBlockSolid;
        }

        set
        {
            _isBottomBlockSolid = value;
        }
    }

    public bool IsTopBlockSolid
    {
        get
        {
            return _isTopBlockSolid;
        }

        set
        {
            _isTopBlockSolid = value;
        }
    }

    public int ChanceToSpawn
    {
        get
        {
            return _chanceToSpawn;
        }

        set
        {
            _chanceToSpawn = value;
        }
    }

    public int ChanceToGrow
    {
        get
        {
            return _chanceToGrow;
        }

        set
        {
            _chanceToGrow = value;
        }
    }

    public BiomesID BiomeId
    {
        get
        {
            return _biomeId;
        }

        set
        {
            _biomeId = value;
        }
    }

    public bool IsBiomeSpecific
    {
        get
        {
            return _isBiomeSpecific;
        }

        set
        {
            _isBiomeSpecific = value;
        }
    }
    #endregion

    #region Methods
    public PlantSO()
    {
        _type = BlockTypes.Plant;
    }

    public override ushort GetId()
    {
        return (ushort)Id;
    }
    #endregion
}
