using UnityEngine;
using UnityEngine.EventSystems;

namespace GeoQuest 
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private InteractionMenuController _menu;

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
                return;

            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 worldPos =
                _camera.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (!hit)
            {
                _menu.Hide();
                return;
            }

            if (hit.collider.TryGetComponent(out Interactable interactable))
            {
                _menu.Select(interactable);
            }
            else
            {
                _menu.Hide();
            }
        }
    }
}
