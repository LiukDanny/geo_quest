using UnityEngine;
using UnityEngine.UI;

public class CountryMapLocation : MonoBehaviour
{
    private Color AvailableColor = Color.yellow;
    private Color UnavailableColor = Color.gray;
    [SerializeField] private CountryMapController _controller;
    [SerializeField] private Button _button;

    [SerializeField] private LevelConfig _level;
    [SerializeField] private MarkType type;
    [SerializeField] private MarkAvailability availability = MarkAvailability.Available;
    [SerializeField] private Image icon;

    [Header("Tail")]
    [SerializeField] private bool showTail = true;
    [SerializeField] private GameObject tailVisual;

    public LevelConfig Level => _level;
    public string LocationName => _level.displayName;
    public MarkType Type => type;
    public MarkAvailability Availability => availability;


    private void Awake()
    {
        if (icon != null)
            icon.color = availability == MarkAvailability.Available ? AvailableColor : UnavailableColor;

        if (tailVisual != null)
            tailVisual.SetActive(showTail);

        _button.onClick.AddListener(OnClicked);
    }

    private void OnClicked() => _controller.ShowLocationInfo(this);
}
