using UnityEngine;

[CreateAssetMenu(menuName = "GeoQuest/Config/Game Config")]
public class GameConfig : ScriptableObject
{
    [SerializeField] private FollowConfig _follow;
    [SerializeField] private ActionRangeConfig _actionRanges;

    public FollowConfig Follow => _follow;
    public ActionRangeConfig ActionRanges => _actionRanges;
}
