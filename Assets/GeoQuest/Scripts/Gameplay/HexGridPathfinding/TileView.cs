using UnityEngine;

public class TileView : MonoBehaviour
{
    private int x;
    private int y;
    private HexGridView grid;
    [SerializeField] private MeshRenderer meshRenderer;

    public void Init(int x, int y, HexGridView grid)
    {
        this.x = x;
        this.y = y;
        this.grid = grid;
        UpdateVisual();
    }


    public void UpdateVisual()
    {
        bool walkable = grid.Data.IsWalkable(x, y);
        meshRenderer.material.color = walkable ? Color.white : Color.red;
    }

    public void SetOccupied(bool isOccupied) 
    {
        bool walkable = grid.Data.IsWalkable(x, y);

        meshRenderer.material.color = walkable && !isOccupied ? Color.white : Color.red;
    }
}