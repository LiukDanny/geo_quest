using UnityEngine;

namespace GeoQuest.Data
{
    [CreateAssetMenu(menuName = "GeoQuest/Effect/Rule")]
    public class EffectRule : ScriptableObject
    {
        //public EffectTag EffectTag;
        public EffectTagEnum EffectTag;
        public EffectSubject Subject;
        public EffectPlace Place;
        public bool OnlyWhenOutOfCharges;
        public Item Item;
        public float Time;
        public int Quantity = 1;
        public int Chance = 100;
    }

    public enum EffectTagEnum 
    {
        Create,
        AddToInventory,
        Destroys
    }

    public enum EffectSubject
    {
        This,
        Other
    }

    public enum EffectPlace
    {
        Nearby,
        OnItself
    }
}