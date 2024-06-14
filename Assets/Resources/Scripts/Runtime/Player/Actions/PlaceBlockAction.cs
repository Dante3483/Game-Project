using System.Collections;
using UnityEngine;

public class PlaceBlockAction : PlayerActionBase
{
    #region Fields
    private BlockSO _block;
    private Vector2Int _position;
    private float _placementSpeed;
    private bool _isPlacementAllowed;
    private InventoryModel _inventory;
    #endregion

    #region Properties

    #endregion

    #region Events / Delegates

    #endregion

    #region Public Methods
    public PlaceBlockAction()
    {
        _inventory = _gameManager.GetPlayerInventory();
        _isPlacementAllowed = true;
    }

    public override void Execute()
    {
        if (CanPlaceBlock())
        {
            PlaceBlock();
        }
    }

    public void Configure(BlockSO block, Vector2Int position, float placementSpeed)
    {
        _block = block;
        _position = position;
        _placementSpeed = placementSpeed;
    }
    #endregion

    #region Private Methods
    private void PlaceBlock()
    {
        int x = _position.x;
        int y = _position.y;
        _worldDataManager.SetBlockData(x, y, _block);
        _inventory.RemoveQuantityFromSelectedItem(1);
        _player.StartCoroutine(BlockPlacementCooldownCoroutine());
        _block = null;
    }

    private bool CanPlaceBlock()
    {
        int x = _position.x;
        int y = _position.y;
        if (!_isPlacementAllowed)
        {
            return false;
        }
        if (_block is null)
        {
            return false;
        }
        if (!_worldDataManager.IsFree(x, y))
        {
            return false;
        }
        if (!_worldDataManager.IsEmpty(x, y))
        {
            return false;
        }
        if (!_worldDataManager.IsWall(x, y) && !_worldDataManager.IsSolidAnyNeighbor(x, y))
        {
            return false;
        }
        return true;
    }

    private IEnumerator BlockPlacementCooldownCoroutine()
    {
        _isPlacementAllowed = false;
        yield return new WaitForSeconds(_placementSpeed);
        _isPlacementAllowed = true;
    }
    #endregion
}