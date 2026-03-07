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

    // Для selection/highlight
    public UnitView TryGetView(int id) => _byId.TryGetValue(id, out var v) ? v : null;

    public IEnumerable<UnitView> AllUnits() => _byId.Values;

    public bool TryRaycastUnitUnderMouse(out UnitView unit)
    {
        unit = null;

        var cam = Camera.main;
        if (cam == null) return false;

        // 2D raycast
        Vector3 wp = cam.ScreenToWorldPoint(Input.mousePosition);
        wp.z = 0f; // важливо для 2D, щоб не було дивних попадань

        var hit = Physics2D.Raycast(wp, Vector2.zero);
        if (hit.collider == null) return false;

        unit = hit.collider.GetComponentInParent<UnitView>();
        return unit != null;
    }

    public void ApplyState(StateMsg state)
    {
        if (state == null || state.units == null) return;

        for (int i = 0; i < state.units.Length; i++)
        {
            var u = state.units[i];
            if (u == null) continue;

            if (!_byId.TryGetValue(u.id, out var view) || view == null)
            {
                view = Spawn(u);
                if (view == null) continue;
                _byId[u.id] = view;
            }

            view.owner = u.owner;
            view.ApplyServerPos(u.x, u.y);
        }

        // (пізніше) despawn відсутніх у state
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

        // важливо: щоб selection працював стабільно — Bind id один раз
        view.Bind(dto.id);

        view.owner = dto.owner;
        view.name = $"Unit_{dto.id}_owner{dto.owner}";
        return view;
    }
}