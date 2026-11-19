using System;
using System.Collections.Generic;
using UnityEngine;

namespace RioLimpio
{
    // Recycling bin categories defined by Colombian Resolution 2184 of 2019
    public enum BinCategory
    {
        WhiteRecyclable = 0,    // Key 1: Clean plastic, cardboard, paper, glass, and metals
        GreenOrganic = 1,       // Key 2: Biodegradable organic matter and fruit peels
        BlackNonRecyclable = 2, // Key 3: Non-recyclables, dirty napkins, greasy food wrappers
        RedHazardous = 3        // Key 4: Hazardous and special posconsumo waste (batteries, bulbs, medicine, small RAEE, aerosols)
    }

    // Serializable data model for JSON catalog loading
    [Serializable]
    public class WasteItemConfig
    {
        public string itemId;
        public string displayName;
        public string spriteName;
        public int categoryIndex;
        public bool isTrapItem;
        public string pedagogicalHint;
    }

    [Serializable]
    public class WasteCatalogData
    {
        public List<WasteItemConfig> items;
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

        // Constructor used when initializing items from code or JSON data
        public WasteItemData(string id, string name, BinCategory category, bool trap, string hint, Sprite sprite = null)
        {
            itemId = id;
            displayName = name;
            correctCategory = category;
            isTrapItem = trap;
            pedagogicalHint = hint;
            itemSprite = sprite;
        }
    }
}
