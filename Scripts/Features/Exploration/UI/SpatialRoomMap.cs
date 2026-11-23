using System;
using System.Collections.Generic;
using Godot;

/// <summary>Draws and controls the first top-down grid room using simple presentation-only markers.</summary>
public partial class SpatialRoomMap : Control
{
    private const string _roomLocationId = "dormitory_room";
    private const int _columns = 12;
    private const int _rows = 9;
    private const float _cellSize = 38.0f;
    private const float _mapOffset = 14.0f;

    private static readonly GridCell _playerStart = new GridCell(5, 6);
    private static readonly Dictionary<string, GridCell> _interactableCells =
        new Dictionary<string, GridCell>(StringComparer.Ordinal)
        {
            ["bed"] = new GridCell(2, 2),
            ["laptop"] = new GridCell(8, 2),
            ["wardrobe"] = new GridCell(9, 5)
        };
    private static readonly Dictionary<string, GridCell> _exitCells =
        new Dictionary<string, GridCell>(StringComparer.Ordinal)
        {
            ["dormitory_lobby"] = new GridCell(4, 8),
            ["dormitory_bathroom"] = new GridCell(8, 8)
        };

    private readonly SpatialGrid _grid = CreateGrid();
    private readonly HashSet<string> _availableInteractables = new HashSet<string>(StringComparer.Ordinal);
    private readonly Dictionary<string, NpcPresence> _presentNpcs = new Dictionary<string, NpcPresence>(StringComparer.Ordinal);
    private readonly Dictionary<string, GridCell[]> _npcRoutes = new Dictionary<string, GridCell[]>(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _npcRouteIndices = new Dictionary<string, int>(StringComparer.Ordinal);
    private string _locationId = string.Empty;
    private string[] _exits = Array.Empty<string>();
    private GridCell _playerCell = _playerStart;
    private double _npcMovementElapsed;

    public event Action<string> TargetActivated;

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(0, 392);
        FocusMode = FocusModeEnum.All;
        QueueRedraw();
    }

    /// <summary>Updates the room map from the current Location and scheduled NPC presence.</summary>
    public void SetLocation(
        string locationId,
        IReadOnlyList<LocationInteractableDefinition> interactables,
        IReadOnlyList<NpcPresence> presentNpcs,
        string[] exits)
    {
        bool locationChanged = !string.Equals(_locationId, locationId, StringComparison.Ordinal);
        _locationId = locationId ?? string.Empty;
        Visible = string.Equals(_locationId, _roomLocationId, StringComparison.Ordinal);
        if (!Visible)
        {
            QueueRedraw();
            return;
        }

        if (locationChanged) _playerCell = _playerStart;
        _exits = exits == null ? Array.Empty<string>() : (string[])exits.Clone();
        _availableInteractables.Clear();
        if (interactables != null)
        {
            foreach (LocationInteractableDefinition interactable in interactables)
            {
                if (interactable != null)
                {
                    _availableInteractables.Add(interactable.Id);
                }
            }
        }

        _presentNpcs.Clear();

        if (presentNpcs != null)
        {
            foreach (NpcPresence presence in presentNpcs)
            {
                if (presence == null || string.IsNullOrWhiteSpace(presence.CharacterId)) continue;
                _presentNpcs[presence.CharacterId] = presence;
                if (_npcRoutes.ContainsKey(presence.CharacterId)) continue;

                GridCell[] route = SpatialNpcRouteCatalog.GetRoute(presence.CharacterId);
                if (route.Length == 0) route = new[] { new GridCell(5, 4) };
                _npcRoutes[presence.CharacterId] = route;
                _npcRouteIndices[presence.CharacterId] = 0;
            }
        }

        RemoveDepartedNpcRoutes();
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (!Visible || _presentNpcs.Count == 0) return;
        _npcMovementElapsed += delta;
        if (_npcMovementElapsed < 1.8) return;
        _npcMovementElapsed = 0;
        MoveScheduledNpcs();
        QueueRedraw();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible || @event is not InputEventKey key || !key.Pressed || key.Echo) return;

        int columnDelta = 0;
        int rowDelta = 0;
        switch (key.Keycode)
        {
            case Key.W:
            case Key.Up: rowDelta = -1; break;
            case Key.S:
            case Key.Down: rowDelta = 1; break;
            case Key.A:
            case Key.Left: columnDelta = -1; break;
            case Key.D:
            case Key.Right: columnDelta = 1; break;
            case Key.E:
            case Key.Enter:
                ActivateNearbyTarget();
                GetViewport().SetInputAsHandled();
                return;
            default: return;
        }

        GridCell destination = _grid.Move(_playerCell, columnDelta, rowDelta);
        if (!IsOccupiedByNpc(destination)) _playerCell = destination;
        QueueRedraw();
        GetViewport().SetInputAsHandled();
    }

    public override void _Draw()
    {
        if (!Visible) return;

        DrawRect(
            new Rect2(0, 0, _mapOffset * 2 + _columns * _cellSize, 392),
            new Color(0.055f, 0.067f, 0.082f, 1), true);

        for (int row = 0; row < _rows; row++)
        {
            for (int column = 0; column < _columns; column++)
            {
                GridCell cell = new GridCell(column, row);
                Rect2 rect = GetCellRect(cell);
                Color tileColor = _grid.CanEnter(cell)
                    ? new Color(0.31f, 0.34f, 0.34f, 1)
                    : new Color(0.20f, 0.22f, 0.25f, 1);
                DrawRect(rect, tileColor, true);
                DrawRect(rect, new Color(0.12f, 0.14f, 0.15f, 1), false, 1.0f);
            }
        }

        if (_availableInteractables.Contains("bed"))
            DrawFurniture("BED", _interactableCells["bed"], new Color(0.56f, 0.36f, 0.32f, 1), new Vector2(2, 2));
        if (_availableInteractables.Contains("laptop"))
            DrawFurniture("LAPTOP", _interactableCells["laptop"], new Color(0.33f, 0.48f, 0.61f, 1), Vector2.One);
        if (_availableInteractables.Contains("wardrobe"))
            DrawFurniture("WARD", _interactableCells["wardrobe"], new Color(0.48f, 0.39f, 0.29f, 1), new Vector2(2, 1));

        foreach (string exitId in _exits)
        {
            if (!_exitCells.TryGetValue(exitId, out GridCell exitCell)) continue;
            DrawRect(GetCellRect(exitCell).Grow(-3), new Color(0.67f, 0.55f, 0.31f, 1), true);
            DrawCellText(exitCell, exitId == "dormitory_lobby" ? "LOBBY" : "BATH", Colors.White);
        }

        foreach (KeyValuePair<string, NpcPresence> entry in _presentNpcs)
        {
            Vector2 center = GetCellRect(GetNpcCell(entry.Key)).GetCenter();
            DrawCircle(center, 11, new Color(0.75f, 0.55f, 0.86f, 1));
            DrawString(ThemeDB.FallbackFont, center + new Vector2(-4, 4), entry.Value.Name.Substring(0, 1),
                HorizontalAlignment.Left, -1, 12, Colors.White);
            DrawString(ThemeDB.FallbackFont, center + new Vector2(-18, -13), entry.Value.Name,
                HorizontalAlignment.Center, 64, 10, new Color(0.93f, 0.91f, 0.86f, 1));
        }

        Vector2 playerCenter = GetCellRect(_playerCell).GetCenter();
        DrawCircle(playerCenter, 12, new Color(0.36f, 0.78f, 0.69f, 1));
        DrawCircle(playerCenter, 5, new Color(0.08f, 0.17f, 0.17f, 1));

        string nearbyLabel = GetNearbyTargetLabel();
        DrawString(
            ThemeDB.FallbackFont,
            new Vector2(_mapOffset, _mapOffset * 2 + _rows * _cellSize + 18),
            string.IsNullOrEmpty(nearbyLabel)
                ? "WASD / ARROWS  Move     E  Interact"
                : "E  " + nearbyLabel + "     ·     WASD / ARROWS  Move",
            HorizontalAlignment.Left, -1, 12, new Color(0.91f, 0.85f, 0.68f, 1));
    }

    private static SpatialGrid CreateGrid()
    {
        HashSet<GridCell> blocked = new HashSet<GridCell>();
        for (int column = 0; column < _columns; column++)
        {
            blocked.Add(new GridCell(column, 0));
            blocked.Add(new GridCell(column, _rows - 1));
        }
        for (int row = 0; row < _rows; row++)
        {
            blocked.Add(new GridCell(0, row));
            blocked.Add(new GridCell(_columns - 1, row));
        }

        blocked.Add(new GridCell(2, 2));
        blocked.Add(new GridCell(3, 2));
        blocked.Add(new GridCell(2, 3));
        blocked.Add(new GridCell(3, 3));
        blocked.Add(new GridCell(8, 2));
        blocked.Add(new GridCell(9, 5));
        blocked.Add(new GridCell(10, 5));
        blocked.Remove(new GridCell(4, _rows - 1));
        blocked.Remove(new GridCell(8, _rows - 1));
        return new SpatialGrid(_columns, _rows, blocked);
    }

    private Rect2 GetCellRect(GridCell cell) =>
        new Rect2(_mapOffset + cell.Column * _cellSize, _mapOffset + cell.Row * _cellSize, _cellSize, _cellSize);

    private void DrawFurniture(string label, GridCell cell, Color color, Vector2 footprint)
    {
        Rect2 rect = GetCellRect(cell);
        rect.Size = new Vector2(footprint.X * _cellSize, footprint.Y * _cellSize);
        DrawRect(rect.Grow(-3), color, true);
        DrawRect(rect.Grow(-3), new Color(0.10f, 0.12f, 0.14f, 1), false, 2.0f);
        DrawString(ThemeDB.FallbackFont, rect.GetCenter() + new Vector2(-rect.Size.X * 0.45f, 4),
            label, HorizontalAlignment.Center, rect.Size.X * 0.9f, 9, Colors.White);
    }

    private void DrawCellText(GridCell cell, string text, Color color)
    {
        Rect2 rect = GetCellRect(cell);
        DrawString(ThemeDB.FallbackFont, rect.GetCenter() + new Vector2(-16, 4), text,
            HorizontalAlignment.Left, 34, 8, color);
    }

    private GridCell GetNpcCell(string characterId)
    {
        GridCell[] route = _npcRoutes[characterId];
        return route[_npcRouteIndices[characterId] % route.Length];
    }

    private void MoveScheduledNpcs()
    {
        foreach (string characterId in _presentNpcs.Keys)
        {
            GridCell[] route = _npcRoutes[characterId];
            if (route.Length <= 1) continue;
            int nextIndex = (_npcRouteIndices[characterId] + 1) % route.Length;
            GridCell nextCell = route[nextIndex];
            if (nextCell == _playerCell || IsOccupiedByAnotherNpc(characterId, nextCell)) continue;
            _npcRouteIndices[characterId] = nextIndex;
        }
    }

    private bool IsOccupiedByNpc(GridCell cell)
    {
        foreach (string characterId in _presentNpcs.Keys)
            if (GetNpcCell(characterId) == cell) return true;
        return false;
    }

    private bool IsOccupiedByAnotherNpc(string currentCharacterId, GridCell cell)
    {
        foreach (string characterId in _presentNpcs.Keys)
            if (characterId != currentCharacterId && GetNpcCell(characterId) == cell) return true;
        return false;
    }

    private void RemoveDepartedNpcRoutes()
    {
        List<string> departed = new List<string>();
        foreach (string characterId in _npcRoutes.Keys)
            if (!_presentNpcs.ContainsKey(characterId)) departed.Add(characterId);
        foreach (string characterId in departed)
        {
            _npcRoutes.Remove(characterId);
            _npcRouteIndices.Remove(characterId);
        }
    }

    private string GetNearbyTargetLabel() =>
        TryGetNearbyTarget(out string label, out _, out _) ? label : string.Empty;

    private void ActivateNearbyTarget()
    {
        if (!TryGetNearbyTarget(out _, out InteractionSourceKind sourceKind, out string targetId)) return;
        ActionSelectionRequest request;
        switch (sourceKind)
        {
            case InteractionSourceKind.Interactable:
                request = new ActionSelectionRequest(sourceKind, _locationId, "select", targetId);
                break;
            case InteractionSourceKind.Npc:
                request = new ActionSelectionRequest(sourceKind, targetId, "select");
                break;
            case InteractionSourceKind.Travel:
                request = new ActionSelectionRequest(sourceKind, _locationId, "travel", targetId);
                break;
            default: return;
        }
        TargetActivated?.Invoke(request.Encode());
    }

    private bool TryGetNearbyTarget(out string label, out InteractionSourceKind sourceKind, out string targetId)
    {
        foreach (KeyValuePair<string, GridCell> entry in _interactableCells)
        {
            if (!_availableInteractables.Contains(entry.Key) || GridCellDistance(entry.Value, _playerCell) > 1) continue;
            label = "Interact: " + entry.Key;
            sourceKind = InteractionSourceKind.Interactable;
            targetId = entry.Key;
            return true;
        }

        foreach (string characterId in _presentNpcs.Keys)
        {
            if (GridCellDistance(GetNpcCell(characterId), _playerCell) > 1) continue;
            label = "Talk: " + _presentNpcs[characterId].Name;
            sourceKind = InteractionSourceKind.Npc;
            targetId = characterId;
            return true;
        }

        foreach (string exitId in _exits)
        {
            if (!_exitCells.TryGetValue(exitId, out GridCell exitCell)
                || GridCellDistance(exitCell, _playerCell) > 1) continue;
            label = "Enter: " + (exitId == "dormitory_lobby" ? "Lobby" : "Bathroom");
            sourceKind = InteractionSourceKind.Travel;
            targetId = exitId;
            return true;
        }

        label = string.Empty;
        sourceKind = InteractionSourceKind.Player;
        targetId = string.Empty;
        return false;
    }

    private static int GridCellDistance(GridCell first, GridCell second) =>
        Math.Abs(first.Column - second.Column) + Math.Abs(first.Row - second.Row);
}