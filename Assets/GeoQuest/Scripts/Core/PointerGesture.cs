using UnityEngine;

public class PointerGesture
{
    private readonly float _thresholdSqr;
    private Vector2 _downPosition;

    public bool IsDragging { get; private set; }

    public PointerGesture(float threshold) => _thresholdSqr = threshold * threshold;

    public void Press(Vector2 position)
    {
        IsDragging = false;
        _downPosition = position;
    }

    public bool Move(Vector2 position)
    {
        if (IsDragging || (position - _downPosition).sqrMagnitude < _thresholdSqr)
            return false;

        IsDragging = true;
        return true;
    }

    public void Reset() => IsDragging = false;
}
