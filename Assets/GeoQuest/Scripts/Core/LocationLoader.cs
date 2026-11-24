using UnityEngine;

public class LocationLoader
{
    private readonly Transform _container;
    private readonly CameraToBottomLeft _camera;
    private readonly HexGridView _gridView;

    public LocationLoader(Transform container, CameraToBottomLeft camera, HexGridView gridView)
    {
        _container = container;
        _camera = camera;
        _gridView = gridView;
    }

    public GridService Load(LevelConfig location)
    {
        var background = Object.Instantiate(location.background, _container);

        if (_camera != null)
            _camera.SetBounds(background.GetComponentInChildren<BoxCollider2D>());

        var grid = new GridService(location.gridConfig);
        _gridView.Init(location.gridConfig);
        return grid;
    }
}
