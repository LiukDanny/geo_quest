using UnityEngine;

public class GlobeMovement : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 0.2f;

    [SerializeField] private float maxRotationX = 0f;
    [SerializeField] private float maxRotationY = 40f;

    [Header("Inertia")]
    [SerializeField] private float inertiaMultiplier = 1.5f;
    [SerializeField] private float inertiaDamping = 6f;
    [SerializeField] private float minInertiaSpeed = 2f;
    [SerializeField] private float maxInertiaSpeed = 15f;
    [SerializeField] private float maxFrameScale = 3f;
    [Range(0f, 1f)]
    [SerializeField] private float velocitySmoothing = 0.35f;

    private const float ReferenceFrameTime = 1f / 60f;

    [Header("Drag")]
    [SerializeField] private float dragThreshold = 10f;

    [Header("Pivots")]
    [Tooltip("Child transform that receives yaw only. This transform receives pitch only.")]
    [SerializeField] private Transform _yawPivot;

    public bool IsDragging => _gesture.IsDragging;
    public float RotationSpeed => rotationSpeed;

    private float _rotationX;
    private float _rotationY;

    private bool _pointerDown;
    private PointerGesture _gesture;

    private Vector2 _lastPointerPosition;
    private Vector2 _velocity;

    private bool _inertiaEnabled = true;
    private bool _inputEnabled = true;

    private void Awake()
    {
        _gesture = new PointerGesture(dragThreshold);
        _rotationX = NormalizeAngle(transform.localEulerAngles.x);
        _rotationY = NormalizeAngle(_yawPivot.localEulerAngles.y);
    }

    private void Update()
    {
        if (!_inputEnabled)
            return;

#if UNITY_EDITOR || UNITY_STANDALONE
        UpdateMouse();
#else
        UpdateTouch();
#endif

        if (!_pointerDown)
            ApplyInertia();

        transform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
        _yawPivot.localRotation = Quaternion.Euler(0f, _rotationY, 0f);
    }

    private void UpdateMouse()
    {
        if (Input.GetMouseButtonDown(0))
            BeginPointer(Input.mousePosition);

        if (Input.GetMouseButton(0))
            MovePointer(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
            EndPointer();
    }

    private void UpdateTouch()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                BeginPointer(touch.position);
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                MovePointer(touch.position);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                EndPointer();
                break;
        }
    }

    private void BeginPointer(Vector2 position)
    {
        _pointerDown = true;
        _gesture.Press(position);

        _lastPointerPosition = position;

        _velocity = Vector2.zero;
    }

    private void MovePointer(Vector2 position)
    {
        if (!_pointerDown)
            return;

        if (!_gesture.IsDragging)
        {
            if (!_gesture.Move(position))
                return;

            _lastPointerPosition = position;
            return;
        }

        Vector2 delta = position - _lastPointerPosition;

        _lastPointerPosition = position;

        _rotationY += delta.x * rotationSpeed;

        if (maxRotationY > 0f && maxRotationY < 360f)
            _rotationY = Mathf.Clamp(_rotationY, -maxRotationY, maxRotationY);

        _rotationX -= delta.y * rotationSpeed;

        if (maxRotationX > 0f && maxRotationX < 360f)
            _rotationX = Mathf.Clamp(_rotationX, -maxRotationX, maxRotationX);

        float frameScale = Mathf.Min(ReferenceFrameTime / Mathf.Max(Time.deltaTime, 0.0001f), maxFrameScale);

        float speedX = Mathf.Sign(delta.x) * Mathf.Clamp(Mathf.Abs(delta.x) * frameScale * inertiaMultiplier, 0f, maxInertiaSpeed);
        float speedY = -Mathf.Sign(delta.y) * Mathf.Clamp(Mathf.Abs(delta.y) * frameScale * inertiaMultiplier, 0f, maxInertiaSpeed);

        _velocity = Vector2.Lerp(_velocity, new Vector2(speedX, speedY), velocitySmoothing);
    }

    private void EndPointer()
    {
        _pointerDown = false;
    }

    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    public void SetInertiaEnabled(bool isEnabled)
    {
        _inertiaEnabled = isEnabled;
    }

    public void SetInputEnabled(bool isEnabled)
    {
        _inputEnabled = isEnabled;

        if (!isEnabled)
        {
            _pointerDown = false;
            _gesture.Reset();
            _velocity = Vector2.zero;
        }
    }

    private void ApplyInertia()
    {
        if (!_inertiaEnabled)
        {
            _velocity = Vector2.zero;
            return;
        }

        if (_velocity.magnitude < minInertiaSpeed)
        {
            _velocity = Vector2.zero;
            return;
        }

        float deltaTimeScale = Time.deltaTime / ReferenceFrameTime;

        _rotationY += _velocity.x * rotationSpeed * deltaTimeScale;

        if (maxRotationY > 0f && maxRotationY < 360f)
            _rotationY = Mathf.Clamp(_rotationY, -maxRotationY, maxRotationY);

        _rotationX -= _velocity.y * rotationSpeed * deltaTimeScale;

        if (maxRotationX > 0f && maxRotationX < 360f)
            _rotationX = Mathf.Clamp(_rotationX, -maxRotationX, maxRotationX);

        _velocity = Vector2.MoveTowards(
            _velocity,
            Vector2.zero,
            inertiaDamping * Time.deltaTime);
    }

    private static float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}