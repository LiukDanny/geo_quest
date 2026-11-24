using GeoQuest.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GeoQuest
{
    public class InteractionMenuView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rect;
        [SerializeField] private List<ActionButton> _actionButtons;

        public event Action<ActionTag> ActionSelected;

        private void Awake()
        {
            foreach (ActionButton button in _actionButtons) 
            {
                button.onClick.AddListener(() => Select(button.ActionTag));
            }
        }

        public void Show(Vector2 screenPos, List<ActionTag> tags)
        {
            //_rect.position = screenPos;
            _rect.anchorMin = screenPos;
            _rect.anchorMax = screenPos;

            _actionButtons.ForEach(b =>
                b.interactable = tags.Contains(b.ActionTag)
            );
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            foreach (ActionButton actionButton in _actionButtons) 
            {
                actionButton.interactable = false;
            }
            gameObject.SetActive(false);
        }

        public void Select(ActionTag action)
        {
            ActionSelected?.Invoke(action);
        }

        private void OnDestroy()
        {
            foreach (ActionButton button in _actionButtons)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }
}
