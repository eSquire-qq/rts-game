using UnityEngine;

public class BuildingClickHandler : MonoBehaviour
{
    public int buildingId;

    public void OnClicked()
    {
        Debug.Log("Clicked building: " + buildingId);

        var world = FindFirstObjectByType<BuildingsClientWorld>();
        if (world == null)
        {
            Debug.LogError("BuildingsClientWorld not found");
            return;
        }

        if (BuildingUI.Instance == null)
        {
            Debug.LogError("BuildingUI.Instance is null");
            return;
        }

        if (world.TryGetBuildingDto(buildingId, out var dto))
        {
            BuildingUI.Instance.Show(dto);
        }
        else
        {
            Debug.LogWarning("BuildingDto not found for id: " + buildingId);
        }
    }
}