using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI label;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }

    public void Setup(string text, UnityEngine.Events.UnityAction action)
    {
        gameObject.SetActive(true);

        if (label != null)
            label.text = text;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        button.onClick.RemoveAllListeners();
    }
}