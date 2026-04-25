using System;
using System.Globalization;
using UnityEngine;

[Serializable]
public class LobbyCreatedMsg { public string type; public string lobbyId; }

[Serializable]
public class MatchStartMsg { public string type; public string matchId; }

[Serializable]
public class HelloMsg { public string type; public int playerId; }

public class LobbyClient : MonoBehaviour
{
    [SerializeField] private TopBarUI topBarUI;
    
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

    public UnitsClientWorld world;
    public BuildingsClientWorld buildingsWorld;
    
    public PlayerUI playerUI;
    
    private void Awake()
    {
        MainThreadDispatcher.EnsureExists();
        
        if (net == null) net = GetComponent<NetClient>();
        if (world == null) world = FindFirstObjectByType<UnitsClientWorld>();
        if(buildingsWorld == null) buildingsWorld = FindObjectOfType<BuildingsClientWorld>();
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
        string xs = x.ToString(CultureInfo.InvariantCulture);
        string ys = y.ToString(CultureInfo.InvariantCulture);

        SendLine("{\"type\":\"cmd_move\",\"unitId\":" + unitId +
                 ",\"x\":" + xs +
                 ",\"y\":" + ys + "}");
    }

    public void CmdTrainUnit(int buildingId, string unitType)
    {
        SendLine("{\"type\":\"cmd_train_unit\",\"buildingId\":" + buildingId + ",\"unitType\":\"" + unitType + "\"}");
    }
    
    public void CmdAttack(int unitId, int targetId)
    {
        SendLine("{\"type\":\"cmd_attack\",\"unitId\":" + unitId + ",\"targetId\":" + targetId + "}");
    }

    public void CmdAttackBuilding(int unitId, int targetId)
    {
        SendLine("{\"type\":\"cmd_attack_building\",\"unitId\":" + unitId + ",\"targetId\":" + targetId + "}");
    }

    public void CmdStop(int unitId)
    {
        SendLine("{\"type\":\"cmd_stop\",\"unitId\":" + unitId + "}");
    }
    public void CmdEndMatch()
    {
        SendLine("{\"type\":\"cmd_end_match\"}");
    }

    private void SendLine(string json)
    {
        if (net == null) return;
        net.SendLine(json);
    }
    
    public void SendBuildCommand(string type, float x, float y)
    {
        string xs = x.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string ys = y.ToString(System.Globalization.CultureInfo.InvariantCulture);

        SendLine("{\"type\":\"cmd_build\",\"buildingType\":\"" + type +
                 "\",\"x\":" + xs + ",\"y\":" + ys + "}");
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            CmdTrainUnit(1, "swordsman");

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
        if (json.Contains("\"type\":\"state\""))
        {
            var msg = JsonUtility.FromJson<StateMsg>(json);

            if (msg != null)
            {
                if (myPlayerId == 0)
                {
                    Debug.Log("WAITING FOR PLAYER ID...");
                    return;
                }

                world?.ApplyState(msg);
                buildingsWorld?.ApplyState(msg);
                playerUI?.UpdateFromState(msg);
            }
            return;
        }
        
        if (json.Contains("\"type\":\"hello\""))
        {
            var msg = JsonUtility.FromJson<HelloMsg>(json);
            myPlayerId = msg.playerId;
            playerUI.myPlayerId = myPlayerId;
            Debug.Log("My playerId = " + myPlayerId);
            return;
        }

        if (json.Contains("\"type\":\"lobby_created\""))
        {
            var msg = JsonUtility.FromJson<LobbyCreatedMsg>(json);
            lobbyId = msg.lobbyId;
            lobbyIdToJoin = lobbyId;
            Debug.Log("Lobby created id=" + lobbyId);
            return;
        }

        if (json.Contains("\"type\":\"lobby_state\""))
        {
            Debug.Log("Lobby state: " + json);
            return;
        }

        if (json.Contains("\"type\":\"match_start\""))
        {
            var msg = JsonUtility.FromJson<MatchStartMsg>(json);
            matchId = msg.matchId;
            inMatch = true; // ← це має спрацювати
        }

        if (json.Contains("\"type\":\"error\""))
        {
            Debug.LogWarning("Server error: " + json);
            return;
        }
        
        Debug.Log("Unknown msg: " + json);
        topBarUI?.SetPlayerId(myPlayerId);
    }
    public void SendBuild(string buildingType)
    {
        SendLine("{\"type\":\"cmd_build\",\"buildingType\":\"" + buildingType + "\",\"x\":0,\"y\":0}");
    }
}