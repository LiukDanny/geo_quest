using Assets.GeoQuest.Gameplay;
using GeoQuest.Data;
using UnityEngine;

namespace GeoQuest.Services.EffectRegistry
{
    public class DestroyHandler : IEffectHandler
    {
        private readonly InteractableObjectService _objects;
        private readonly InventoryService _inventory;

        public DestroyHandler(InteractableObjectService objects, InventoryService inventory)
        {
            _objects = objects;
            _inventory = inventory;
        }

        public bool Execute(EffectRule effect, InteractionContext ctx)
        {
            if (effect.Subject == EffectSubject.This && ctx.UsedEntry != null)
            {
                if (effect.OnlyWhenOutOfCharges && ctx.UsedEntry.Charges != 0)
                    return true;

                _inventory.Remove(ctx.UsedEntry);
            }
            else
            {
                var obj = ctx.Target.transform.parent.gameObject;
                _objects.Unregister(obj);
                Object.Destroy(obj);
            }
            Debug.Log("Destroys: " + ctx.Target.name);
            return true;
        }
    }
}
