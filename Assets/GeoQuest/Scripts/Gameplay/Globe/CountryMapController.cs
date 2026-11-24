using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CountryMapController : MonoBehaviour
{
    [SerializeField] private GameObject _infoPopup;
    [SerializeField] private TMP_Text _infoTitleText;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _backdropButton;
    [SerializeField] private Button _travelButton;

    private CountryMapLocation _selectedLocation;
    private Action<LevelConfig> _selectLocation;

    public void Open(Action<LevelConfig> selectLocation)
    {
        _selectLocation = selectLocation;
        gameObject.SetActive(true);
    }

    public void Close() => gameObject.SetActive(false);

    private void Awake()
    {
        _closeButton.onClick.AddListener(HidePopup);
        _backdropButton.onClick.AddListener(HidePopup);
        _travelButton.onClick.AddListener(OnTravelClicked);

        HidePopup();
    }

    private void OnDestroy()
    {
        _closeButton.onClick.RemoveListener(HidePopup);
        _backdropButton.onClick.RemoveListener(HidePopup);
        _travelButton.onClick.RemoveListener(OnTravelClicked);
    }

    public void ShowLocationInfo(CountryMapLocation location)
    {
        _selectedLocation = location;

        _infoTitleText.text = location.LocationName;
        _travelButton.interactable = location.Availability == MarkAvailability.Available;
        _backdropButton.gameObject.SetActive(true);
        _infoPopup.SetActive(true);
    }

    private void HidePopup()
    {
        _selectedLocation = null;
        _backdropButton.gameObject.SetActive(false);
        _infoPopup.SetActive(false);
    }

    private void OnTravelClicked()
    {
        _selectLocation(_selectedLocation.Level);
        SceneManager.LoadScene(SceneNames.Game);
    }
}
