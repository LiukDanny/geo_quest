using System;
using System.Collections.Generic;
using GeoQuest.Data;
using GeoQuest.Services;
using UnityEngine;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    [SerializeField] private Button _toggleInventory;
    [SerializeField] private Canvas _canvas;

    [SerializeField] private GameObject _view;
    [SerializeField] private RectTransform _content;
    [SerializeField] private SlotItem _inventorySlotPrefab;
    [SerializeField] private ToggleGroup _toggleGroup;
    [SerializeField] private InventoryInspectPanel _inspectPanel;

    private readonly List<SlotItem> _slots = new();
    private Item _selectedItem;
    private SlotItem _draggedSlot;
    private int _dragStartIndex;
    private bool _isOpen;

    public event Action<InventoryEntry, Vector3> ItemDroppedOnWorld;

    private void Awake()
    {
        _toggleInventory.onClick.AddListener(Toggle);
    }

    public void AddItem(InventoryEntry entry)
    {
        var slot = Instantiate(_inventorySlotPrefab, _content);
        slot.Init(this, entry);
        slot.OnItemSelected += OnItemSelected;
        _slots.Add(slot);
    }

    public void RemoveItem(InventoryEntry entry)
    {
        var slot = _draggedSlot != null && _draggedSlot.Entry == entry
            ? _draggedSlot
            : _slots.Find(s => s.Entry == entry);

        if (slot == null)
            return;

        _slots.Remove(slot);
        Destroy(slot.gameObject);
    }

    public void Open()
    {
        if (!_isOpen)
            Toggle();
    }

    private void Toggle()
    {
        _isOpen = !_isOpen;
        _view.SetActive(_isOpen);
    }

    public void SetToggleGroup(Toggle toggle)
    {
        toggle.group = _toggleGroup;
    }

    private void OnItemSelected(Item item, bool isOn)
    {
        if (!_toggleGroup.AnyTogglesOn())
        {
            _inspectPanel.Hide();
            _selectedItem = null;
            return;
        }

        if (isOn)
        {
            _selectedItem = item;
            _inspectPanel.Show(item);
        }
    }

    public void BeginDrag(SlotItem slot, Vector2 position)
    {
        _draggedSlot = slot;
        _dragStartIndex = _slots.IndexOf(slot);
        slot.transform.SetParent(transform, true);
        if (_selectedItem == slot.Item)
            slot.Deselect();
    }

    public void OnDrag(SlotItem slot, Vector2 deltaPos)
    {
        var rect = slot.transform as RectTransform;
        rect.anchoredPosition += deltaPos / _canvas.scaleFactor;
    }

    public void OnEndDrag(SlotItem slot, Vector2 position)
    {
        var uiObject = Utils.GetUIUnderPointer(position);

        if (uiObject == null)
        {
            ItemDroppedOnWorld?.Invoke(slot.Entry, _canvas.worldCamera.ScreenToWorldPoint(position));

            bool keptInInventory = _slots.Contains(slot);
            if (keptInInventory)
                MoveSlot(slot, _dragStartIndex);
        }
        else
        {
            bool onBelt = uiObject.CompareTag("InventoryPanel") || uiObject.CompareTag("Slot");
            MoveSlot(slot, onBelt ? GetInsertIndex(position) : _dragStartIndex);
        }

        _draggedSlot = null;
    }

    private void MoveSlot(SlotItem slot, int index)
    {
        _slots.Remove(slot);
        _slots.Insert(index, slot);
        slot.transform.SetParent(_content, false);
        slot.transform.SetSiblingIndex(index);
    }

    private int GetInsertIndex(Vector2 dropPosition)
    {
        int index = _slots.Count;

        for (int i = 0; i < _slots.Count; i++)
        {
            var rect = _slots[i].GetComponent<RectTransform>();
            var itemPos = RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, rect.position);

            if (dropPosition.x < itemPos.x)
            {
                index = i;
                break;
            }
        }

        return index;
    }
}
