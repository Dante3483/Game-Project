public abstract class OnlineState : ConnectionStateBase
{
    #region Private fields

    #endregion

    #region Public fields

    #endregion

    #region Properties

    #endregion

    #region Methods
    public OnlineState(ConnectionManager connectionManager) : base(connectionManager)
    {

    }

    public override void OnUserRequestedShutdown()
    {
        _connectionManager.ChangeState(_connectionManager.OfflineState);
    }

    public override void OnTransportFailure()
    {
        _connectionManager.ChangeState(_connectionManager.OfflineState);
    }
    #endregion
}