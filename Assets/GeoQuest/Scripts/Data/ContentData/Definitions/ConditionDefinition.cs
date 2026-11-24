using Assets.GeoQuest.Gameplay;
using UnityEngine;

namespace GeoQuest.Data
{
    public abstract class ConditionDefinition : ScriptableObject
    {
        public abstract bool Evaluate(InteractionContext context);
    }
}
