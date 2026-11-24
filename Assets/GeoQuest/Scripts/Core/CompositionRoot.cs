using System.Collections.Generic;
using System.Linq;
using GeoQuest;
using GeoQuest.Data;
using GeoQuest.Services;
using GeoQuest.Services.EffectRegistry;
using UnityEngine;

public class CompositionRoot : MonoBehaviour
{
    [SerializeField] private InteractionMenuController _interactionMenu;
    [SerializeField] private HexGridView _gridView;
    [SerializeField] private InventoryController _inventoryController;
    [SerializeField] private CharactersController _characters;
    //[SerializeField] private LevelConfig config;
    [SerializeField] private LayerMask _itemLayer;
    [SerializeField] private GameConfig _gameConfig;
    [SerializeField] private LevelConfig _fallbackLocation;


    [SerializeField] private Transform _bgContainer;
    [SerializeField] private CameraToBottomLeft _camera;

    private GridService _gridService;
    private InteractableObjectService _objectService;

    private InventoryService _inventoryService;
    private InteractionService _interactionService;
    private EffectHandlerRegistry _effectHandleRegistry;

    private void Awake()
    {
        var config = App.Session.Location ?? _fallbackLocation;
        var save = App.Save;
        var objectsSave = save.ObjectsFor(config.id);

        _gridService = new LocationLoader(_bgContainer, _camera, _gridView).Load(config);
        _characters.Init(_gridService, _gameConfig.Follow);

        _objectService = new InteractableObjectService(_itemLayer, _gridView, _gridService, objectsSave);
        _objectService.Populate(config, _bgContainer);

        var entries = save.Inventory.Load()
            .Select(d => (data: d, item: config.interactionDatabase.FindItem(d.Id)))
            .Where(x => x.item != null)
            .Select(x => new InventoryEntry { Item = x.item, Charges = x.data.Charges ?? x.item.Charge })
            .ToList();

        _inventoryService = new InventoryService(entries, save.Inventory);
        _effectHandleRegistry = new EffectHandlerRegistry(_inventoryService, _objectService, destroyCancellationToken);
        _interactionService = new InteractionService(_effectHandleRegistry, _gameConfig.ActionRanges, _characters.CharacterService, _inventoryService);

        _interactionMenu.Init(_interactionService);

        _inventoryController.Init(_inventoryService, _interactionService, _objectService);
    }
}
