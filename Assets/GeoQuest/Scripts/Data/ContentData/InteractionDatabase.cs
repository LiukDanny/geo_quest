using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GeoQuest.Data
{
    [CreateAssetMenu(menuName = "Game/Interaction/Database")]
    public class InteractionDatabase : ScriptableObject
    {
        //public Location Location;
        //public Character Character;
        public List<InteractionRule> Rules = new();

        public Item FindItem(string id)
        {
            var item = Rules.FirstOrDefault(r => r.Target.Id == id)?.Target;
            if (item == null)
                Debug.LogError($"Item '{id}' has no rule in interaction database '{name}'");
            return item;
        }
    }
}
