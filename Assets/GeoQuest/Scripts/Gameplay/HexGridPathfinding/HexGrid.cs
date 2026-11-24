using System;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid<TGridObject>
{

    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    private TGridObject[,] gridArray;

    private HexGridData GridData;

    public HexGrid(HexGridData data, Func<HexGrid<TGridObject>, int, int, TGridObject> createGridObject)
    {
        GridData = data;

        gridArray = new TGridObject[data.width, data.height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }
    }

    public int GetWidth() => GridData.width;

    public int GetHeight() => GridData.height;

    public Vector2 GetWorldPosition(int x, int y)
    {
        return
            new Vector2(x, 0) * GridData.cellSize +
            new Vector2(0, y) * (GridData.cellSize * GridData.HEX_VERTICAL_OFFSET_MULTIPLIER) +
            ((Mathf.Abs(y) % 2) == 1 ? new Vector2(1, 0) * (GridData.cellSize * .5f) : Vector2.zero) +
            GridData.origin;
    }

    public void GetXY(Vector2 worldPosition, out int x, out int y)
    {
        int roughX = Mathf.RoundToInt((worldPosition - GridData.origin).x / GridData.cellSize);
        int roughY = Mathf.RoundToInt((worldPosition - GridData.origin).y / GridData.cellSize / GridData.HEX_VERTICAL_OFFSET_MULTIPLIER);

        Vector2Int roughXY = new Vector2Int(roughX, roughY);

        bool oddRow = roughY % 2 == 1;

        List<Vector2Int> neighbourXYList = new List<Vector2Int> {
             roughXY + new Vector2Int(-1, 0),
             roughXY + new Vector2Int(+1, 0),

             roughXY + new Vector2Int(oddRow ? +1 : -1, +1),
             roughXY + new Vector2Int(+0, +1),

             roughXY + new Vector2Int(oddRow ? +1 : -1, -1),
             roughXY + new Vector2Int(+0, -1),
        };

        Vector2Int closestXY = roughXY;

        foreach (Vector2Int neighbourXY in neighbourXYList)
        {
            if (Vector2.Distance(worldPosition, GetWorldPosition(neighbourXY.x, neighbourXY.y)) <
                Vector2.Distance(worldPosition, GetWorldPosition(closestXY.x, closestXY.y)))
            {
                // Closer than closest
                closestXY = neighbourXY;
            }

        }

        x = closestXY.x;
        y = closestXY.y;
    }

    public void SetGridObject(int x, int y, TGridObject value)
    {
        if (x >= 0 && y >= 0 && x < GridData.width && y < GridData.height)
        {
            gridArray[x, y] = value;
            TriggerGridObjectChanged(x, y);
        }
    }

    public void TriggerGridObjectChanged(int x, int y)
    {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }

    /*public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        GetXY(worldPosition, out int x, out int y);
        SetGridObject(x, y, value);
    }*/

    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < GridData.width && y < GridData.height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TGridObject);
        }
    }

    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }

    public void GetNearestXY(Vector2 position, out int x, out int y)
    {
        var clampedPos = new Vector2(
            Mathf.Clamp(position.x, 0, (GetWidth() - 1)  * GridData.cellSize),
            Mathf.Clamp(position.y, 0, (GetHeight() - 1) * GridData.cellSize * GridData.HEX_VERTICAL_OFFSET_MULTIPLIER)
        );
        
        GetXY(clampedPos, out x, out y);
    }
}
