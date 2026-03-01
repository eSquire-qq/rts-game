using UnityEngine;

/// <summary>
/// Відповідає лише за візуальне “підсвічування” юніта.
/// Логіка вибору НЕ тут.
/// </summary>
public sealed class SelectionVisual : MonoBehaviour
{
    [SerializeField] private GameObject indicator; // коло/обводка як окремий GameObject

    private void Awake()
    {
        SetSelected(false);
    }

    public void SetSelected(bool value)
    {
        if (indicator != null)
            indicator.SetActive(value);
    }
}