using Assets.GeoQuest.Gameplay;
using GeoQuest.Data;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GeoQuest.Services.EffectRegistry
{
    public class EffectHandlerRegistry
    {
        private readonly Dictionary<EffectTagEnum, IEffectHandler> _handlers = new();
        private readonly CancellationToken _lifetime;

        public EffectHandlerRegistry(InventoryService inventory, InteractableObjectService objects, CancellationToken lifetime)
        {
            _lifetime = lifetime;
            _handlers = new Dictionary<EffectTagEnum, IEffectHandler>
            {
                { EffectTagEnum.AddToInventory, new AddToInventoryHandler(inventory, objects) },
                { EffectTagEnum.Create, new CreateHandler(objects) },
                { EffectTagEnum.Destroys , new DestroyHandler(objects, inventory) },
            };
        }

        public async UniTask<bool> Execute(EffectRule effect, InteractionContext ctx)
        {
            if (Random.Range(0, 100) >= effect.Chance)
                return false;

            if (effect.Time > 0)
            {
                ExecuteDelayed(effect, ctx).Forget();
                return true;
            }

            return Run(effect, ctx);
        }

        private async UniTaskVoid ExecuteDelayed(EffectRule effect, InteractionContext ctx)
        {
            // TODO: pending delayed effects are not saved (lost on leaving the location, app close)
            // TODO: no UI/animation hook yet to show that an effect is pending
            Debug.Log($"{effect.EffectTag} effect in {effect.Time}s");
            await UniTask.Delay(System.TimeSpan.FromSeconds(effect.Time), cancellationToken: _lifetime);

            if (ctx.Target == null)
            {
                Debug.LogWarning($"{effect.EffectTag} effect skipped: the target is gone");
                return;
            }

            Run(effect, ctx);
        }

        private bool Run(EffectRule effect, InteractionContext ctx)
        {
            if (_handlers.TryGetValue(effect.EffectTag, out var handler))
            {
                return handler.Execute(effect, ctx);
            }
            else
            {
                Debug.LogError("Handler is not implemented for EffectTag: " + effect.EffectTag);
                return false;
            }
        }

    }
}
