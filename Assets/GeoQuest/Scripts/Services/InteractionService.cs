using Assets.GeoQuest.Gameplay;
using GeoQuest;
using GeoQuest.Data;
using GeoQuest.Services;
using GeoQuest.Services.EffectRegistry;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class InteractionService
{
    private readonly EffectHandlerRegistry _effectRegistry;
    private readonly ActionRangeConfig _rangeConfig;
    private readonly CharacterService _characterService;
    private readonly InventoryService _inventory;

    public InteractionService(EffectHandlerRegistry effectRegistry, ActionRangeConfig rangeConfig, CharacterService characterService, InventoryService inventory)
    {
        _effectRegistry = effectRegistry;
        _rangeConfig = rangeConfig;
        _characterService = characterService;
        _inventory = inventory;
    }

    public UniTask<ActionResult> Interact(Interactable target, ActionTag action, InventoryEntry usedEntry = null)
    {
        MovePositionHex character = _characterService.Active;
        return character.RunAction(ct => InteractInRange(character, target, usedEntry, action, ct));
    }

    private async UniTask<ActionResult> ExecuteRule(MovePositionHex character, Interactable item, InventoryEntry usedEntry, ActionTag action, CancellationToken ct)
    {
        Debug.Log($"{item.name} -> {action}");
        var context = new InteractionContext { Target = item, UsedEntry = usedEntry };
        var rules = item.Item.ActionRules.Where(t => t.Action == action).ToList();
        if (rules.Count == 0)
            return ActionResult.Completed;

        var currentActionRule = rules.FirstOrDefault(r => r.Conditions.All(c => c.Evaluate(context)));
        if (currentActionRule == null)
            return ActionResult.Failed;

        if (currentActionRule.Time > 0)
            await character.Act(currentActionRule.Time, ct);

        if (currentActionRule.SpendsCharge && !_inventory.TrySpend(usedEntry))
            return ActionResult.Failed;

        var failed = false;
        foreach (var effect in currentActionRule.Effects)
        {
            if (!await _effectRegistry.Execute(effect, context))
                failed = true;
        }

        return failed ? ActionResult.Failed : ActionResult.Completed;
    }

    private async UniTask<ActionResult> InteractInRange(MovePositionHex character, Interactable target, InventoryEntry usedEntry, ActionTag action, CancellationToken ct)
    {
        float range = _rangeConfig.GetRange(action);
        float distance = Vector3.Distance(character.transform.position, target.transform.position);

        if (distance > range)
        {
            Vector3 movePoint = Utils.GetPointInRange(character.transform.position, target.transform.position, range);
            await character.MoveTo(movePoint, ct);
        }

        return await ExecuteRule(character, target, usedEntry, action, ct);
    }
}
