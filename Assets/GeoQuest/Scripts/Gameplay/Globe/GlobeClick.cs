using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlobeClick : MonoBehaviour
{
    [SerializeField] private GlobePhaseController _phaseController;
    [SerializeField] private GlobeMovement _globeMovement;
    [SerializeField] private Material globeMaterial;
    [SerializeField] private Texture2D techMap;
    [SerializeField] private List<Continent> continents;

    private void Update()
    {
        if (!Input.GetMouseButtonUp(0))
            return;

        if (Utils.GetUIUnderPointer(Input.mousePosition) != null)
            return;

        if (_phaseController != null && _phaseController.CurrentPhase == GlobePhase.CountryMap)
            return;

        if (_globeMovement != null && _globeMovement.IsDragging)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        if (_phaseController != null && hit.collider.TryGetComponent(out CountryMark mark))
        {
            _phaseController.HandleCountryMarkTapped(mark);
            return;
        }

        Vector2 uv = hit.textureCoord;

        int x = Mathf.Clamp(Mathf.FloorToInt(uv.x * techMap.width), 0, techMap.width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(uv.y * techMap.height), 0, techMap.height - 1);

        Color pixelColor = techMap.GetPixel(x, y);

        if (pixelColor == Color.white)
            return;

        globeMaterial.SetColor("_SelectedCountryColor", pixelColor);

        Continent continent = continents.FirstOrDefault(c => c.colorId == pixelColor);
        if (continent == null)
            return;

        if (_phaseController != null)
            _phaseController.EnterCountriesPhase(continent, hit.point);
    }
}
