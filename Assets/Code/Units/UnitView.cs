using UnityEngine;

public class UnitView : MonoBehaviour
{
    public int owner;
    public int hp;

    [SerializeField] private EntityId entityId;
    [SerializeField] private HealthBarScript healthBar;

    private void Awake()
    {
        if (entityId == null) entityId = GetComponent<EntityId>();
        if (entityId == null) entityId = GetComponentInParent<EntityId>();

        if (healthBar == null) healthBar = GetComponentInChildren<HealthBarScript>();

        if (healthBar != null)
            healthBar.SetMaxHealth(100f); // поки хардкод, потім можна слати maxHp із сервера
    }

    public void Bind(int id)
    {
        if (entityId == null)
        {
            Debug.LogError("UnitView: no EntityId on prefab!");
            return;
        }

        entityId.Set(id);
    }

    public void ApplyServerPos(float x, float y)
    {
        transform.position = new Vector3(x, y, transform.position.z);
    }

    public void ApplyHp(int newHp)
    {
        hp = newHp;

        if (healthBar != null)
            healthBar.SetHealth(hp);
    }

    public int GetId()
    {
        if (entityId == null) return -1;
        return entityId.Id;
    }
}