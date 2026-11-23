using System;
using System.Collections.Generic;

/// <summary>Identifies one integer cell in a two-dimensional exploration map.</summary>
public readonly struct GridCell : IEquatable<GridCell>
{
    public int Column { get; }
    public int Row { get; }

    public GridCell(int column, int row)
    {
        Column = column;
        Row = row;
    }

    public bool Equals(GridCell other) => Column == other.Column && Row == other.Row;
    public override bool Equals(object obj) => obj is GridCell other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Column, Row);
    public static bool operator ==(GridCell left, GridCell right) => left.Equals(right);
    public static bool operator !=(GridCell left, GridCell right) => !left.Equals(right);
}

/// <summary>Provides bounds, blocking and adjacency rules for a small tile-based space.</summary>
public sealed class SpatialGrid
{
    private readonly HashSet<GridCell> _blockedCells;
    public int Width { get; }
    public int Height { get; }

    public SpatialGrid(int width, int height, IEnumerable<GridCell> blockedCells = null)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        Width = width;
        Height = height;
        _blockedCells = new HashSet<GridCell>();
        if (blockedCells == null) return;

        foreach (GridCell cell in blockedCells)
        {
            if (!Contains(cell))
                throw new ArgumentOutOfRangeException(nameof(blockedCells), "Blocked cells must be inside the grid.");
            _blockedCells.Add(cell);
        }
    }

    public bool Contains(GridCell cell) =>
        cell.Column >= 0 && cell.Column < Width && cell.Row >= 0 && cell.Row < Height;

    public bool CanEnter(GridCell cell) => Contains(cell) && !_blockedCells.Contains(cell);

    public GridCell Move(GridCell current, int columnDelta, int rowDelta)
    {
        if (Math.Abs(columnDelta) + Math.Abs(rowDelta) != 1)
            throw new ArgumentException("Movement must target one adjacent cardinal cell.");
        GridCell destination = new GridCell(current.Column + columnDelta, current.Row + rowDelta);
        return CanEnter(destination) ? destination : current;
    }

    public static bool IsAdjacent(GridCell first, GridCell second) =>
        Math.Abs(first.Column - second.Column) + Math.Abs(first.Row - second.Row) == 1;
}

/// <summary>Provides simple authored walking routes for NPC markers in the first spatial room.</summary>
public static class SpatialNpcRouteCatalog
{
    private static readonly Dictionary<string, GridCell[]> _routes =
        new Dictionary<string, GridCell[]>(StringComparer.Ordinal)
        {
            ["mira"] = new[] { new GridCell(4, 4), new GridCell(5, 4), new GridCell(5, 5), new GridCell(4, 5) },
            ["noah"] = new[] { new GridCell(7, 4), new GridCell(8, 4), new GridCell(8, 5), new GridCell(7, 5) },
            ["sera"] = new[] { new GridCell(3, 5), new GridCell(3, 6), new GridCell(4, 6), new GridCell(4, 5) },
            ["ms_carter"] = new[] { new GridCell(6, 5), new GridCell(6, 6), new GridCell(7, 6), new GridCell(7, 5) }
        };

    public static GridCell[] GetRoute(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId) || !_routes.TryGetValue(characterId, out GridCell[] route))
            return Array.Empty<GridCell>();
        return (GridCell[])route.Clone();
    }
}