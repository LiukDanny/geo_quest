using UnityEngine;

namespace GeoQuest
{
    public enum ActionType 
    {
        Look,
        Talk,
        Take,
    }

    public class Interactable : MonoBehaviour
    {
        [SerializeField] private bool _isActive = true;
        [SerializeField] private Transform _menuAnchor;

        public bool IsActive => _isActive;
        public Transform MenuAnchor => _menuAnchor;

        public void Interact(ActionType action)
        {
            Debug.Log($"{name} -> {action}");
        }
    }
}
