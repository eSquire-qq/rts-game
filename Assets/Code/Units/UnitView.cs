using UnityEngine;

public class UnitView : MonoBehaviour
{
    public int owner;
    public int hp;
    public int maxHp;
    public int Id { get; private set; }
    
    [SerializeField] private EntityId entityId;
    [SerializeField] private HealthBarScript healthBar;

    private void Awake()
    {
        if (entityId == null) entityId = GetComponent<EntityId>();
        if (entityId == null) entityId = GetComponentInParent<EntityId>();

        if (healthBar == null) healthBar = GetComponentInChildren<HealthBarScript>();
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
        transform.position = new Vector3(x, y, 0f);
    }

    public void ApplyHp(int newHp, int newMaxHp)
    {
        hp = newHp;
        maxHp = newMaxHp;

        if (healthBar != null)
        {
            healthBar.SetHealth(hp, maxHp);
        }
    }

    public int GetId()
    {
        if (entityId == null) return -1;
        return entityId.Id;
    }
}