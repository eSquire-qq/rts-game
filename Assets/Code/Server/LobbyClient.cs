using System;
using UnityEngine;

[Serializable]
public class LobbyCreatedMsg { public string type; public string lobbyId; }

[Serializable]
public class MatchStartMsg { public string type; public string matchId; }

[Serializable]
public class HelloMsg { public string type; public int playerId; }

public class LobbyClient : MonoBehaviour
{
    [Header("Network")]
    public NetClient net;

    [Header("Test inputs")]
    public string lobbyIdToJoin = "";
    public int moveUnitId = 1;
    public float moveX = 10;
    public float moveY = 3;

    [Header("State (read-only)")]
    public int myPlayerId;
    public string lobbyId;
    public string matchId;
    public bool inMatch;

    private void Awake()
    {
        MainThreadDispatcher.EnsureExists();

        if (net == null)
            net = GetComponent<NetClient>();

        if (net == null)
            Debug.LogError("LobbyClient: NetClient not assigned and not found on same GameObject!");
    }

    private void Start()
    {
        Connect();
    }

    private void OnEnable()
    {
        if (net == null) return;

        net.OnLine += HandleLine;
        net.OnConnected += OnConnected;
        net.OnDisconnected += OnDisconnected;
    }

    private void OnDisable()
    {
        if (net == null) return;

        net.OnLine -= HandleLine;
        net.OnConnected -= OnConnected;
        net.OnDisconnected -= OnDisconnected;
    }

    private void OnConnected()
    {
        Debug.Log("LobbyClient: connected");
    }

    private void OnDisconnected(string reason)
    {
        Debug.Log("LobbyClient: disconnected " + reason);
        inMatch = false;
        matchId = null;
        lobbyId = null;
    }

    // === UI buttons can call these ===
    public void Connect()
    {
        if (net == null) return;
        net.Connect();
    }

    public void CreateLobby()
    {
        SendLine("{\"type\":\"create_lobby\"}");
    }

    public void JoinLobby(string id)
    {
        SendLine("{\"type\":\"join_lobby\",\"lobbyId\":\"" + id + "\"}");
    }

    public void SetReady(bool ready)
    {
        SendLine("{\"type\":\"set_ready\",\"ready\":" + (ready ? "true" : "false") + "}");
    }

    public void CmdMove(int unitId, float x, float y)
    {
        SendLine("{\"type\":\"cmd_move\",\"unitId\":" + unitId + ",\"x\":" + x + ",\"y\":" + y + "}");
    }

    public void CmdEndMatch()
    {
        SendLine("{\"type\":\"cmd_end_match\"}");
    }

    // Small wrapper so Update() stays clean
    private void SendLine(string json)
    {
        if (net == null) return;
        net.SendLine(json);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            CreateLobby();

        if (Input.GetKeyDown(KeyCode.J))
            JoinLobby(lobbyIdToJoin);

        if (Input.GetKeyDown(KeyCode.R))
            SetReady(true);

        if (Input.GetKeyDown(KeyCode.M))
            CmdMove(moveUnitId, moveX, moveY);

        if (Input.GetKeyDown(KeyCode.E))
            CmdEndMatch();
    }

    private void HandleLine(string json)
    {
        // hello
        if (json.Contains("\"type\":\"hello\""))
        {
            var msg = JsonUtility.FromJson<HelloMsg>(json);
            myPlayerId = msg.playerId;
            Debug.Log("My playerId = " + myPlayerId);
            return;
        }

        // lobby_created
        if (json.Contains("\"type\":\"lobby_created\""))
        {
            var msg = JsonUtility.FromJson<LobbyCreatedMsg>(json);
            lobbyId = msg.lobbyId;
            Debug.Log("Lobby created id=" + lobbyId);
            return;
        }
        
        if (json.Contains("\"type\":\"lobby_created\""))
        {
            var msg = JsonUtility.FromJson<LobbyCreatedMsg>(json);
            lobbyId = msg.lobbyId;
            lobbyIdToJoin = lobbyId; // <-- додай
            Debug.Log("Lobby created id=" + lobbyId);
            return;
        }

        // lobby_state
        if (json.Contains("\"type\":\"lobby_state\""))
        {
            Debug.Log("Lobby state: " + json);
            return;
        }

        // match_start
        if (json.Contains("\"type\":\"match_start\""))
        {
            var msg = JsonUtility.FromJson<MatchStartMsg>(json);
            matchId = msg.matchId;
            inMatch = true;
            Debug.Log("Match started id=" + matchId);
            return;
        }

        // state (ticks)
        if (json.Contains("\"type\":\"state\""))
        {
            // щоб не спамити консоль на кожен тик, можна закоментити
            // Debug.Log("State: " + json);
            return;
        }

        if (json.Contains("\"type\":\"error\""))
        {
            Debug.LogWarning("Server error: " + json);
            return;
        }

        Debug.Log("Unknown msg: " + json);
    }
}