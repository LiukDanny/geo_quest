using GeoQuest.Services;
using GeoQuest.Services.EffectRegistry;
using UnityEngine;

public class HubBootstrap : MonoBehaviour
{
    //[SerializeField] private InteractionMenuController _interactionMenu;
    [SerializeField] private HexGridView _gridView;
    //[SerializeField] private InventoryController _inventoryController;
    [SerializeField] private LevelConfig config;
    //[SerializeField] private LayerMask _itemLayer;


    [SerializeField] private Transform _bgContainer;
    [SerializeField] private CameraToBottomLeft _camera;
    [SerializeField] private CharactersController _characters;
    [SerializeField] private GlobePhaseController _globe;
    [SerializeField] private GameConfig _gameConfig;

    private GridService _gridService;
    private InteractableObjectService _objectService;

    private InventoryService _inventoryService;
    private InteractionService _interactionService;
    private EffectHandlerRegistry _effectHandleRegistry;

    private void Awake()
    {
        _gridService = new LocationLoader(_bgContainer, _camera, _gridView).Load(config);
        _characters.Init(_gridService, _gameConfig.Follow);
        _globe.Init(App.Session.SelectLocation);



        //_objectService = new InteractableObjectService(_itemLayer, _gridView, _gridService);

       /* //TODO: Change based on has data or not
        var objects = SaveData.Objects.Load();
        if (objects.Count == 0)
        {
            Debug.Log(config.interactionDatabase);
            foreach (var item in config.objects)
            {
                var rule = config.interactionDatabase.Rules.FirstOrDefault(rule => rule.Target.Id == item.itemId);
                var prefab = rule.Target.ItemPrefab;
                var obj = Instantiate(prefab, _gridService.GetGrid().GetWorldPosition(item.anchor.x, item.anchor.y), Quaternion.identity, _bgContainer);
                _objectService.Register(obj);
            }
        }
        else
        {
            foreach (var item in objects)
            {
                var rule = config.interactionDatabase.Rules.FirstOrDefault(rule => rule.Target.Id == item.itemId);
                var prefab = rule.Target.ItemPrefab;
                var obj = Instantiate(prefab, _gridService.GetGrid().GetWorldPosition(item.anchor.x, item.anchor.y), Quaternion.identity, _bgContainer);
                var col = obj.GetComponentInChildren<Collider2D>();
                _objectService.ToggleHexesUnderObstacle(col, true);
            }
        }


        //Content
        _effectHandleRegistry = new EffectHandlerRegistry();

        //refactor
        List<string> inv = SaveData.Inventory.Load();
        List<Item> items = config.interactionDatabase.Rules
            .Where(r => inv.Contains(r.Target.Id))
            .Select(r => r.Target)
            .ToList();

        _inventoryService = new InventoryService(items);
        _interactionService = new InteractionService(_effectHandleRegistry, _inventoryService, _objectService);

        _interactionMenu.Init(_interactionService);

        _inventoryController.Init(_inventoryService, _gridService, _interactionService, _objectService);*/
    }
}
