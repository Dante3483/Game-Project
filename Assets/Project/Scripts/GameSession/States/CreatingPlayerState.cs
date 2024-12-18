using SavageWorld.Runtime.Network;

namespace SavageWorld.Runtime.GameSession.States
{
    public class CreatingPlayerState : GameStateBase
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        #region Events / Delegates

        #endregion

        #region Public Methods
        public override void Enter()
        {
            if (NetworkManager.Instance.IsMultiplayer)
            {
                if (GameManager.Instance.IsClient)
                {
                    NetworkManager.Instance.ConnectToServer();
                }
                else
                {
                    NetworkManager.Instance.StartServer();
                }
            }
            else
            {
                _gameManager.PlayerPrefab.CreateInstance(new(3655, 2200));
            }
            _gameManager.ChangeState(_gameManager.PlayingState);
        }

        public override void Exit()
        {

        }
        #endregion

        #region Private Methods

        #endregion
    }
}
