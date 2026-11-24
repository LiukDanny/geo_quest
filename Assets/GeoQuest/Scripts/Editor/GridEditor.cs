using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HexGridView))]
public class GridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HexGridView grid = (HexGridView)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Create Grid"))
        {
            if (grid.Data == null)
            {
                Debug.LogError("No GridData assigned");
            }
            else
            {
                grid.Init(grid.Data);
            }
        }

        if (GUILayout.Button("Clear Grid"))
        {
            grid.ClearGrid();
        }
    }
}
