using UnityEngine;

namespace GeoQuest.Data
{
    [CreateAssetMenu(menuName = "GeoQuest/Action/Tag")]
    public class ActionTag : ScriptableObject
    {
        public string Id;
        public string Name;
        public Sprite Icon;
        public bool isCharacterAction;
    }
}
