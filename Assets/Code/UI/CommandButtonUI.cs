using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Image icon;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    public void Setup(string text, Sprite sprite, UnityEngine.Events.UnityAction action)
    {
        gameObject.SetActive(true);

        if (label != null)
            label.text = text;

        if (icon != null)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
        button.interactable = true;
    }
    public void Setup(string text, UnityEngine.Events.UnityAction action)
    {
        Setup(text, null, action);
    }

    public void Hide()
    {
        if (button != null)
            button.onClick.RemoveAllListeners();

        gameObject.SetActive(false);
    }
}