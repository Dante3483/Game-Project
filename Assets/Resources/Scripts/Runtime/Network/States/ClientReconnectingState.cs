using System.Collections;
using UnityEngine;

public class ClientReconnectingState : ClientConnectingState
{
    #region Private fields
    private Coroutine _reconnectCoroutine;
    private int _numberOfAttempts;
    private const float _timeBeforeFirstAttempt = 1;
    private const float _timeBetweenAttempts = 5;
    #endregion

    #region Public fields

    #endregion

    #region Properties

    #endregion

    #region Methods
    public ClientReconnectingState(ConnectionManager connectionManager) : base(connectionManager)
    {

    }

    public override void Enter()
    {
        _numberOfAttempts = 0;
        _reconnectCoroutine = _connectionManager.StartCoroutine(ReconnectCoroutine());
    }

    public override void Exit()
    {
        if (_reconnectCoroutine != null)
        {
            _connectionManager.StopCoroutine(_reconnectCoroutine);
            _reconnectCoroutine = null;
        }
    }

    public override void OnClientConnected(ulong _)
    {
        _connectionManager.ChangeState(_connectionManager.ClientConnectedState);
    }

    public override void OnClientDisconnect(ulong _)
    {
        var disconnectReason = _connectionManager.NetworkManager.DisconnectReason;
        if (_numberOfAttempts < _connectionManager.NumberOfReconnectAttempts)
        {
            if (string.IsNullOrEmpty(disconnectReason))
            {
                _reconnectCoroutine = _connectionManager.StartCoroutine(ReconnectCoroutine());
            }
            else
            {
                var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                switch (connectStatus)
                {
                    case ConnectStatus.UserRequestedDisconnect:
                    case ConnectStatus.HostEndedSession:
                    case ConnectStatus.ServerFull:
                    case ConnectStatus.IncompatibleBuildType:
                        _connectionManager.ChangeState(_connectionManager.OfflineState);
                        break;
                    default:
                        _reconnectCoroutine = _connectionManager.StartCoroutine(ReconnectCoroutine());
                        break;
                }
            }
        }
        else
        {
            _connectionManager.ChangeState(_connectionManager.OfflineState);
        }
    }

    private IEnumerator ReconnectCoroutine()
    {
        if (_numberOfAttempts > 0)
        {
            yield return new WaitForSeconds(_timeBetweenAttempts);
        }

        Debug.Log("Lost connection to host, trying to reconnect...");

        _connectionManager.NetworkManager.Shutdown();

        yield return new WaitWhile(() => _connectionManager.NetworkManager.ShutdownInProgress);
        Debug.Log($"Reconnecting attempt {_numberOfAttempts + 1}/{_connectionManager.NumberOfReconnectAttempts}...");

        if (_numberOfAttempts == 0)
        {
            yield return new WaitForSeconds(_timeBeforeFirstAttempt);
        }

        _numberOfAttempts++;
        var reconnectingSetupTask = _connectionMethod.SetupClientReconnectionAsync();
        yield return new WaitUntil(() => reconnectingSetupTask.IsCompleted);

        if (!reconnectingSetupTask.IsFaulted && reconnectingSetupTask.Result.success)
        {
            var connectingTask = ConnectClientAsync();
            yield return new WaitUntil(() => connectingTask.IsCompleted);
        }
        else
        {
            if (!reconnectingSetupTask.Result.shouldTryAgain)
            {
                _numberOfAttempts = _connectionManager.NumberOfReconnectAttempts;
            }
            OnClientDisconnect(0);
        }
    }
    #endregion
}