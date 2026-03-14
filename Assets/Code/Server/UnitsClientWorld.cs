using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitDto
{
    public int id;
    public int owner;
    public float x;
    public float y;
    public int hp;
}

[Serializable]
public class StateMsg
{
    public string type;
    public int tick;
    public UnitDto[] units;
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

        // 1) Збираємо всіх "живих" юнітів, які прийшли від сервера в цьому state
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
            view.ApplyHp(u.hp);
        }

        // 2) Видаляємо локальні юніти, яких більше немає в state
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

        // 3) Прибираємо їх із dictionary
        for (int i = 0; i < toRemove.Count; i++)
        {
            _byId.Remove(toRemove[i]);
        }
    }

    private UnitView Spawn(UnitDto dto)
    {
        if (unitPrefab == null)
        {
            Debug.LogError("UnitsClientWorld: unitPrefab is not set!");
            return null;
        }

        var pos = new Vector3(dto.x, dto.y, 0f);
        var view = Instantiate(unitPrefab, pos, Quaternion.identity, unitsParent);

        view.Bind(dto.id);
        view.owner = dto.owner;
        view.ApplyHp(dto.hp);
        view.name = $"Unit_{dto.id}_owner{dto.owner}";

        return view;
    }
}