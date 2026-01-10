using UnityEngine;

namespace GeoQuest
{
    public class InteractionMenuController : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private InteractionMenuView _view;

        private Interactable _current;

        void Awake()
        {
            _view.Hide();
            _view.ActionSelected += OnActionSelected;
        }

        public void Select(Interactable interactable)
        {
            if (!interactable.IsActive)
                return;

            _current = interactable;
            ShowMenu();
        }

        public void Hide()
        {
            _current = null;
            _view.Hide();
        }

        /*void LateUpdate()
        {
            if (_current != null)
                UpdatePosition();
        }*/

        private void ShowMenu()
        {
            Vector2 screenPos =
                _camera.WorldToViewportPoint(_current.MenuAnchor.position);

            _view.Show(screenPos);
        }

        private void OnActionSelected(ActionType action)
        {
            _current?.Interact(action);
            Hide();
        }
    }
}
