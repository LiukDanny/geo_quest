using System.Runtime.CompilerServices;
using UnityEngine;

public class CameraToBottomLeft : MonoBehaviour
{
    [SerializeField] private GameObject _scene;
    [SerializeField] private Transform[] _characters;
    [SerializeField] private float dragSpeed = 1f;
    [SerializeField] private float maxDistanceFromCharacter = 50f;

    private BoxCollider2D _bounds;

    private Camera cam;
    private Vector3 _lastPointerPosition;
    private bool _startedOverUi;

    void Start()
    {
        
        cam = Camera.main;

        float screenHeight = cam.orthographicSize * 2f;
        float screenWidth = screenHeight * cam.aspect;

        cam.transform.position = new Vector3(
            screenWidth / 2f,
            cam.transform.position.y,
            cam.transform.position.z
        );

        /*_scene.transform.position = new Vector3(
            screenWidth / 2f,
            screenHeight / 2f,
            _scene.transform.position.z
        );*/
    }

    private void Update()
    {
        HandleDrag();
    }

    private void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _lastPointerPosition = Input.mousePosition;
            _startedOverUi = Utils.GetUIUnderPointer(Input.mousePosition) != null;
        }

        if (Input.GetMouseButton(0) && !_startedOverUi)
        {
            Vector3 delta = Input.mousePosition - _lastPointerPosition;

            Vector3 move = new Vector3(
                -delta.x * dragSpeed * Time.deltaTime,
                0,
                -delta.y * dragSpeed * Time.deltaTime);

            cam.transform.position += move;

            ClampToNearestCharacter();

            _lastPointerPosition = Input.mousePosition;
        }
    }

    private void ClampToNearestCharacter()
    {
        Transform nearest = GetNearestCharacter();

        Vector3 camPos = cam.transform.position;
        Vector3 charPos = nearest.position;

        Vector2 cam2D = new(camPos.x, camPos.z);
        Vector2 char2D = new(charPos.x, charPos.z);

        float distance = Vector2.Distance(cam2D, char2D);

        if (distance > maxDistanceFromCharacter)
        {
            Vector2 dir = (cam2D - char2D).normalized;
            cam2D = char2D + dir * maxDistanceFromCharacter;
        }

        float x = cam2D.x;
        float z = cam2D.y;

        if (_bounds != null)
        {
            Bounds bounds = _bounds.bounds;
            x = ClampAxis(x, bounds.min.x, bounds.max.x, cam.orthographicSize * cam.aspect);
            z = ClampAxis(z, bounds.min.z, bounds.max.z, cam.orthographicSize);
        }

        cam.transform.position = new Vector3(x, camPos.y, z);
    }

    private static float ClampAxis(float value, float min, float max, float halfExtent)
    {
        float clampedMin = min + halfExtent;
        float clampedMax = max - halfExtent;
        return clampedMin <= clampedMax ? Mathf.Clamp(value, clampedMin, clampedMax) : value;
    }

    private Transform _pendingCenterTarget;

    public void CenterOnCharacter(Transform character)
    {
        _pendingCenterTarget = character;
    }

    public void SetBounds(BoxCollider2D bounds)
    {
        _bounds = bounds;
    }

    private void LateUpdate()
    {
        if (_pendingCenterTarget == null || cam == null)
            return;

        Vector3 camPos = cam.transform.position;
        float targetX = _pendingCenterTarget.position.x;

        if (_bounds != null)
            targetX = ClampAxis(targetX, _bounds.bounds.min.x, _bounds.bounds.max.x, cam.orthographicSize * cam.aspect);

        cam.transform.position = new Vector3(
            targetX,
            camPos.y,
            camPos.z);

        _pendingCenterTarget = null;
    }

    private Transform GetNearestCharacter()
    {
        Transform nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Transform character in _characters)
        {
            float distance = Vector3.Distance(
                cam.transform.position,
                character.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = character;
            }
        }

        return nearest;
    }
}