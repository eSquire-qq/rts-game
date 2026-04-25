using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : MonoBehaviour
{
    public static BuildingUI Instance;

    [Header("Refs")]
    [SerializeField] private LobbyClient client;
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI queueText;
    [SerializeField] private Slider progressBar;

    [Header("Command buttons")]
    [SerializeField] private CommandButtonUI[] commandButtons;

    private int selectedBuildingId = -1;
    private string selectedBuildingType = "";

    private void Awake()
    {
        Instance = this;

        if (client == null)
            client = FindFirstObjectByType<LobbyClient>();

        if (panel == null)
            panel = gameObject;

        panel.SetActive(false);

        HideAllButtons();
    }

    public void Show(BuildingDto building)
    {
        selectedBuildingId = building.id;
        selectedBuildingType = building.type;

        panel.SetActive(true);

        if (titleText != null)
            titleText.text = $"{building.type} #{building.id}";

        SetupButtons();
        UpdateBuildingInfo(building);
    }

    public void Hide()
    {
        selectedBuildingId = -1;
        selectedBuildingType = "";
        panel.SetActive(false);
        HideAllButtons();
    }

    public bool IsShowingBuilding(int id)
    {
        return panel.activeSelf && selectedBuildingId == id;
    }

    public void UpdateBuildingInfo(BuildingDto building)
    {
        if (!IsShowingBuilding(building.id)) return;

        if (queueText != null)
            queueText.text = $"Queue: {building.queueSize}";

        if (progressBar != null)
        {
            bool hasTraining = !string.IsNullOrEmpty(building.currentUnit) && building.currentUnit != "null";

            progressBar.gameObject.SetActive(hasTraining);

            if (hasTraining)
            {
                float totalTime = 3f;
                float progress = 1f - Mathf.Clamp01(building.trainTime / totalTime);
                progressBar.value = progress;
            }
        }
    }

    private void SetupButtons()
    {
        HideAllButtons();

        int index = 0;

        if (selectedBuildingType == "barracks")
        {
            commandButtons[index++].Setup("Swordsman", () =>
            {
                client.CmdTrainUnit(selectedBuildingId, "swordsman");
            });
        }

        if (selectedBuildingType == "archery")
        {
            commandButtons[index++].Setup("Archer", () =>
            {
                client.CmdTrainUnit(selectedBuildingId, "archer");
            });
        }
    }

    private void HideAllButtons()
    {
        if (commandButtons == null) return;

        foreach (var btn in commandButtons)
        {
            if (btn != null)
                btn.Hide();
        }
    }
}