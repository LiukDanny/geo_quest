using GeoQuest.Scripts.Data;
using UnityEngine;

namespace GeoQuest.Services
{
    public class InteractableObjectService
    {
        private LayerMask _itemsLayer;
        private HexGridView _gridView;
        private GridService _gridService;
        private HexGrid<PathNode> _grid;
        private SaveObjectData _save;

        public InteractableObjectService(LayerMask itemsLayer, HexGridView gridView, GridService gridService, SaveObjectData save)
        {
            _itemsLayer = itemsLayer;
            _gridView = gridView;
            _gridService = gridService;
            _save = save;
            
            _grid = _gridService.GetGrid();
        }

        public void Populate(LevelConfig location, Transform parent)
        {
            bool restore = _save.HasData;
            var objects = restore ? _save.Load() : location.objects;

            foreach (var obj in objects)
            {
                var item = location.interactionDatabase.FindItem(obj.itemId);
                if (item == null)
                    continue;

                var position = _grid.GetWorldPosition(obj.anchor.x, obj.anchor.y);
                if (restore)
                    Restore(item.ItemPrefab, position, parent);
                else
                    Spawn(item.ItemPrefab, position, parent);
            }
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Transform parent = null)
        {
            var obj = Create(prefab, position, parent);
            Register(obj);
            return obj;
        }

        public GameObject Restore(GameObject prefab, Vector3 position, Transform parent = null)
        {
            var obj = Create(prefab, position, parent);
            ToggleHexesUnderObstacle(obj.GetComponentInChildren<Collider2D>(), true);
            return obj;
        }

        public bool TrySpawnNearby(GameObject prefab, Interactable around, Transform parent)
        {
            var obstacle = around.GetComponent<Collider2D>();
            _grid.GetXY(obstacle.bounds.min, out int minX, out int minY);
            _grid.GetXY(obstacle.bounds.max, out int maxX, out int maxY);

            PathNode nearest = null;
            var nearestDistance = float.MaxValue;

            for (int x = minX - 2; x <= maxX + 2; x++)
            {
                for (int y = minY - 2; y <= maxY + 2; y++)
                {
                    var cell = _grid.GetGridObject(x, y);
                    if (cell == null || !cell.IsAvailable())
                        continue;

                    var center = _grid.GetWorldPosition(x, y);
                    var distance = Vector2.Distance(center, obstacle.ClosestPoint(center));
                    if (distance < nearestDistance)
                    {
                        nearest = cell;
                        nearestDistance = distance;
                    }
                }
            }

            if (nearest == null)
                return false;

            Spawn(prefab, _grid.GetWorldPosition(nearest.x, nearest.y), parent);
            return true;
        }

        private GameObject Create(GameObject prefab, Vector3 position, Transform parent) =>
            Object.Instantiate(prefab, _gridService.GetNearestPoint(position), Quaternion.identity, parent);

        private void Register(GameObject obj)
        {
            var col = obj.GetComponentInChildren<Collider2D>();
            var interactable = obj.GetComponentInChildren<Interactable>();
            ToggleHexesUnderObstacle(col, true);
            
            _grid.GetXY(obj.transform.position, out int x, out int y);
            var data = new GridObjectData
            {
                itemId = interactable.Item.Target.Id,
                anchor = new Vector2Int(x, y)
            };
            Debug.Log("Registered " + interactable.Item.Target.Id);
            _save.Add(data);
        }

        public void Unregister(GameObject obj)
        {
            var col = obj.GetComponentInChildren<Collider2D>();
            var interactable = obj.GetComponentInChildren<Interactable>();
            ToggleHexesUnderObstacle(col, false);
            
            _grid.GetXY(obj.transform.position, out int x, out int y);
            var data = new GridObjectData
            {
                itemId = interactable.Item.Target.Id,
                anchor = new Vector2Int(x, y)
            };
            
            Debug.Log("Unregistered " + interactable.Item.Target.Id);

            _save.Remove(data);
        }
        
        private void ToggleHexesUnderObstacle(Collider2D obstacle, bool isOccupied)
        {
            
            var bounds = obstacle.bounds;
            
            _grid.GetXY(bounds.min, out int minX, out int minY);
            _grid.GetXY(bounds.max, out int maxX, out int maxY);
        
            minX -= 1; minY -= 1;
            maxX += 1; maxY += 1;
        

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var cell = _grid.GetGridObject(x, y);
                    if (cell == null)
                        continue;

                    Vector2 hexCenter = _grid.GetWorldPosition(x, y);

                    if (Physics2D.OverlapPoint(hexCenter, _itemsLayer))
                    {
                        _grid.GetGridObject(x, y).SetIsOccupied(isOccupied);
                        _gridView.SetTileOccupied(x, y, isOccupied);
                    }
                }
            }
        }
    }
}
