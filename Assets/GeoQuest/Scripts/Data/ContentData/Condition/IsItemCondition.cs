using Assets.GeoQuest.Gameplay;
using UnityEngine;

namespace GeoQuest.Data
{
    [CreateAssetMenu(menuName = "GeoQuest/Condition/IsItem")]
    public class IsItemCondition : ConditionDefinition
    {
        public Item Item;

        public override bool Evaluate(InteractionContext context) => context.UsedItem == Item;
    }
}
