using Assets.GeoQuest.Gameplay;
using GeoQuest.Data;
using UnityEngine;
[CreateAssetMenu(menuName = "GeoQuest/Condition/HasTag")]
public class HasTagCondition : ConditionDefinition
{
    public ItemTag Tag;
    
    public override bool Evaluate(InteractionContext context)
    {
        return context.UsedItem != null && context.UsedItem.Tags.Contains(Tag);
    }
}
