using System.Collections.Generic;
using UnityEngine;

namespace GeoQuest.Data 
{
    [CreateAssetMenu(menuName = "GeoQuest/Item/Object")]
    public class Item : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        [TextArea]
        public string Description;
        public Sprite Icon;
        public GameObject ItemPrefab;
        public List<ItemTag> Tags;
        public int Charge = -1;
    }
}
