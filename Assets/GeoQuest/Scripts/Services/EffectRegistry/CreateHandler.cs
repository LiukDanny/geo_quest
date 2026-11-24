using Assets.GeoQuest.Gameplay;
using GeoQuest.Data;
using UnityEngine;

namespace GeoQuest.Services.EffectRegistry
{
    public class CreateHandler : IEffectHandler
    {
        private readonly InteractableObjectService _objects;

        public CreateHandler(InteractableObjectService objects) => _objects = objects;

        public bool Execute(EffectRule effect, InteractionContext ctx)
        {
            Transform root = ctx.Target.transform.parent;

            if (effect.Place == EffectPlace.OnItself)
            {
                _objects.Spawn(effect.Item.ItemPrefab, root.position, root.parent);
                return true;
            }

            var created = 0;
            for (var i = 0; i < effect.Quantity; i++)
                if (_objects.TrySpawnNearby(effect.Item.ItemPrefab, ctx.Target, root.parent))
                    created++;

            if (created < effect.Quantity)
                Debug.LogWarning($"Created {created} of {effect.Quantity} {effect.Item.name}: no free cell");
            return created > 0;
        }
    }
}