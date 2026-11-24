using UnityEngine;
using Newtonsoft.Json;

public class SaveDataManager
{
    public void Save<T>(string key, T value)
    {
        var wrapper = new DataWrapper<T> { Value = value };
        var json = JsonConvert.SerializeObject(wrapper);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
    }

    public T Load<T>(string key) where T : new()
    {
    
        if (!PlayerPrefs.HasKey(key))
        {
            return new();
        }
    
        var json = PlayerPrefs.GetString(key);
        var wrapper = JsonConvert.DeserializeObject<DataWrapper<T>>(json);
        return wrapper != null ? wrapper.Value : default;
    }

    public T Load<T>(string key, T defaultValue) where T : new()
    {
    
        if (PlayerPrefs.HasKey(key))
        {
            return Load<T>(key);
        }
    
        return defaultValue;
    }

    public bool Has(string key) => PlayerPrefs.HasKey(key);

    public void DeleteAll() => PlayerPrefs.DeleteAll();

    public void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);


     [System.Serializable]
    private class DataWrapper<T>
    {
        public T Value;
    }
}

