using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(menuName = "GeoQuest/Grid/Hex Grid Data")]
public class HexGridData : ScriptableObject
{
    public int width;
    public int height;
    public float cellSize;
    public Vector2 origin;

    public float HEX_VERTICAL_OFFSET_MULTIPLIER = 0.75f;

    public bool[] walkable; // size = width * height

    private HexGrid<PathNode> grid;

    public void Init()
    {
        int requiredLength = width * height;

        if (walkable.Length != requiredLength)
        {
            var oldWalkable = walkable;
            walkable = new bool[requiredLength];

            for (int i = 0; i < walkable.Length; i++)
                walkable[i] = i < oldWalkable.Length ? oldWalkable[i] : true;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        grid = new HexGrid<PathNode>(this, (HexGrid<PathNode> g, int x, int y) => new PathNode(g, x, y, IsWalkable(x,y)));
    }

    public bool IsWalkable(int x, int y)
    {
        return walkable[y * width + x];
    }

    public void SetWalkable(int x, int y, bool value)
    {
        walkable[y * width + x] = value;
        grid.GetGridObject(x, y).SetIsWalkable(value);
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    public HexGrid<PathNode> GetGrid() => grid;
}
