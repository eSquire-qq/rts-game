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
    [SerializeField] private LayerMask buildingMask;
    [SerializeField] private LayerMask groundMask;

    [Header("Selection")]
    [SerializeField] private float dragThresholdPixels = 10f;

    private readonly List<int> selectedIds = new();
    private readonly List<SelectionVisual> selectedVisuals = new();

    private bool isDragging;
    private Vector2 dragStartScreen;

    // ── стан миші який оновлюється вручну ──
    private Vector2 _mousePos;
    private bool _leftDown, _leftHeld, _leftUp;
    private bool _rightDown;
    private bool _stopDown;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (sim == null) sim = FindFirstObjectByType<GameSimulation>();
        if (lobby == null) lobby = FindFirstObjectByType<LobbyClient>();
    }

    private void Update()
    {
        // ── читаємо ввід один раз на початку фрейму ──
        PollInput();

        if (_leftDown)
        {
            isDragging = true;
            dragStartScreen = _mousePos;
            boxView?.Begin(dragStartScreen);
        }

        if (isDragging && _leftHeld)
        {
            boxView?.UpdateBox(_mousePos);
        }

        if (isDragging && _leftUp)
        {
            isDragging = false;
            Vector2 end = _mousePos;
            boxView?.End();

            float dragDist = Vector2.Distance(dragStartScreen, end);
            if (dragDist < dragThresholdPixels)
                HandleLeftClick(end);
            else
                SelectBox(dragStartScreen, end);
        }

        if (_rightDown)
        {
            IssueMove();
        }

        if (_stopDown)
        {
            IssueStop();
        }
    }

    // ── універсальне читання вводу ──
    private void PollInput()
    {
#if ENABLE_INPUT_SYSTEM
        // Новий Input System
        var mouse = Mouse.current;
        var keyboard = Keyboard.current;

        if (mouse != null)
        {
            _mousePos  = mouse.position.ReadValue();
            _leftDown  = mouse.leftButton.wasPressedThisFrame;
            _leftHeld  = mouse.leftButton.isPressed;
            _leftUp    = mouse.leftButton.wasReleasedThisFrame;
            _rightDown = mouse.rightButton.wasPressedThisFrame;
        }
        else
        {
            // Fallback на старий Input якщо Mouse.current = null
            _mousePos  = Input.mousePosition;
            _leftDown  = Input.GetMouseButtonDown(0);
            _leftHeld  = Input.GetMouseButton(0);
            _leftUp    = Input.GetMouseButtonUp(0);
            _rightDown = Input.GetMouseButtonDown(1);
        }

        _stopDown = keyboard != null
            ? keyboard.sKey.wasPressedThisFrame
            : Input.GetKeyDown(KeyCode.S);
#else
        // Старий Input System
        _mousePos  = Input.mousePosition;
        _leftDown  = Input.GetMouseButtonDown(0);
        _leftHeld  = Input.GetMouseButton(0);
        _leftUp    = Input.GetMouseButtonUp(0);
        _rightDown = Input.GetMouseButtonDown(1);
        _stopDown  = Input.GetKeyDown(KeyCode.S);
#endif
    }

    // ---------------- Left click logic ----------------

    private void HandleLeftClick(Vector2 screenPos)
    {
        if (cam == null) return;

        Vector2 worldPos = cam.ScreenToWorldPoint(screenPos);

        Collider2D unitHit = Physics2D.OverlapPoint(worldPos, unitMask);
        if (unitHit != null)
        {
            HandleUnitClick(unitHit);
            return;
        }

        Collider2D buildingHit = Physics2D.OverlapPoint(worldPos, buildingMask);
        if (buildingHit != null)
        {
            HandleBuildingClick(buildingHit);
            return;
        }

        ClearSelection();
    }

    private void HandleUnitClick(Collider2D hit)
    {
        var eid = hit.GetComponentInParent<EntityId>();
        if (eid == null) { ClearSelection(); return; }

        var view = hit.GetComponentInParent<UnitView>();
        bool isOwnUnit = view == null
                      || lobby == null
                      || lobby.myPlayerId == 0
                      || view.owner == lobby.myPlayerId;

        if (isOwnUnit)
        {
            ClearSelection();
            AddToSelection(eid.Id, hit.GetComponentInParent<SelectionVisual>());
            return;
        }

        // Чужий юніт — атакуємо якщо є виділення
        if (selectedIds.Count > 0)
        {
            IssueAttack(eid.Id);
            return;
        }

        ClearSelection();
    }

    private void HandleBuildingClick(Collider2D hit)
    {
        var eid  = hit.GetComponentInParent<EntityId>();
        var view = hit.GetComponentInParent<BuildingView>();

        if (eid == null || view == null) { ClearSelection(); return; }

        bool isOwnBuilding = lobby == null
                          || lobby.myPlayerId == 0
                          || view.owner == lobby.myPlayerId;

        if (isOwnBuilding)
        {
            ClearSelection();
            return;
        }

        if (selectedIds.Count > 0)
        {
            IssueAttackBuilding(eid.Id);
            return;
        }

        ClearSelection();
    }

    // ---------------- Box selection ----------------

    private void SelectBox(Vector2 startScreen, Vector2 endScreen)
    {
        if (cam == null) return;

        Rect rect = RectFromScreenPoints(startScreen, endScreen);
        ClearSelection();

        var allColliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
        foreach (var col in allColliders)
        {
            if (col == null) continue;
            if ((unitMask.value & (1 << col.gameObject.layer)) == 0) continue;

            Vector3 sp = cam.WorldToScreenPoint(col.transform.position);
            if (!rect.Contains(new Vector2(sp.x, sp.y))) continue;

            var eid = col.GetComponentInParent<EntityId>();
            if (eid == null) continue;

            var view = col.GetComponentInParent<UnitView>();
            if (view != null && lobby != null && lobby.myPlayerId != 0 && view.owner != lobby.myPlayerId)
                continue;

            AddToSelection(eid.Id, col.GetComponentInParent<SelectionVisual>());
        }
    }

    private Rect RectFromScreenPoints(Vector2 a, Vector2 b)
    {
        Vector2 min = Vector2.Min(a, b);
        Vector2 max = Vector2.Max(a, b);
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    // ---------------- Selection helpers ----------------

    private void ClearSelection()
    {
        foreach (var vis in selectedVisuals)
            if (vis != null) vis.SetSelected(false);

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

        Vector2 worldPos = cam.ScreenToWorldPoint(_mousePos);

        // ✅ Якщо groundMask не налаштований — дозволяємо рух без перевірки
        if (groundMask.value != 0)
        {
            Collider2D ground = Physics2D.OverlapPoint(worldPos, groundMask);
            if (!ground) return;
        }

        foreach (int unitId in selectedIds)
        {
            if (lobby != null && lobby.inMatch)
                lobby.CmdMove(unitId, worldPos.x, worldPos.y);
            else if (sim != null)
                sim.Commands.Enqueue(new MoveCommand(unitId, worldPos));
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

    private void IssueAttackBuilding(int targetBuildingId)
    {
        if (selectedIds.Count == 0 || lobby == null || !lobby.inMatch) return;

        foreach (int attackerId in selectedIds)
            lobby.CmdAttackBuilding(attackerId, targetBuildingId);
    }

    private void IssueStop()
    {
        if (selectedIds.Count == 0 || lobby == null || !lobby.inMatch) return;

        foreach (int unitId in selectedIds)
            lobby.CmdStop(unitId);
    }
}