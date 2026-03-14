using UnityEngine;

public class BuildingView : MonoBehaviour
{
    public int owner;
    public int hp;
    public int maxHp;
    public string buildingType;

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
            Debug.LogError("BuildingView: no EntityId on prefab!");
            return;
        }

        entityId.Set(id);
    }

    public void ApplyServerState(float x, float y, int hp, int maxHp, string type, int owner)
    {
        transform.position = new Vector3(x, y, transform.position.z);

        this.hp = hp;
        this.maxHp = maxHp;
        this.buildingType = type;
        this.owner = owner;

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHp);
            healthBar.SetHealth(hp);
        }
    }

    public int GetId()
    {
        return entityId != null ? entityId.Id : -1;
    }
}