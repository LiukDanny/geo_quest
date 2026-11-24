using GeoQuest.Data;
using System.Collections.Generic;
using UnityEngine;

namespace GeoQuest
{
    public class Interactable : MonoBehaviour
    {
        [SerializeField] private bool _isActive = true;
        [SerializeField] private Transform _menuAnchor;
        [SerializeField] private InteractionRule _item;

        [HideInInspector] public List<ActionTag> ActionTags;
        public InteractionRule Item => _item;

        public bool IsActive => _isActive;
        public Transform MenuAnchor => _menuAnchor;

        private void Start()
        {
            ActionTags = new List<ActionTag>();
            foreach (var actionRule in _item.ActionRules) 
            {
                ActionTags.Add(actionRule.Action);
            }
        }
    }
}
