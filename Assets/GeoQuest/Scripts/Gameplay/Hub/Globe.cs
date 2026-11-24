using UnityEngine;

public class Globe : ClickableItem
{
    [SerializeField] private GameObject _globeRoot;
    [SerializeField] private GameObject _hubScene;
    [SerializeField] private GameObject _globeBackground;
    [SerializeField] private GameObject _hubGrid;
    [SerializeField] private GameObject _hubCanvas;

    public override void OnClicked()
    {
        base.OnClicked();
        _globeRoot.SetActive(true);
        _hubScene.SetActive(false);
        _globeBackground.SetActive(true);
        _hubGrid.SetActive(false);
        _hubCanvas.SetActive(false);
    }
}
