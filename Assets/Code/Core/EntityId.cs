using UnityEngine;

public sealed class EntityId : MonoBehaviour
{
    [SerializeField] private int id;
    public int Id => id;

    // На старті можна вручну виставляти в інспекторі.
    // Пізніше: сервер буде роздавати ці ID.
}