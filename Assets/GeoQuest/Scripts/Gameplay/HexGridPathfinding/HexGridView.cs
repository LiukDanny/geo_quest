using UnityEngine;

public class HexGridView : MonoBehaviour
{
    [Header("Config")]
    //[SerializeField] private HexGridData data;
    [SerializeField] private Transform hexPrefab;

    [Header("Debug")]
    [SerializeField] private bool showInPlayMode = false;
    public bool editMode;

    private TileView[,] tiles;
    public HexGridData Data ;

    //public void CreateGrid()
    public void Init(HexGridData data)
    {
        Data = data;
        ClearGrid();

        tiles = new TileView[data.width, data.height];

        for (int x = 0; x < data.width; x++)
        {
            for (int y = 0; y < data.height; y++)
            {
                if (data.GetGrid() == null) { data.Init(); }

                Vector3 pos = data.GetGrid().GetWorldPosition(x, y);
                var t = Instantiate(hexPrefab, pos, Quaternion.identity, transform);
                t.localScale = Vector3.one * data.cellSize;

                var tile = t.GetComponent<TileView>();
                tile.Init(x, y, this);

                tiles[x, y] = tile;
            }
        }
    }

    public void ClearGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }

    public void ToggleTile(int x, int y)
    {
        bool current = Data.IsWalkable(x, y);
        Data.SetWalkable(x, y, !current);
        tiles[x, y].UpdateVisual();
    }

    public void SetTileOccupied(int x, int y, bool isOccupied) 
    {
        if (tiles != null && tiles[x, y] != null)
        {
            tiles[x, y].SetOccupied(isOccupied);
        }

    }

    private void Update()
    {
        if (Application.isPlaying && !showInPlayMode)
        {
            if (transform.childCount > 0)
                ClearGrid();
        }

        if (Application.isPlaying && showInPlayMode) 
        {
            CheckTilePressed();
        }
    }


    private void CheckTilePressed()
    {
        if (!editMode)
            return;
        if (Input.GetMouseButtonDown(0))
        {
            Data.GetGrid().GetXY(GetMouseWorldPosition(), out int x, out int y);
            ToggleTile(x, y);
        }
        
    }

    private Vector3 GetMouseWorldPosition()//refactor
    {

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);


        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        return hit.point;
    }
}