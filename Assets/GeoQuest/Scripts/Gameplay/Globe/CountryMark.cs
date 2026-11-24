using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CountryMark : MonoBehaviour
{
    private static readonly Color AvailableColor = Color.yellow;
    private static readonly Color UnavailableColor = Color.gray;

    [SerializeField] private string countryName;
    [SerializeField] private MarkType type;
    [SerializeField] private MarkAvailability availability = MarkAvailability.Available;
    [SerializeField] private MarkVisiblePhases visiblePhases = MarkVisiblePhases.Both;
    [SerializeField] private float iconScale = 1f;
    [SerializeField] private CountryMapController countryMapScreen;

    [Header("Tail")]
    [SerializeField] private bool showTail = true;
    [SerializeField] private GameObject tailVisual;

    [Header("Visibility")]
    [Tooltip("How far from the globe's silhouette edge the mark hides itself, in dot-product units (0 = right at the edge, higher = hides earlier).")]
    [SerializeField] private float edgeOffset = 0.1f;

    public string CountryName => countryName;
    public MarkType Type => type;
    public MarkAvailability Availability => availability;
    public CountryMapController CountryMapScreen => countryMapScreen;

    private Collider _collider;
    private SpriteRenderer _iconRenderer;
    private Transform _globeCenter;
    private GlobePhaseController _phaseController;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _iconRenderer = GetComponent<SpriteRenderer>();

        var globeMovement = GetComponentInParent<GlobeMovement>();
        if (globeMovement != null)
            _globeCenter = globeMovement.transform;

        _phaseController = GetComponentInParent<GlobePhaseController>();

        transform.localScale = Vector3.one * iconScale;

        if (_iconRenderer != null)
            _iconRenderer.color = availability == MarkAvailability.Available ? AvailableColor : UnavailableColor;
    }

    private void LateUpdate()
    {
        bool isFrontFacing = true;

        if (_globeCenter != null)
        {
            Vector3 outwardNormal = (transform.position - _globeCenter.position).normalized;
            transform.rotation = Quaternion.LookRotation(outwardNormal, Vector3.up);
            isFrontFacing = outwardNormal.z < -edgeOffset;
        }

        bool visible = IsPhaseVisible() && isFrontFacing;

        if (_iconRenderer != null)
            _iconRenderer.enabled = visible;

        _collider.enabled = visible;

        if (tailVisual != null)
            tailVisual.SetActive(visible && showTail);
    }

    private bool IsPhaseVisible()
    {
        if (_phaseController == null)
            return true;

        MarkVisiblePhases currentPhase = _phaseController.CurrentPhase switch
        {
            GlobePhase.Countries => MarkVisiblePhases.Countries,
            GlobePhase.Continents => MarkVisiblePhases.Continents,
            _ => 0
        };

        return (visiblePhases & currentPhase) != 0;
    }
}
