using UnityEngine;

public enum CommandMode
{
    None,
    Move,
    Attack,
    Gather
}

public class CommandPanelUI : MonoBehaviour
{
    public static CommandPanelUI Instance;

    [Header("Refs")]
    [SerializeField] private LobbyClient lobby;
    [SerializeField] private CommandButtonUI[] buttons;

    [Header("Icons")]
    [SerializeField] private Sprite moveIcon;
    [SerializeField] private Sprite stopIcon;
    [SerializeField] private Sprite attackIcon;
    [SerializeField] private Sprite gatherIcon;
    [SerializeField] private Sprite buildIcon;
    [SerializeField] private Sprite swordIcon;
    [SerializeField] private Sprite archerIcon;

    private int selectedUnitId = -1;
    private string selectedUnitType = "";

    private int selectedBuildingId = -1;
    private string selectedBuildingType = "";

    private CommandMode mode = CommandMode.None;

    private void Awake()
    {
        Instance = this;

        if (lobby == null)
            lobby = FindFirstObjectByType<LobbyClient>();

        HideAll();
    }

    public bool HasCommandMode()
    {
        return mode != CommandMode.None && selectedUnitId > 0;
    }

    public void ShowForUnit(UnitDto unit)
    {
        selectedUnitId = unit.id;
        selectedUnitType = unit.unitType;

        selectedBuildingId = -1;
        selectedBuildingType = "";
        mode = CommandMode.None;

        HideAll();

        int i = 0;

        Add(ref i, "Move", moveIcon, () =>
        {
            mode = CommandMode.Move;
        });

        Add(ref i, "Stop", stopIcon, () =>
        {
            mode = CommandMode.None;
            if (lobby != null)
                lobby.CmdStop(selectedUnitId);
        });

        Add(ref i, "Attack", attackIcon, () =>
        {
            mode = CommandMode.Attack;
        });

        if (selectedUnitType == "worker")
        {
            Add(ref i, "Gather", gatherIcon, () =>
            {
                mode = CommandMode.Gather;
            });

            Add(ref i, "House", buildIcon, () =>
            {
                mode = CommandMode.None;
                BuildPlacementManager.Instance?.StartPlacement("house");
            });

            Add(ref i, "Barracks", buildIcon, () =>
            {
                mode = CommandMode.None;
                BuildPlacementManager.Instance?.StartPlacement("barracks");
            });

            Add(ref i, "Archery", buildIcon, () =>
            {
                mode = CommandMode.None;
                BuildPlacementManager.Instance?.StartPlacement("archery");
            });
        }
    }

    public void ShowForBuilding(BuildingDto building)
    {
        selectedBuildingId = building.id;
        selectedBuildingType = building.type;

        selectedUnitId = -1;
        selectedUnitType = "";
        mode = CommandMode.None;

        HideAll();

        int i = 0;

        if (selectedBuildingType == "barracks")
        {
            Add(ref i, "Sword", swordIcon, () =>
            {
                if (lobby != null)
                    lobby.CmdTrainUnit(selectedBuildingId, "swordsman");
            });
        }
        else if (selectedBuildingType == "archery")
        {
            Add(ref i, "Archer", archerIcon, () =>
            {
                if (lobby != null)
                    lobby.CmdTrainUnit(selectedBuildingId, "archer");
            });
        }
    }

    public bool ExecuteCommand(Vector2 worldPos, Collider2D[] hits)
    {
        if (lobby == null || !lobby.inMatch) return false;
        if (selectedUnitId <= 0) return false;

        if (mode == CommandMode.Move)
        {
            lobby.CmdMove(selectedUnitId, worldPos.x, worldPos.y);
            mode = CommandMode.None;
            return true;
        }

        if (mode == CommandMode.Attack)
        {
            foreach (var hit in hits)
            {
                var targetUnit = hit.GetComponentInParent<UnitView>();
                if (targetUnit != null && targetUnit.GetId() != selectedUnitId)
                {
                    lobby.CmdAttack(selectedUnitId, targetUnit.GetId());
                    mode = CommandMode.None;
                    return true;
                }

                var targetBuilding = hit.GetComponentInParent<BuildingClickHandler>();
                if (targetBuilding != null)
                {
                    lobby.CmdAttackBuilding(selectedUnitId, targetBuilding.buildingId);
                    mode = CommandMode.None;
                    return true;
                }
            }

            return false;
        }

        if (mode == CommandMode.Gather)
        {
            foreach (var hit in hits)
            {
                var resource = hit.GetComponentInParent<ResourceView>();
                if (resource != null)
                {
                    lobby.CmdGather(selectedUnitId, resource.resourceId);
                    mode = CommandMode.None;
                    return true;
                }
            }

            return false;
        }

        return false;
    }

    public void HideAll()
    {
        mode = CommandMode.None;

        if (buttons == null) return;

        foreach (var btn in buttons)
        {
            if (btn != null)
                btn.Hide();
        }
    }

    private void Add(ref int index, string text, Sprite icon, UnityEngine.Events.UnityAction action)
    {
        if (buttons == null || index >= buttons.Length) return;
        if (buttons[index] == null) return;

        buttons[index].Setup(text, icon, action);
        index++;
    }
}