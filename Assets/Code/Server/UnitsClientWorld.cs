using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitDto
{
    public int id;
    public int owner;
    public string unitType;
    public float x;
    public float y;
    public int hp;
    public int maxHp;
}

[Serializable]
public class PlayerDto
{
    public int playerId;
    public int gold;
    public int lumber;
    public int usedSupply;
    public int maxSupply;
}

[System.Serializable]
public class BuildingDto
{
    public int id;
    public int owner;
    public string type;
    public float x;
    public float y;
    public int hp;
    public int maxHp;

    public string currentUnit;
    public float trainTime;
    public int queueSize;
}

[Serializable]
public class StateMsg
{
    public string type;
    public int tick;
    public UnitDto[] units;
    public BuildingDto[] buildings;
    public PlayerDto[] players;
}

public class UnitsClientWorld : MonoBehaviour
{
    [Header("Prefabs")]
    public UnitView unitPrefab;

    [Header("Optional parent for spawned units")]
    public Transform unitsParent;

    private readonly Dictionary<int, UnitView> _byId = new Dictionary<int, UnitView>();

    public bool TryGetUnit(int id, out UnitView view) => _byId.TryGetValue(id, out view);

    public UnitView TryGetView(int id) => _byId.TryGetValue(id, out var v) ? v : null;

    public IEnumerable<UnitView> AllUnits() => _byId.Values;

    public GameObject warriorPrefab;
    public GameObject archerPrefab;
    public GameObject workerPrefab;

    private Dictionary<string, GameObject> prefabMap;

    public void Awake()
    {
        prefabMap = new Dictionary<string, GameObject>()
        {
            { "swordsman", warriorPrefab },
            { "archer", archerPrefab },
            { "worker", workerPrefab }
        };
    }
    
    public bool TryRaycastUnitUnderMouse(out UnitView unit)
    {
        unit = null;

        var cam = Camera.main;
        if (cam == null) return false;

        Vector3 wp = cam.ScreenToWorldPoint(Input.mousePosition);
        wp.z = 0f;

        var hit = Physics2D.Raycast(wp, Vector2.zero);
        if (hit.collider == null) return false;

        unit = hit.collider.GetComponentInParent<UnitView>();
        return unit != null;
    }

    public void ApplyState(StateMsg state)
    {
        if (state == null || state.units == null) return;
        
        var aliveIds = new HashSet<int>();

        for (int i = 0; i < state.units.Length; i++)
        {
            var u = state.units[i];
            if (u == null) continue;

            aliveIds.Add(u.id);

            if (!_byId.TryGetValue(u.id, out var view) || view == null)
            {
                view = Spawn(u);
                if (view == null) continue;
                _byId[u.id] = view;
            }

            view.owner = u.owner;
            view.ApplyServerPos(u.x, u.y);
            view.ApplyHp(u.hp, u.maxHp);
        }
        
        var toRemove = new List<int>();

        foreach (var kv in _byId)
        {
            if (!aliveIds.Contains(kv.Key))
            {
                if (kv.Value != null)
                    Destroy(kv.Value.gameObject);

                toRemove.Add(kv.Key);
            }
        }
        
        for (int i = 0; i < toRemove.Count; i++)
        {
            _byId.Remove(toRemove[i]);
        }
    }

    private UnitView Spawn(UnitDto dto)
    {
        if (!prefabMap.TryGetValue(dto.unitType, out var prefab))
        {
            Debug.LogError($"No prefab for unit type: {dto.unitType}");
            return null;
        }

        var pos = new Vector3(dto.x, dto.y, 0f);
        var go = Instantiate(prefab, pos, Quaternion.identity, unitsParent);

        var view = go.GetComponent<UnitView>();
        if (view == null)
        {
            Debug.LogError($"Prefab {dto.unitType} has no UnitView!");
            return null;
        }

        view.Bind(dto.id);
        view.owner = dto.owner;
        view.ApplyHp(dto.hp, dto.maxHp);
        view.name = $"Unit_{dto.id}_{dto.unitType}_owner{dto.owner}";

        return view;
    }
}