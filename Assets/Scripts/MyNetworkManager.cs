using Cainos.PixelArtTopDown_Basic;
using Mirror;
using UnityEngine;

public class MyNetworkManager : NetworkManager
{
    [Header("Custom Settings")]
    [SerializeField] bool useDebugLog = true;

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);

        DebugLog($"Client connected to server [{conn.address}] with connection ID: {conn.connectionId}");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        MyNetworkPlayer networkPlayer = conn.identity.GetComponent<MyNetworkPlayer>();
        networkPlayer.SetPlayerName($"Player {conn.connectionId}");
        Color randomColor = new Color(Random.Range(0.4f, 1f),
                              Random.Range(0.4f, 1f),
                              Random.Range(0.4f, 1f));

        Color pastelColor = Color.Lerp(randomColor, Color.white, 0.6f);
        networkPlayer.SetPlayerColor(pastelColor);
        networkPlayer.InvokeIsMineEvents();

        DebugLog($"Player added for connection ID: {conn.connectionId}\n" +
            $"Total Players: {numPlayers}\n" +
            $"Current Player Server: {conn.identity.isServer}");
    }

    private void DebugLog(string message)
    {
        if (useDebugLog)
        {
            Debug.Log(message);
        }
    }

}
