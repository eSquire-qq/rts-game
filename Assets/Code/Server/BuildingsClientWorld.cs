using System.Collections.Generic;
using UnityEngine;

public class BuildingsClientWorld : MonoBehaviour
{
    public BuildingView buildingPrefab;
    public Transform buildingsParent;

    private readonly Dictionary<int, BuildingView> _byId = new();

    public void ApplyState(StateMsg state)
    {
        if (state == null || state.buildings == null) return;

        var aliveIds = new HashSet<int>();

        foreach (var b in state.buildings)
        {
            if (b == null) continue;

            aliveIds.Add(b.id);

            if (!_byId.TryGetValue(b.id, out var view) || view == null)
            {
                view = Spawn(b);
                if (view == null) continue;
                _byId[b.id] = view;
            }

            view.ApplyServerState(b.x, b.y, b.hp, b.maxHp, b.type, b.owner);
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
            _byId.Remove(toRemove[i]);
    }

    private BuildingView Spawn(BuildingDto dto)
    {
        if (buildingPrefab == null)
        {
            Debug.LogError("BuildingsClientWorld: buildingPrefab is not set!");
            return null;
        }

        var pos = new Vector3(dto.x, dto.y, 0f);
        var view = Instantiate(buildingPrefab, pos, Quaternion.identity, buildingsParent);

        view.Bind(dto.id);
        view.ApplyServerState(dto.x, dto.y, dto.hp, dto.maxHp, dto.type, dto.owner);
        view.name = $"Building_{dto.id}_{dto.type}_owner{dto.owner}";

        return view;
    }
}