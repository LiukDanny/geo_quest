using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScrollViewButtons : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button _leftBtn;
    [SerializeField] private Button _rightBtn;
    [SerializeField] private float scrollStep = 0.2f;
    [SerializeField] private float duration = 0.3f;

    private void Awake()
    {
        _leftBtn.onClick.AddListener(ScrollLeft);
        _rightBtn.onClick.AddListener(ScrollRight);
    }

    public void ScrollLeft()
    {
        float target = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition - scrollStep);
        DOTween.To(() => scrollRect.horizontalNormalizedPosition,
            x => scrollRect.horizontalNormalizedPosition = x,
            target,
            duration);
    }

    public void ScrollRight()
    {
        float target = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition + scrollStep);
        DOTween.To(() => scrollRect.horizontalNormalizedPosition,
            x => scrollRect.horizontalNormalizedPosition = x,
            target,
            duration);
    }

    private void OnDestroy()
    {
        _leftBtn.onClick.RemoveListener(ScrollLeft);
        _rightBtn.onClick.RemoveListener(ScrollRight);
    }
}