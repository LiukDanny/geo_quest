using Cysharp.Threading.Tasks;
using GeoQuest;
using GeoQuest.Data;
using GeoQuest.Services;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private InventoryView _view;
    [SerializeField] private ActionTag _combineTag; //remove and change

    private InventoryService _inventory;
    private InteractionService _interaction;
    private InteractableObjectService _objects;

    public void Init(InventoryService inventory, InteractionService interaction, InteractableObjectService objects)
    {
        _inventory = inventory;
        _interaction = interaction;
        _objects = objects;

        foreach (var entry in _inventory.Entries)
            _view.AddItem(entry);

        _inventory.EntryAdded += OnEntryAdded;
        _inventory.EntryRemoved += _view.RemoveItem;
        _view.ItemDroppedOnWorld += OnItemDroppedOnWorld;
    }

    private void OnEntryAdded(InventoryEntry entry)
    {
        _view.AddItem(entry);
        _view.Open();
    }

    private void OnItemDroppedOnWorld(InventoryEntry entry, Vector3 worldPoint)
    {
        var hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hit && hit.collider.TryGetComponent(out Interactable target))
        {
            _interaction.Interact(target, _combineTag, entry).Forget();
        }
        else
        {
            _inventory.Remove(entry);
            _objects.Spawn(entry.Item.ItemPrefab, worldPoint);
        }
    }
}
