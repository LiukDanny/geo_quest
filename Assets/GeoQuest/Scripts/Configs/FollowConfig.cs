using UnityEngine;

[CreateAssetMenu(menuName = "GeoQuest/Config/Follow Config")]
public class FollowConfig : ScriptableObject
{
    public bool Enabled = true;
    public float MaxDistanceTiles = 5f;
    public float IntervalSeconds = 3f;
}
