using System.Collections.Generic;
using System.Linq;
using GeoQuest.Scripts.Data;
using UnityEngine;

namespace GeoQuest.Services
{
    public class InventoryEntryData
    {
        public string Id;
        public int? Charges;
    }

    public class SaveInventory
    {
        private readonly SaveDataManager _data;


        public SaveInventory(SaveDataManager dataManager) => _data = dataManager;

        public List<InventoryEntryData> Load()
        {
            if (_data.Has(SaveDataKeys.InventoryEntries))
                return _data.Load<List<InventoryEntryData>>(SaveDataKeys.InventoryEntries) ?? new List<InventoryEntryData>();

            return (_data.Load<List<string>>(SaveDataKeys.Inventory) ?? new List<string>())
                .Select(id => new InventoryEntryData { Id = id })
                .ToList();
        }

        public void Save(List<InventoryEntryData> list)
        {
            _data.Save(SaveDataKeys.InventoryEntries, list);
        }
    }
}
