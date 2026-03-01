using UnityEngine;

/// <summary>
/// Мотор руху юніта.
/// ВАЖЛИВО: тут немає інпуту, немає “кліків миші”.
/// Юніт просто отримує ціль (SetMoveTarget) і рухається.
/// Це робить систему готовою до сервера: сервер/симуляція дає команду, мотор виконує.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public sealed class UnitMotor2D : MonoBehaviour
{
    // Швидкість руху юніта (можеш міняти в інспекторі).
    [SerializeField] private float moveSpeed = 3.5f;

    private Rigidbody2D rb;

    // Точка, куди йти.
    private Vector2 target;

    // Чи є активна ціль руху.
    private bool hasTarget;

    public bool HasTarget => hasTarget;
    public Vector2 Target => target;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Для RTS в 2D гравітація зазвичай не потрібна.
        rb.gravityScale = 0;

        // Рекомендація: Body Type = Kinematic (в інспекторі), щоб ми контролювали рух самі.
    }

    /// <summary>
    /// Встановлюємо нову точку призначення.
    /// Викликається симуляцією (або сервером у майбутньому).
    /// </summary>
    public void SetMoveTarget(Vector2 worldPoint)
    {
        target = worldPoint;
        hasTarget = true;
    }

    /// <summary>
    /// Зупинка руху (скидаємо ціль + швидкість).
    /// </summary>
    public void Stop()
    {
        hasTarget = false;
        rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// FixedUpdate викликається з фіксованим кроком фізики.
    /// Рух через Rigidbody2D краще робити тут.
    /// </summary>
    private void FixedUpdate()
    {
        if (!hasTarget) return;

        Vector2 pos = rb.position;
        Vector2 to = target - pos;

        // Якщо ми вже майже в точці, “доклеюємося” і зупиняємося.
        if (to.sqrMagnitude < 0.02f)
        {
            rb.MovePosition(target);
            Stop();
            return;
        }

        // Рухаємося прямою лінією до цілі (без pathfinding).
        Vector2 step = to.normalized * moveSpeed * Time.fixedDeltaTime;

        // MovePosition — правильний спосіб для керованого руху Rigidbody2D.
        rb.MovePosition(pos + step);
    }
}
