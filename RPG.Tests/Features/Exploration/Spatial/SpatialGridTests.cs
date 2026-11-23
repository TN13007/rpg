using Xunit;

public sealed class SpatialGridTests
{
    [Fact]
    public void Move_EntersWalkableAdjacentCell()
    {
        SpatialGrid grid = new SpatialGrid(4, 4);
        Assert.Equal(new GridCell(2, 1), grid.Move(new GridCell(1, 1), 1, 0));
    }

    [Fact]
    public void Move_StaysInPlaceWhenDestinationIsBlockedOrOutsideBounds()
    {
        SpatialGrid grid = new SpatialGrid(4, 4, new[] { new GridCell(2, 1) });
        Assert.Equal(new GridCell(1, 1), grid.Move(new GridCell(1, 1), 1, 0));
        Assert.Equal(new GridCell(0, 1), grid.Move(new GridCell(1, 1), -1, 0));
        Assert.Equal(new GridCell(0, 1), grid.Move(new GridCell(0, 1), -1, 0));
    }

    [Fact]
    public void Move_RejectsDiagonalAndMultiCellMovement()
    {
        SpatialGrid grid = new SpatialGrid(4, 4);
        Assert.Throws<System.ArgumentException>(() => grid.Move(new GridCell(1, 1), 1, 1));
        Assert.Throws<System.ArgumentException>(() => grid.Move(new GridCell(1, 1), 2, 0));
    }

    [Fact]
    public void IsAdjacent_UsesManhattanDistance()
    {
        Assert.True(SpatialGrid.IsAdjacent(new GridCell(1, 1), new GridCell(2, 1)));
        Assert.True(SpatialGrid.IsAdjacent(new GridCell(1, 1), new GridCell(1, 2)));
        Assert.False(SpatialGrid.IsAdjacent(new GridCell(1, 1), new GridCell(2, 2)));
        Assert.False(SpatialGrid.IsAdjacent(new GridCell(1, 1), new GridCell(1, 1)));
    }

    [Fact]
    public void NpcRouteCatalog_ReturnsCopyOfAuthoredRoute()
    {
        GridCell[] first = SpatialNpcRouteCatalog.GetRoute("mira");
        first[0] = new GridCell(0, 0);
        Assert.Equal(new GridCell(4, 4), SpatialNpcRouteCatalog.GetRoute("mira")[0]);
        Assert.Empty(SpatialNpcRouteCatalog.GetRoute("unknown"));
    }
}