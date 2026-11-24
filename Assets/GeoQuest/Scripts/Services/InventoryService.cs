using GeoQuest.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GeoQuest.Services
{
    public class InventoryService
    {
        private readonly List<InventoryEntry> _entries;
        private readonly SaveInventory _save;

        public event Action<InventoryEntry> EntryAdded;
        public event Action<InventoryEntry> EntryRemoved;

        public List<InventoryEntry> Entries => _entries;

        public InventoryService(List<InventoryEntry> entries, SaveInventory save)
        {
            _entries = entries;
            _save = save;
        }

        public InventoryEntry Add(Item item)
        {
            if (item == null)
                return null;

            var entry = new InventoryEntry { Item = item, Charges = item.Charge };
            _entries.Add(entry);
            Save();
            EntryAdded?.Invoke(entry);
            return entry;
        }

        public void Remove(InventoryEntry entry)
        {
            if (_entries.Remove(entry))
            {
                Save();
                EntryRemoved?.Invoke(entry);
            }

        }

        public bool Contains(Item item)
        {
            return _entries.Exists(e => e.Item == item);
        }

        public bool TrySpend(InventoryEntry entry)
        {
            if (entry == null || entry.Charges == 0)
                return false;

            if (entry.Charges > 0)
            {
                entry.Charges--;
                Save();
                Debug.Log($"Spent a charge of {entry.Item.name}, left {entry.Charges}");
            }

            return true;
        }

        private void Save()
        {
            _save.Save(_entries
                .Select(e => new InventoryEntryData { Id = e.Item.Id, Charges = e.Charges })
                .ToList());
        }
    }
}
