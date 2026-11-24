using System;
using System.Collections.Generic;
using UnityEngine;

namespace GeoQuest.Data
{
    [CreateAssetMenu(menuName = "GeoQuest/Action/Rule")]
    public class ActionRule : ScriptableObject
    {
        public ActionTag Action;
        public List<ConditionDefinition> Conditions;
        public float Time;
        public bool SpendsCharge;
        public List<EffectRule> Effects;
    }

    /*public enum ActionTagEnum 
    {
        Inspect,
        Pick,
        Talk,
        Use,
        Combine
    }*/
}
