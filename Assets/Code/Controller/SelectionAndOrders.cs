using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed class SelectionAndOrders : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera cam;
    [SerializeField] private GameSimulation sim;
    [SerializeField] private SelectionBoxView boxView;

    [Header("Layer masks")]
    [SerializeField] private LayerMask unitMask;
    [SerializeField] private LayerMask groundMask;

    [Header("Selection")]
    [SerializeField] private float dragThresholdPixels = 10f; // скільки треба “протягнути”, щоб це вважалось drag

    // Поточне виділення (мульти)
    private readonly List<EntityId> selected = new();
    private readonly List<SelectionVisual> selectedVisuals = new();

    private bool isDragging;
    private Vector2 dragStartScreen;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        // --- ЛКМ down ---
        if (LeftDown())
        {
            isDragging = true;
            dragStartScreen = GetMouseScreen();

            boxView?.Begin(dragStartScreen);
        }

        // --- ЛКМ hold ---
        if (isDragging && LeftHeld())
        {
            Vector2 current = GetMouseScreen();

            // якщо ще не пройшли поріг — рамку можемо не показувати (але ми вже її показали; ок)
            boxView?.UpdateBox(current);
        }

        // --- ЛКМ up ---
        if (isDragging && LeftUp())
        {
            isDragging = false;

            Vector2 end = GetMouseScreen();
            boxView?.End();

            float dragDist = Vector2.Distance(dragStartScreen, end);

            if (dragDist < dragThresholdPixels)
            {
                // Клік — вибір одного
                SelectSingleAtScreen(end);
            }
            else
            {
                // Drag — рамка
                SelectBox(dragStartScreen, end);
            }
        }

        // --- ПКМ: наказ руху ---
        if (RightDown())
        {
            IssueMove();
        }
        
        // testing selection box
        if (LeftDown())
            Debug.Log("LeftDown in GAME coords: " + GetMouseScreen());

    }

    // ---------------------- Selection ----------------------

    private void SelectSingleAtScreen(Vector2 screenPos)
    {
        Vector2 world = cam.ScreenToWorldPoint(screenPos);
        Collider2D hit = Physics2D.OverlapPoint(world, unitMask);

        ClearSelection();

        if (!hit) return;

        var id = hit.GetComponent<EntityId>();
        if (id == null) return;

        AddToSelection(id);
        // Debug.Log($"Selected 1: {id.Id}");
    }

    private void SelectBox(Vector2 startScreen, Vector2 endScreen)
    {
        Rect rect = RectFromScreenPoints(startScreen, endScreen);

        ClearSelection();

        // Простіше і стабільніше для старту:
        // Беремо всі юніти на сцені і перевіряємо, чи їх screen point в прямокутнику.
        foreach (var entity in FindObjectsByType<EntityId>(FindObjectsSortMode.None))
        {
            // фільтруємо тільки юніти по layer mask
            int layer = entity.gameObject.layer;
            if ((unitMask.value & (1 << layer)) == 0) continue;

            Vector3 screen = cam.WorldToScreenPoint(entity.transform.position);
            if (rect.Contains(screen))
            {
                AddToSelection(entity);
            }
        }

        // Debug.Log($"Selected box: {selected.Count}");
    }

    private Rect RectFromScreenPoints(Vector2 a, Vector2 b)
    {
        Vector2 min = Vector2.Min(a, b);
        Vector2 max = Vector2.Max(a, b);
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private void ClearSelection()
    {
        // вимкнути підсвітку
        for (int i = 0; i < selectedVisuals.Count; i++)
            if (selectedVisuals[i] != null) selectedVisuals[i].SetSelected(false);

        selected.Clear();
        selectedVisuals.Clear();
    }

    private void AddToSelection(EntityId id)
    {
        selected.Add(id);

        var vis = id.GetComponent<SelectionVisual>();
        if (vis != null)
        {
            selectedVisuals.Add(vis);
            vis.SetSelected(true);
        }
    }

    // ---------------------- Orders ----------------------

    private void IssueMove()
    {
        if (selected.Count == 0) return;

        Vector2 world = GetMouseWorld();

        // Дозволяємо наказ тільки по землі
        Collider2D ground = Physics2D.OverlapPoint(world, groundMask);
        if (!ground) return;

        // Поки що всі в одну точку (наступним кроком зробимо формацію)
        for (int i = 0; i < selected.Count; i++)
        {
            sim.Commands.Enqueue(new MoveCommand(selected[i].Id, world));
        }
    }

    // ---------------------- Input (Old + New) ----------------------

    private bool LeftDown()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    private bool LeftHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.isPressed;
#else
        return Input.GetMouseButton(0);
#endif
    }

    private bool LeftUp()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
#else
        return Input.GetMouseButtonUp(0);
#endif
    }

    private bool RightDown()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(1);
#endif
    }

    private Vector2 GetMouseScreen()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current.position.ReadValue();
#else
        return Input.mousePosition;
#endif
    }

    private Vector2 GetMouseWorld()
    {
        Vector2 screen = GetMouseScreen();
        return cam.ScreenToWorldPoint(screen);
    }
}
