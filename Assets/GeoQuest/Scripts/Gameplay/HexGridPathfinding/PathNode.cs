using UnityEngine;

public class PathNode
{
    private HexGrid<PathNode> grid;

    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public int fCost;

    private bool isWalkable;
    private bool isOccupied;
    public PathNode cameFromNode;

    public PathNode(HexGrid<PathNode> grid, int x, int y, bool isWalkable)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.isWalkable = isWalkable;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public void SetIsWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
        grid.TriggerGridObjectChanged(x, y);
    }

    public void SetIsOccupied(bool isOccupied)
    {
        this.isOccupied = isOccupied;
        grid.TriggerGridObjectChanged(x, y);
    }

    public bool IsAvailable() => isWalkable && !isOccupied;

    public override string ToString()
    {
        return x + "," + y;
    }
}
