using UnityEngine;

public class UnitView : MonoBehaviour
{
    public int owner;

    [SerializeField] private EntityId entityId;

    private void Awake()
    {
        if (entityId == null) entityId = GetComponent<EntityId>();
        if (entityId == null) entityId = GetComponentInParent<EntityId>();
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
}