using Assets.GeoQuest.Gameplay;
using GeoQuest.Data;
using UnityEngine;

namespace GeoQuest.Services.EffectRegistry
{
    public interface IEffectHandler
    {
        bool Execute(EffectRule effect, InteractionContext ctx);
    }
}
