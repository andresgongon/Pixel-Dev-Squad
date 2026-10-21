using System;
using UnityEngine;

namespace RioLimpio
{
    // Defines the four recycling bin categories based on Colombian Resolution 2184
    public enum BinCategory
    {
        BlueRecyclable = 0,     // Key 1: Plastics, PET bottles, clean glass and clean cardboard
        RedSpecial = 1,         // Key 2: Special or hazardous waste items like used batteries
        GreenOrganic = 2,       // Key 3: Organic biodegradable food scraps and fruit peels
        GreyNonRecyclable = 3   // Key 4: Non-recyclable or contaminated items like greasy pizza boxes
    }

    [Serializable]
    public class WasteItemData
    {
        public string itemId;
        public string displayName;
        public Sprite itemSprite;
        public BinCategory correctCategory;
        public bool isTrapItem;

        [TextArea(2, 3)]
        public string pedagogicalHint;

        // Constructor used to initialize default waste items in the catalog
        public WasteItemData(string id, string name, BinCategory category, bool trap, string hint)
        {
            itemId = id;
            displayName = name;
            correctCategory = category;
            isTrapItem = trap;
            pedagogicalHint = hint;
            itemSprite = null;
        }
    }
}
