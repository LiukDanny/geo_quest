using UnityEditor;
using UnityEngine;
using System;

public class CreateAssetPopup : EditorWindow
{
    string assetName = "";
    Type assetType;
    string path;
    string label;
    Func<string, UnityEngine.Object> createFunc;

    Action<UnityEngine.Object> onCreated;

    public static void Show<T>(
        Func<string, T> createFunc,
        Action<T> onCreated,
        string label = null
    ) where T : UnityEngine.Object
    {
        Show(
            typeof(T),
            name => createFunc(name),
            obj => onCreated?.Invoke(obj as T),
            label
        );
    }

    public static void Show(
        Type assetType,
        Func<string, UnityEngine.Object> createFunc,
        Action<UnityEngine.Object> onCreated,
        string label = null
    )
    {
        var window = CreateInstance<CreateAssetPopup>();

        window.assetType = assetType;
        window.label = label ?? assetType.Name;
        window.createFunc = createFunc;
        window.onCreated = onCreated;

        window.titleContent = new GUIContent($"Create {window.label}");
        var origin = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), Vector2.zero);
        window.ShowAsDropDown(origin, new Vector2(320, 90));
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField(
            $"New {label}",
            EditorStyles.boldLabel
        );

        assetName = EditorGUILayout.TextField("Name", assetName);

        GUILayout.Space(6);

        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(assetName)))
        {
            if (GUILayout.Button("Create"))
            {
                var asset = createFunc.Invoke(assetName);

                onCreated?.Invoke(asset);
                Close();
            }
        }
    }
}
