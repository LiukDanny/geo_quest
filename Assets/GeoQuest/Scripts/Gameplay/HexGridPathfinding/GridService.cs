using System;
using System.Collections.Generic;
using UnityEngine;

public class GridService
{
    private const int MOVE_STRAIGHT_COST = 10;

    private HexGrid<PathNode> grid;
    private HexGridData gridData;
    private List<PathNode> openList;
    private List<PathNode> closedList;

    public GridService(HexGridData data)
    {
        if (data.GetGrid() == null)
        {
            data.Init();
        }
        gridData = data;
        grid = data.GetGrid();
    }

    public HexGrid<PathNode> GetGrid()
    {
        return grid;
    }

    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition, Func<PathNode, bool> canStandHere)
    {
        grid.GetXY(startWorldPosition, out int startX, out int startY);
        grid.GetXY(endWorldPosition, out int endX, out int endY);

        List<PathNode> path = FindPath(startX, startY, endX, endY, canStandHere);
        if (path == null)
        {
            return null;
        }
        else
        {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (PathNode pathNode in path)
            {
                vectorPath.Add(grid.GetWorldPosition(pathNode.x, pathNode.y));
            }
            return vectorPath;
        }
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY, Func<PathNode, bool> canStandHere)
    {
        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = FindNearestValidNode(endX, endY, canStandHere);

        if (startNode == null || endNode == null)
        {
            return null;
        }

        openList = new List<PathNode> { startNode };
        closedList = new List<PathNode>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode pathNode = grid.GetGridObject(x, y);
                pathNode.gCost = 99999999;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                // Reached final node
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                if (closedList.Contains(neighbourNode)) continue;

                if (!canStandHere(neighbourNode))
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }

        // Out of nodes on the openList
        return null;
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        bool oddRow = currentNode.y % 2 == 1;

        if (currentNode.x - 1 >= 0)
        {
            // Left
            neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
        }
        if (currentNode.x + 1 < grid.GetWidth())
        {
            // Right
            neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));
        }
        if (currentNode.y - 1 >= 0)
        {
            // Down
            neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1));
        }
        if (currentNode.y + 1 < grid.GetHeight())
        {
            // Up
            neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1));
        }

        if (oddRow)
        {
            if (currentNode.y + 1 < grid.GetHeight() && currentNode.x + 1 < grid.GetWidth())
            {
                neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
            }
            if (currentNode.y - 1 >= 0 && currentNode.x + 1 < grid.GetWidth())
            {
                neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
            }
        }
        else
        {
            if (currentNode.y + 1 < grid.GetHeight() && currentNode.x - 1 >= 0)
            {
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
            }
            if (currentNode.y - 1 >= 0 && currentNode.x - 1 >= 0)
            {
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
            }
        }


        return neighbourList;
    }

    public PathNode GetNode(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }

    public int GetTileDistance(Vector3 a, Vector3 b)
    {
        grid.GetXY(a, out int ax, out int ay);
        grid.GetXY(b, out int bx, out int by);
        return GetTileDistance(ax, ay, bx, by);
    }

    private int GetTileDistance(int ax, int ay, int bx, int by)
    {
        int xDistance = Mathf.Abs(ax - bx);
        int yDistance = Mathf.Abs(ay - by);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return Mathf.Min(xDistance, yDistance) + remaining;
    }
    
    //TODO: Check if node is occupied or blocked
    public Vector2 GetNearestPoint(Vector2 position)
    {
        grid.GetNearestXY(position, out int x, out int y);
        return grid.GetWorldPosition(x, y);
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        return MOVE_STRAIGHT_COST * GetTileDistance(a.x, a.y, b.x, b.y);
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }

    private PathNode FindNearestValidNode(
    int targetX,
    int targetY,
    Func<PathNode, bool> canStandHere)
    {
        PathNode start = grid.GetGridObject(targetX, targetY);
        if (start == null)
            return null;

        Queue<PathNode> queue = new();
        HashSet<PathNode> visited = new();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            PathNode node = queue.Dequeue();

            if (canStandHere(node))
                return node;

            foreach (var n in GetNeighbourList(node))
            {
                if (n == null) continue;
                if (visited.Contains(n)) continue;

                visited.Add(n);
                queue.Enqueue(n);
            }
        }

        return null;
    }
}
