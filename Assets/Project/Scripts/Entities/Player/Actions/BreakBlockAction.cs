namespace SavageWorld.Runtime.Entities.Player.Actions
{
    public class BreakBlockAction : BreakAction
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        #region Events / Delegates

        #endregion

        #region Public Methods
        public BreakBlockAction() : base()
        {
            _replacment = _gameManager.TilesAtlas.Air;
            _addDamage += _miningDamageController.AddDamageToBlock;
            _replace += _tilesManager.SetBlockData;
            _miningDamageController.BlockDamageReachedMaxValue += Break;
        }
        #endregion

        #region Private Methods
        protected override bool CanBreak(int x, int y)
        {
            if (!base.CanBreak(x, y))
            {
                return false;
            }
            if (_tilesManager.IsAbstract(x, y))
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}