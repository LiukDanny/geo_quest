#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class LevelEditorUtility
{
    private const string PATH = "Assets/GeoQuest/Configs/Levels/";

    public static LevelConfig CreateOrLoadConfig(string id)
    {
        if (!Directory.Exists(PATH))
        {
            Directory.CreateDirectory(PATH);
        }

        string fullPath = PATH + id + ".asset";

        var config = AssetDatabase.LoadAssetAtPath<LevelConfig>(fullPath);

        if (config == null)
        {
            config = ScriptableObject.CreateInstance<LevelConfig>();
            AssetDatabase.CreateAsset(config, fullPath);
            Debug.Log($"Created new LevelConfig: {id}");
        }

        return config;
    }
}
#endif