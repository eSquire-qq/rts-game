using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Image fill;

    private float maxHealth = 100f;

    public void SetMaxHealth(float health)
    {
        maxHealth = health;
        slider.maxValue = health;
        slider.value = health;
        fill.color = gradient.Evaluate(1f);
        slider.gameObject.SetActive(false);
    }

    public void SetHealth(float health)
    {
        slider.maxValue = maxHealth;
        slider.value = health;

        // ховаємо бар, якщо hp повне
        slider.gameObject.SetActive(health < maxHealth);

        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}