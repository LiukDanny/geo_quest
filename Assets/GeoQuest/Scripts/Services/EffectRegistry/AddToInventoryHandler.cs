using Assets.GeoQuest.Gameplay;
using GeoQuest.Data;
using UnityEngine;

namespace GeoQuest.Services.EffectRegistry
{
    public class AddToInventoryHandler : IEffectHandler
    {
        private readonly InventoryService _inventory;
        private readonly InteractableObjectService _objects;

        public AddToInventoryHandler(InventoryService inventory, InteractableObjectService objects)
        {
            _inventory = inventory;
            _objects = objects;
        }

        public bool Execute(EffectRule effect, InteractionContext ctx)
        {
            var item = effect.Subject == EffectSubject.This ? ctx.Target.Item.Target : effect.Item;
            var count = effect.Subject == EffectSubject.This ? 1 : effect.Quantity;
            for (var i = 0; i < count; i++)
                _inventory.Add(item);

            if (effect.Subject == EffectSubject.This)
            {
                var obj = ctx.Target.transform.parent.gameObject;
                _objects.Unregister(obj);
                Object.Destroy(obj);
            }

            Debug.Log("Added to inventory: " + item.name);
            return true;
        }
    }
}
