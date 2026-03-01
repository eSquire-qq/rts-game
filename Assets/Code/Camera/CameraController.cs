using UnityEngine;
using UnityEngine.InputSystem;

// Цей клас керує камерою в стилі RTS (Real-Time Strategy) ігор, як StarCraft.
// Він дозволяє рухати камеру за допомогою стрілок клавіатури та краю екрану (edge-scroll),
// зумити коліщатком миші з фокусом під курсор, та обмежує рух в межах карти (bounds).
// Керування WASD повністю вимкнено.
public class RTSCameraController : MonoBehaviour
{
    [SerializeField] private Camera cam; // Посилання на камеру, яку ми керуємо

    [Header("Pan")]
    [SerializeField] private float panSpeed = 20f; // Базова швидкість руху
    [SerializeField] private float panZoomBase = 10f; // Базовий рівень зуму для масштабування швидкості
    [SerializeField] private float panSmooth = 20f; // Плавність руху
    [SerializeField] private bool edgeScroll = true; // Увімкнути рух біля краю екрану
    [SerializeField] private int edgePx = 30; // Розмір зони краю в пікселях
    [SerializeField] private float edgeScrollPower = 1.2f; // Сила edge-scroll

    [Header("Pan Safety")]
    [SerializeField] private float edgeScrollGrace = 0.25f; // Час після якого edge-scroll вимикається якщо миша не рухається
    [SerializeField] private bool disableEdgeScrollWhenMouseIsZero = true; // Ігнорувати (0,0)

    [Header("Zoom")]
    [SerializeField] private float zoomStepPerTick = 1.5f; // Крок зуму за один тик скролу
    [SerializeField] private float zoomSmooth = 20f; // Плавність зуму
    [SerializeField] private float minZoom = 2f; // Мінімальний зум
    [SerializeField] private float maxZoom = 100f; // Максимальний зум

    [Header("Bounds")]
    [SerializeField] private bool useBounds = true; // Чи використовувати межі
    [SerializeField] private Vector2 boundsMin = new(-50, -50); // Мінімальні координати карти
    [SerializeField] private Vector2 boundsMax = new(50, 50); // Максимальні координати карти

    private Vector3 targetPos; // Цільова позиція
    private float targetZoom; // Цільовий зум

    private Vector2 lastMousePos; // Остання позиція миші
    private float lastMouseMoveTime; // Час останнього руху миші

    private void Awake()
    {
        if (!cam) cam = Camera.main;

        var p = transform.position;
        transform.position = new Vector3(p.x, p.y, 0f); // Камера-риг завжди на Z=0

        targetPos = transform.position;
        targetZoom = cam.orthographicSize;

        if (Mouse.current != null)
        {
            lastMousePos = Mouse.current.position.ReadValue();
            lastMouseMoveTime = Time.unscaledTime;
        }
    }

    private void LateUpdate()
    {
        if (!Application.isFocused) return;

        float dt = Time.unscaledDeltaTime;

        UpdateMouseMovementStamp();

        // Читаємо рух ТІЛЬКИ зі стрілок
        Vector2 move = ReadMove();

        // Додаємо рух від краю екрану
        if (edgeScroll && CanUseEdgeScroll())
            move += ReadEdgeScroll();

        // Нормалізація щоб не було прискорення по діагоналі
        if (move.sqrMagnitude > 1f) move.Normalize();

        // Масштабування швидкості залежно від зуму
        float zoomFactor = targetZoom / panZoomBase;

        // Додаємо зміщення
        targetPos += (Vector3)(move * panSpeed * zoomFactor * dt);

        HandleZoom();

        if (useBounds)
            targetPos = ClampToBounds(targetPos, targetZoom);

        // Плавний рух
        transform.position = Smooth(transform.position, targetPos, panSmooth, dt);

        // Плавний зум
        cam.orthographicSize = Smooth(cam.orthographicSize, targetZoom, zoomSmooth, dt);
    }

    // Фіксуємо коли миша рухалась востаннє
    private void UpdateMouseMovementStamp()
    {
        var m = Mouse.current;
        if (m == null) return;

        Vector2 p = m.position.ReadValue();

        if ((p - lastMousePos).sqrMagnitude > 0.01f)
        {
            lastMousePos = p;
            lastMouseMoveTime = Time.unscaledTime;
        }
    }

    // Перевірка чи можна використовувати edge-scroll
    private bool CanUseEdgeScroll()
    {
        var m = Mouse.current;
        if (m == null) return false;

        Vector2 p = m.position.ReadValue();

        if (disableEdgeScrollWhenMouseIsZero && p == Vector2.zero)
            return false;

        if (Time.unscaledTime - lastMouseMoveTime > edgeScrollGrace)
            return false;

        if (p.x < 0 || p.x > Screen.width || p.y < 0 || p.y > Screen.height)
            return false;

        return true;
    }

    // ❗ ВАЖЛИВО: читаємо тільки стрілки (WASD повністю прибрано)
    private Vector2 ReadMove()
    {
        var k = Keyboard.current;
        if (k == null) return Vector2.zero;

        float x = (k.rightArrowKey.isPressed ? 1f : 0f) -
                  (k.leftArrowKey.isPressed ? 1f : 0f);

        float y = (k.upArrowKey.isPressed ? 1f : 0f) -
                  (k.downArrowKey.isPressed ? 1f : 0f);

        return new Vector2(x, y);
    }

    // Обчислення руху при торканні країв екрану
    private Vector2 ReadEdgeScroll()
    {
        Vector2 p = Mouse.current.position.ReadValue();
        float x = 0f, y = 0f;

        if (p.x <= edgePx)
            x = -Mathf.Pow(1f - p.x / edgePx, edgeScrollPower);

        if (p.x >= Screen.width - edgePx)
            x = Mathf.Pow(1f - (Screen.width - p.x) / edgePx, edgeScrollPower);

        if (p.y <= edgePx)
            y = -Mathf.Pow(1f - p.y / edgePx, edgeScrollPower);

        if (p.y >= Screen.height - edgePx)
            y = Mathf.Pow(1f - (Screen.height - p.y) / edgePx, edgeScrollPower);

        return new Vector2(x, y);
    }

    // Обробка зуму з фокусом під курсор
    private void HandleZoom()
    {
        var m = Mouse.current;
        if (m == null) return;

        float raw = m.scroll.ReadValue().y;
        if (Mathf.Abs(raw) < 0.01f) return;

        float oldTarget = targetZoom;
        float ticks = raw / 120f;

        targetZoom = Mathf.Clamp(targetZoom - ticks * zoomStepPerTick, minZoom, maxZoom);

        if (Mathf.Abs(targetZoom - oldTarget) > 0.01f)
        {
            Vector2 mousePos = m.position.ReadValue();
            Vector2 normMouse = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);
            Vector2 sizeDelta = new Vector2((oldTarget - targetZoom) * cam.aspect, oldTarget - targetZoom);

            targetPos.x -= sizeDelta.x * (normMouse.x - 0.5f) * 2f;
            targetPos.y -= sizeDelta.y * (normMouse.y - 0.5f) * 2f;
        }
    }

    // Обмеження руху в межах карти
    private Vector3 ClampToBounds(Vector3 pos, float ortho)
    {
        float halfH = ortho;
        float halfW = ortho * cam.aspect;

        float minX = boundsMin.x + halfW;
        float maxX = boundsMax.x - halfW;
        float minY = boundsMin.y + halfH;
        float maxY = boundsMax.y - halfH;

        pos.x = (minX > maxX) ? (boundsMin.x + boundsMax.x) * 0.5f : Mathf.Clamp(pos.x, minX, maxX);
        pos.y = (minY > maxY) ? (boundsMin.y + boundsMax.y) * 0.5f : Mathf.Clamp(pos.y, minY, maxY);
        pos.z = 0f;

        return pos;
    }

    // Плавне наближення позиції
    private static Vector3 Smooth(Vector3 from, Vector3 to, float sharpness, float dt) =>
        Vector3.Lerp(from, to, 1f - Mathf.Exp(-sharpness * dt));

    // Плавне наближення зуму
    private static float Smooth(float from, float to, float sharpness, float dt) =>
        Mathf.Lerp(from, to, 1f - Mathf.Exp(-sharpness * dt));

    public void SetBounds(Vector2 min, Vector2 max)
    {
        boundsMin = min;
        boundsMax = max;
        targetPos = ClampToBounds(targetPos, targetZoom);
    }

    public void CenterOn(Vector3 worldPos)
    {
        targetPos = new Vector3(worldPos.x, worldPos.y, 0f);
    }
}
