using System;
using GeoQuest.Data;
using GeoQuest.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private Toggle _toggle;
    [SerializeField] private GameObject _checkmark;
    private InventoryView _view;

    public InventoryEntry Entry { get; private set; }
    public Item Item => Entry.Item;

    public event Action<Item, bool> OnItemSelected;

    private void OnEnable()
    {
        _toggle.onValueChanged.AddListener(SetCheckmark);
    }

    private void OnDisable()
    {
        _toggle.onValueChanged.RemoveListener(SetCheckmark);
    }

    private void SetCheckmark(bool isOn)
    {
        _checkmark.SetActive(isOn);
        OnItemSelected?.Invoke(Item, isOn);
    }
    public void Init(InventoryView view, InventoryEntry entry)
    {
        _view = view;
        Entry = entry;
        _icon.sprite = entry.Item.Icon;
        _view.SetToggleGroup(_toggle);
    }

    public void Deselect() => _toggle.isOn = false;

    public void OnBeginDrag(PointerEventData e)
    {
        _icon.raycastTarget = false;
        _view.BeginDrag(this, e.position);
    }

    public void OnDrag(PointerEventData e) => _view.OnDrag(this, e.delta);

    public void OnEndDrag(PointerEventData e)
    {
        _view.OnEndDrag(this, e.position);
        _icon.raycastTarget = true;
    }
}
