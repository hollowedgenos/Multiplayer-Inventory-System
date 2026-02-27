using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Scriptable Objects/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [Header("Item Rarity Drop Rates")]
    //using 0 to 100 ints to prevent floating point issues
    [Range(0, 100)] public int CommonWeight;
    [Range(0, 100)] public int RareWeight;
    [Range(0, 100)] public int LegendaryWeight;

    [Header("All Items")]
    public List<ItemData> allItems;

    [Header("Unlocked Items")]
    public List<ItemData> unlockedItems;

    private void OnValidate()
    {
        // Clamp valueA to total
        CommonWeight = Mathf.Clamp(CommonWeight, 0, 100);

        // Clamp RareWeight to remaining amount
        var remaining = 100 - CommonWeight;
        RareWeight = Mathf.Clamp(RareWeight, 0, remaining);

        // Clamp LegendaryWeight to remaining amount
        remaining = remaining - RareWeight;
        LegendaryWeight = Mathf.Clamp(LegendaryWeight, 0, remaining);
    }
}
