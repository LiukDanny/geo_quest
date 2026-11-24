
namespace GeoQuest.Services
{
    public class SaveService
    {
        private readonly SaveDataManager _data = new();

        public SaveInventory Inventory { get; }

        public SaveService()
        {
            Inventory = new SaveInventory(_data);
        }

        public SaveObjectData ObjectsFor(string locationId) =>
            new SaveObjectData(_data, $"{SaveDataKeys.GridObjects}/{locationId}");

        public void Save<T>(string key, T value) => _data.Save<T>(key, value);

        public T Load<T>(string key, T defaultValue) where T : new() => _data.Load(key, defaultValue);

        public void DeleteAll() => _data.DeleteAll();

        public void DeleteKey(string key) => _data.DeleteKey(key);
    }
}
