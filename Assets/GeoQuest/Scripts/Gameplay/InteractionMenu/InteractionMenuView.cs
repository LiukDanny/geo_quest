using System;
using UnityEngine;

namespace GeoQuest
{
    public class InteractionMenuView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rect;

        public event Action<ActionType> ActionSelected;

        public void Show(Vector2 screenPos)
        {
            //_rect.position = screenPos;
            _rect.anchorMin = screenPos;
            _rect.anchorMax = screenPos;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Select(ActionType action)
        {
            ActionSelected?.Invoke(action);
        }

        public void SelectLook()
        {
            ActionSelected?.Invoke(ActionType.Look);
        }
    }
}
