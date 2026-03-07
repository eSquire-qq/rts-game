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
    [SerializeField] private LobbyClient lobby; // optional

    [Header("Layer masks")]
    [SerializeField] private LayerMask unitMask;
    [SerializeField] private LayerMask groundMask;

    [Header("Selection")]
    [SerializeField] private float dragThresholdPixels = 10f;

    private readonly List<int> selectedIds = new();
    private readonly List<SelectionVisual> selectedVisuals = new();

    private bool isDragging;
    private Vector2 dragStartScreen;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (sim == null) sim = FindFirstObjectByType<GameSimulation>();
        if (lobby == null) lobby = FindFirstObjectByType<LobbyClient>();
    }

    private void Update()
    {
        if (LeftDown())
        {
            isDragging = true;
            dragStartScreen = GetMouseScreen();
            boxView?.Begin(dragStartScreen);
        }

        if (isDragging && LeftHeld())
        {
            boxView?.UpdateBox(GetMouseScreen());
        }

        if (isDragging && LeftUp())
        {
            isDragging = false;
            Vector2 end = GetMouseScreen();
            boxView?.End();

            float dragDist = Vector2.Distance(dragStartScreen, end);
            if (dragDist < dragThresholdPixels) SelectSingleAtScreen(end);
            else SelectBox(dragStartScreen, end);
        }

        if (RightDown())
        {
            IssueMove();
        }
    }

    // ---------------- Selection ----------------

    private void SelectSingleAtScreen(Vector2 screenPos)
    {
        if (cam == null) return;

        Vector2 world = cam.ScreenToWorldPoint(screenPos);
        Collider2D hit = Physics2D.OverlapPoint(world, unitMask);

        ClearSelection();
        if (!hit) return;

        // ✅ беремо EntityId з юніта
        var eid = hit.GetComponentInParent<EntityId>();
        if (eid == null) return;

        AddToSelection(eid.Id, hit.GetComponentInParent<SelectionVisual>());
    }

    private void SelectBox(Vector2 startScreen, Vector2 endScreen)
    {
        if (cam == null) return;

        Rect rect = RectFromScreenPoints(startScreen, endScreen);
        ClearSelection();

        var allColliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
        foreach (var col in allColliders)
        {
            if (col == null) continue;

            int layer = col.gameObject.layer;
            if ((unitMask.value & (1 << layer)) == 0) continue;

            Vector3 sp = cam.WorldToScreenPoint(col.transform.position);
            if (!rect.Contains(sp)) continue;

            var eid = col.GetComponentInParent<EntityId>();
            if (eid == null) continue;

            AddToSelection(eid.Id, col.GetComponentInParent<SelectionVisual>());
        }
    }

    private Rect RectFromScreenPoints(Vector2 a, Vector2 b)
    {
        Vector2 min = Vector2.Min(a, b);
        Vector2 max = Vector2.Max(a, b);
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private void ClearSelection()
    {
        for (int i = 0; i < selectedVisuals.Count; i++)
            if (selectedVisuals[i] != null) selectedVisuals[i].SetSelected(false);

        selectedIds.Clear();
        selectedVisuals.Clear();
    }

    private void AddToSelection(int id, SelectionVisual vis)
    {
        if (selectedIds.Contains(id)) return;
        selectedIds.Add(id);

        if (vis != null)
        {
            selectedVisuals.Add(vis);
            vis.SetSelected(true);
        }
    }

    // ---------------- Orders ----------------

    private void IssueMove()
    {
        if (selectedIds.Count == 0) return;

        Vector2 world = GetMouseWorld();
        Collider2D ground = Physics2D.OverlapPoint(world, groundMask);
        
        if (!ground) return;
        
        for (int i = 0; i < selectedIds.Count; i++)
        {
            int unitId = selectedIds[i];

            if (lobby != null && lobby.inMatch)
                lobby.CmdMove(unitId, world.x, world.y);
            else if (sim != null)
                sim.Commands.Enqueue(new MoveCommand(unitId, world));
        }
    }

    // ---------------- Input ----------------

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
        return Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;
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