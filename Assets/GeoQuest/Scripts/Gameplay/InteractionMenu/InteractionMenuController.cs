using Cysharp.Threading.Tasks;
using GeoQuest.Data;
using UnityEngine;

namespace GeoQuest
{
    public class InteractionMenuController : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private InteractionMenuView _view;

        private Interactable _current;
        private InteractionService _interactionService;

        public void Init(InteractionService interactionService)
        {
            _interactionService = interactionService;
        }

        private void Awake()
        {
            _view.Hide();
            _view.ActionSelected += OnActionSelected;
        }

        public void Select(Interactable interactable, bool showMenu = true)
        {
            if (!interactable.IsActive)
                return;

            _current = interactable;
            if(showMenu)
                ShowMenu();
        }

        public void Hide()
        {
            _current = null;
            _view.Hide();
        }

        private void ShowMenu()
        {
            Vector2 screenPos =
                _camera.WorldToViewportPoint(_current.MenuAnchor.position);

            _view.Show(screenPos, _current.ActionTags);
        }

        //TODO: Move to interaction service
        public async void OnActionSelected(ActionTag action)
        {
            if (_current != null)
            {
                await _interactionService.Interact(_current, action);
            }
            Hide();
        }
    }
}
