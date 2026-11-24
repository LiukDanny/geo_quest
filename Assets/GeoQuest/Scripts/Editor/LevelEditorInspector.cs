#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelEditor))]
public class LevelEditorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        LevelEditor editor = (LevelEditor)target;

        if (GUILayout.Button("Generate Level Config"))
        {
            editor.GenerateConfig();
        }
    }
}
#endif