using System.Collections.Generic;
using UnityEngine;

namespace GeoQuest.Data 
{
    [CreateAssetMenu(menuName = "GeoQuest/Rule")]
    public class InteractionRule : ScriptableObject
    {
        public Item Target;

        public List<ActionRule> ActionRules;
    }
}
