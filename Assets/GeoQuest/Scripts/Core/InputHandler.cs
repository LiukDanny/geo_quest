using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GeoQuest 
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private CharactersController _characters;
        [SerializeField] private InteractionMenuController _menu;
        [SerializeField] private bool _isHub; //refactor
        [SerializeField] private float _dragThreshold = 10f;

        private PointerGesture _gesture;
        private bool _startedOverUi;

        private void Awake() => _gesture = new PointerGesture(_dragThreshold);

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _gesture.Press(Input.mousePosition);
                _startedOverUi = Utils.GetUIUnderPointer(Input.mousePosition) != null;
            }

            if (Input.GetMouseButton(0))
                _gesture.Move(Input.mousePosition);

            if (!Input.GetMouseButtonUp(0))
                return;

            if (_startedOverUi || _gesture.IsDragging)
                return;

            Vector2 worldPos =
                _camera.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);


            if (!hit)
            {
                _menu.Hide();
                return;
            }

            if (_isHub) 
            {
                if (hit.collider.TryGetComponent(out ClickableItem item))
                {
                    item.OnClicked();
                }
                else
                {
                    _characters.MoveActiveChar(hit.point).Forget();
                    _menu.Hide();
                }
                return;
            }

            if (hit.collider.TryGetComponent(out Interactable interactable))
            {
                _menu.Select(interactable);
            }
            else
            {
                _characters.MoveActiveChar(hit.point).Forget();
                _menu.Hide();
            }
        }

        
    }
}
