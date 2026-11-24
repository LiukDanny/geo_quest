using System.Linq;
using GeoQuest.Data;
using TMPro;
using UnityEngine;

public class InventoryInspectPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private TMP_Text _tags;
    
    public void Show(Item item)
    {
        gameObject.SetActive(true);
        _name.text = item.DisplayName;
        _description.text = item.Description;
        _tags.text = string.Join(", ", item.Tags.Select(t => t.DisplayName));
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
