using System.Collections.Generic;
using GeoQuest.Scripts.Data;
using UnityEngine;

namespace GeoQuest.Services
{
    public class SaveObjectData
    {
        private readonly SaveDataManager _data;
        private readonly string _key;

        // public List<GridObjectData> Objects
        // {
        //     get => _data.Load<List<GridObjectData>>(SaveDataKeys.GridObjects);
        //     set => _data.Save(SaveDataKeys.GridObjects, value);
        // }

        public SaveObjectData(SaveDataManager dataManager, string key)
        {
            _data = dataManager;
            _key = key;
        }

        public bool HasData => _data.Has(_key);

        public List<GridObjectData> Load()
        {
            return _data.Load<List<GridObjectData>>(_key)
                   ?? new List<GridObjectData>();
        }

        public void Save(List<GridObjectData> list)
        {
            _data.Save(_key, list);
        }

        public void Add(GridObjectData obj)
        {
            var list = Load();
            list.Add(obj);
            Save(list);
        }

        public void Remove(GridObjectData obj)
        {
            var list = Load();
            list.RemoveAll(o => 
                o.itemId == obj.itemId &&
                o.anchor == obj.anchor
            );
            Save(list);
        }
    }
}
