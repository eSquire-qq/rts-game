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
    [SerializeField] private LobbyClient lobby;

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
        if (sim == null) sim = FindObjectOfType<GameSimulation>();
        if (lobby == null) lobby = FindObjectOfType<LobbyClient>();
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
            if (dragDist < dragThresholdPixels)
                HandleLeftClick(end);
            else
                SelectBox(dragStartScreen, end);
        }

        if (RightDown())
        {
            IssueMove();
        }

        // ✅ STOP по клавіші S
        if (StopDown())
        {
            IssueStop();
        }
    }

    // ---------------- Left click logic ----------------

    private void HandleLeftClick(Vector2 screenPos)
    {
        if (cam == null) return;

        Vector2 world = cam.ScreenToWorldPoint(screenPos);
        Collider2D hit = Physics2D.OverlapPoint(world, unitMask);
        if (!hit)
        {
            ClearSelection();
            return;
        }

        var eid = hit.GetComponentInParent<EntityId>();
        if (eid == null)
        {
            ClearSelection();
            return;
        }

        var view = hit.GetComponentInParent<UnitView>();
        bool isOwnUnit = view == null || lobby == null || lobby.myPlayerId == 0 || view.owner == lobby.myPlayerId;

        if (isOwnUnit)
        {
            ClearSelection();
            AddToSelection(eid.Id, hit.GetComponentInParent<SelectionVisual>());
            return;
        }

        if (selectedIds.Count > 0)
        {
            IssueAttack(eid.Id);
            return;
        }

        ClearSelection();
    }

    // ---------------- Selection ----------------

    private void SelectBox(Vector2 startScreen, Vector2 endScreen)
    {
        if (cam == null) return;

        Rect rect = RectFromScreenPoints(startScreen, endScreen);
        ClearSelection();

        var allColliders = FindObjectsOfType<Collider2D>();
        foreach (var col in allColliders)
        {
            if (col == null) continue;
            if ((unitMask.value & (1 << col.gameObject.layer)) == 0) continue;

            Vector3 sp = cam.WorldToScreenPoint(col.transform.position);
            if (!rect.Contains(sp)) continue;

            var eid = col.GetComponentInParent<EntityId>();
            if (eid == null) continue;

            var view = col.GetComponentInParent<UnitView>();
            if (view != null && lobby != null && lobby.myPlayerId != 0 && view.owner != lobby.myPlayerId) continue;

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
        foreach (var vis in selectedVisuals)
        {
            if (vis != null) vis.SetSelected(false);
        }

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

        foreach (int unitId in selectedIds)
        {
            if (lobby != null && lobby.inMatch)
                lobby.CmdMove(unitId, world.x, world.y);
            else if (sim != null)
                sim.Commands.Enqueue(new MoveCommand(unitId, world));
        }
    }

    private void IssueAttack(int targetId)
    {
        if (selectedIds.Count == 0 || lobby == null || !lobby.inMatch) return;

        foreach (int attackerId in selectedIds)
        {
            if (attackerId == targetId) continue;
            lobby.CmdAttack(attackerId, targetId);
        }
    }

    private void IssueStop()
    {
        if (selectedIds.Count == 0) return;

        if (lobby != null && lobby.inMatch)
        {
            foreach (int unitId in selectedIds)
            {
                lobby.CmdStop(unitId);
            }
            return;
        }

        // OFFLINE: nothing yet
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

    private bool StopDown()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.S);
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