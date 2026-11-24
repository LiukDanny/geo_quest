using System.Collections.Generic;
using GeoQuest;
using GeoQuest.Data;
using GeoQuest.Scripts.Data;
using UnityEngine;

public class LevelEditor : MonoBehaviour
{
    public string id;
    public InteractionDatabase interactionDatabase;
    public HexGridData gridConfig;
    public GameObject background;

    [Header("Scene References")]
    [SerializeField] private Transform objectsRoot;

    [HideInInspector] public List<GridObjectData> objects = new();

    public void GenerateConfig()
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogError("Level ID is empty!");
            return;
        }

        if (interactionDatabase == null || gridConfig == null || background == null)
        {
            Debug.LogError("Missing references!");
            return;
        }

        if (objectsRoot == null)
        {
            Debug.LogError("ObjectsRoot is null!");
            return;
        }

        BakeObjects();

        LevelConfig config = LevelEditorUtility.CreateOrLoadConfig(id);

        config.id = id;
        config.interactionDatabase = interactionDatabase;
        config.gridConfig = gridConfig;
        config.background = background;

        config.objects.Clear();
        config.objects.AddRange(objects);

        UnityEditor.EditorUtility.SetDirty(config);
        UnityEditor.AssetDatabase.SaveAssets();

        Debug.Log($"LevelConfig '{id}' saved!");
#endif
    }

    private void BakeObjects()
    {
        objects.Clear();

        gridConfig.Init();
        var grid = gridConfig.GetGrid();

        foreach (Transform child in objectsRoot)
        {
            var interactable = child.GetComponentInChildren<Interactable>();
            if (interactable == null)
                continue;

            var collider = child.GetComponentInChildren<Collider2D>();
            if (collider == null)
            {
                Debug.LogWarning($"{child.name} has no Collider2D");
                continue;
            }

            Debug.Log(grid);
            Debug.Log(child);
            grid.GetXY(child.position, out int x, out int y);

            // var footprint = GridFootprintCalculator.Calculate(
            //     collider,
            //     gridConfig
            // );

            objects.Add(new GridObjectData
            {
                itemId = interactable.Item.Target.Id,
                anchor = new Vector2Int(x, y),
                //occupiedCells = footprint
            });
        }
    }
}