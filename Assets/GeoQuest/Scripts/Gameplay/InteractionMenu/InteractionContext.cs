using GeoQuest;
using GeoQuest.Data;
using GeoQuest.Services;

namespace Assets.GeoQuest.Gameplay
{
    public class InteractionContext
    {
        public Interactable Target;
        public InventoryEntry UsedEntry;
        public Item UsedItem => UsedEntry?.Item;
    }
}
