using System.Collections.Generic;
using GeoQuest.Data;
using GeoQuest.Scripts.Data;
using UnityEngine;

[CreateAssetMenu(menuName = "GeoQuest/LevelConfig")]

public class LevelConfig : ScriptableObject
{
    public string id;
    public string displayName;
    public InteractionDatabase interactionDatabase;
    public HexGridData gridConfig;
    public GameObject background;
    public List<GridObjectData> objects = new();
}
