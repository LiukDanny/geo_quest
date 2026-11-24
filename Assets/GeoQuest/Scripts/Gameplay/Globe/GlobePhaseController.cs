using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobePhaseController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera _camera;
    [SerializeField] private GlobeMovement _globeMovement;
    [SerializeField] private Material _globeMaterial;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private Button _backButton;
    [SerializeField] private GameObject _hubScene;
    [SerializeField] private GameObject _globeBackground;
    [SerializeField] private GameObject _hubGrid;
    [SerializeField] private GameObject _hubCanvas;

    [Header("Rotation")]
    [SerializeField] private float _countriesRotationSpeed = 0.08f;

    [Header("Camera Zoom In (Continents -> Countries)")]
    [SerializeField] private float _zoomInOrthographicSize = 1.5f;
    [SerializeField] private float _zoomInDuration = 0.6f;

    [Header("Camera Zoom Out (Countries -> Continents)")]
    [SerializeField] private float _zoomOutDuration = 0.5f;

    [Header("Border")]
    [SerializeField] private float _countriesOutlineWidth = 1f;

    public GlobePhase CurrentPhase { get; private set; } = GlobePhase.Continents;

    public event Action<Continent> ContinentSelected;
    public event Action CountriesPhaseExited;
    public event Action<CountryMark> CountryMarkTapped;
    private Vector3 _restCameraPosition;
    private float _restOrthographicSize;
    private float _defaultRotationSpeed;
    private float _defaultOutlineWidth;
    private Continent _activeContinent;
    private CountryMapController _activeCountryMapScreen;
    private Action<LevelConfig> _selectLocation;

    public void Init(Action<LevelConfig> selectLocation) => _selectLocation = selectLocation;
    private int _zoomRequestId;

    private void Awake()
    {
        _restCameraPosition = _camera.transform.position;
        _restOrthographicSize = _camera.orthographicSize;
        _defaultRotationSpeed = _globeMovement.RotationSpeed;
        _defaultOutlineWidth = _globeMaterial.GetFloat("_OutlineWidth");

        if (_backButton != null)
            _backButton.onClick.AddListener(OnBackButtonPressed);

        if (_titleText != null)
            _titleText.transform.parent.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (_backButton != null)
            _backButton.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if (_backButton != null)
            _backButton.onClick.RemoveListener(OnBackButtonPressed);
    }

    public void EnterCountriesPhase(Continent continent, Vector3 focusWorldPoint)
    {
        if (continent == null || continent == _activeContinent)
            return;

        if (CurrentPhase == GlobePhase.Countries && _activeContinent != null)
            SetContinentUIActive(_activeContinent, false);

        CurrentPhase = GlobePhase.Countries;
        _activeContinent = continent;

        SetContinentUIActive(continent, true);

        if (_titleText != null)
        {
            _titleText.text = continent.name;
            _titleText.transform.parent.gameObject.SetActive(true);
        }

        _globeMovement.SetRotationSpeed(_countriesRotationSpeed);
        _globeMovement.SetInertiaEnabled(false);

        _globeMaterial.SetFloat("_OutlineWidth", _countriesOutlineWidth);

        Vector3 targetPosition = new(focusWorldPoint.x, focusWorldPoint.y, _restCameraPosition.z);
        AnimateCamera(targetPosition, _zoomInOrthographicSize, _zoomInDuration).Forget();

        ContinentSelected?.Invoke(continent);
    }

    public void OnBackButtonPressed()
    {
        if (CurrentPhase == GlobePhase.CountryMap)
        {
            ExitCountryMapPhase();
            return;
        }

        if (CurrentPhase == GlobePhase.Continents)
        {
            if (_backButton != null)
                _backButton.gameObject.SetActive(false);

            if (_hubScene != null)
                _hubScene.SetActive(true);

            if (_globeBackground != null)
                _globeBackground.SetActive(false);

            if (_hubGrid != null)
                _hubGrid.SetActive(true);

            if (_hubCanvas != null)
                _hubCanvas.SetActive(true);

            gameObject.SetActive(false);
            return;
        }

        CurrentPhase = GlobePhase.Continents;

        if (_activeContinent != null)
            SetContinentUIActive(_activeContinent, false);
        _activeContinent = null;

        if (_titleText != null)
        {
            _titleText.transform.parent.gameObject.SetActive(false);
        }

        _globeMovement.SetRotationSpeed(_defaultRotationSpeed);
        _globeMovement.SetInertiaEnabled(true);

        _globeMaterial.SetFloat("_OutlineWidth", _defaultOutlineWidth);

        AnimateCamera(_restCameraPosition, _restOrthographicSize, _zoomOutDuration).Forget();

        CountriesPhaseExited?.Invoke();
    }

    public void HandleCountryMarkTapped(CountryMark mark)
    {
        CountryMarkTapped?.Invoke(mark);

        if (CurrentPhase != GlobePhase.Countries || mark.CountryMapScreen == null)
            return;

        CurrentPhase = GlobePhase.CountryMap;
        _activeCountryMapScreen = mark.CountryMapScreen;
        _activeCountryMapScreen.Open(_selectLocation);

        if (_titleText != null)
            _titleText.text = mark.CountryName;

        _globeMovement.SetInputEnabled(false);
    }

    private void ExitCountryMapPhase()
    {
        CurrentPhase = GlobePhase.Countries;

        if (_activeCountryMapScreen != null)
            _activeCountryMapScreen.Close();
        _activeCountryMapScreen = null;

        if (_titleText != null && _activeContinent != null)
            _titleText.text = _activeContinent.name;

        _globeMovement.SetInputEnabled(true);
    }

    private static void SetContinentUIActive(Continent continent, bool active)
    {
        if (continent.resourceIconsLayer != null)
            continent.resourceIconsLayer.SetActive(active);
    }

    private async UniTaskVoid AnimateCamera(Vector3 targetPosition, float targetSize, float duration)
    {
        int requestId = ++_zoomRequestId;

        Vector3 startPosition = _camera.transform.position;
        float startSize = _camera.orthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (requestId != _zoomRequestId)
                return;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            _camera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            _camera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);

            await UniTask.Yield();
        }

        if (requestId != _zoomRequestId)
            return;

        _camera.transform.position = targetPosition;
        _camera.orthographicSize = targetSize;
    }
}
