using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ItemEffect
{
    public enum EFFECT_VALUE { None, Integer, String }

    public EFFECT_VALUE Type;
    public int IntValue;
    public string StringValue;
    public string EffectName;

    // Constructor for integer values
    public ItemEffect(string name, int value)
    {
        Type = EFFECT_VALUE.Integer;
        IntValue = value;
        EffectName = name;
        StringValue = null; // Ensure string is null when storing an int
    }

    // Constructor for string values
    public ItemEffect(string name, string value)
    {
        Type = EFFECT_VALUE.String;
        StringValue = value;
        EffectName = name;
        IntValue = 0; // Ensure int is default when storing a string
    }
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public enum ITEM_RARITY { Uncommon, Rare, Legendary}

    [Header("Identification")]
    public string itemID;
    public string itemName;
    public ITEM_RARITY rarity;

    [Header("Item Effects")]
    public List<ItemEffect>
    effects = new List<ItemEffect>()
    {

    };

    [Header("Prefab Reference")]
    public GameObject itemPrefab;

}