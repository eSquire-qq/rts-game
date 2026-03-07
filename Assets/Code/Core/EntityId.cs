using UnityEngine;

public sealed class EntityId : MonoBehaviour
{
    // залишай як хочеш в інспекторі
    [SerializeField] private int id;

    // ✅ єдине “правильне” API для коду
    public int Id => id;

    // якщо треба встановлювати зі спавну
    
    public void Set(int newId) => id = newId;
}