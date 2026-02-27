using Fusion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;


public class LocalInventory : NetworkBehaviour
{
    private Dictionary<ItemData, int> Inventory = new Dictionary<ItemData, int>(); // Stores the item data and the item counter
    public Dictionary<string, float> EffectModifiers = new Dictionary<string, float>(5) // Stores the effect modifier stats from the inventory
    {
       {"hp_multiply", 0.00f},
       {"defense_add", 0.00f},
       {"speed_multiply", 0.00f},
       {"gold_gain_multiply", 0.00f},
       {"xp_gain_multiply", 0.00f}
    };
    public TMP_Text moneyText;
    public int oldItemValue = 0; // Stores the old value for the item from the Inventory
    [Networked]
    public int Money { get; set; } = 0;

    public override void FixedUpdateNetwork()
    {
        // Update UI every tick for owner
        if (Object.HasInputAuthority && moneyText != null)
        {
            moneyText.text = $"Money: {Money}";
        }
    }

    // Adds item to inventory
    public void AddItem(ItemData item)
    {
        // If the Inventory is not empty
        if (Inventory != null || Inventory.Count != 0)
        {
            foreach (var pair in Inventory.Keys.ToList()) // For each keyvalue pair in the Inventory
            {
                Debug.Log(pair + "==" + item);
                if (pair == item) // If a key is the same as the current item
                {
                    oldItemValue = Inventory[item]; // Store the old keys value here
                    Inventory[pair]++; // Increment key's value by 1
                    Debug.Log(pair + ", " + Inventory[pair]);
                }
            }
        }

        // If Inventory is empty, or if none of the keys match the current item,
        if (Inventory != null && Inventory.Count == 0|| !Inventory.ContainsKey(item))
        {
            Inventory.Add(item, 1); // Add the item and set value to 1
            oldItemValue = 0;
            Debug.Log(item + ", " + Inventory[item]);
        }
    }
    public void AddMoneyServer(int amount)
    {
        if (!Object.HasStateAuthority) return;

        Money += amount;
    }

    // Grabs the item effect values for each item in Inventory dictionary,
    // and appends them to their corresponding keys in the EffectModifiers dictionary
    public void AppendModifiers(ItemData item)
    {
        if (Inventory.TryGetValue(item, out int currentItemValue)) // Check if current item value exists and assign to currentItemValue
        {
            Debug.Log("Current Value: " + currentItemValue + ", Old Value: " + oldItemValue);
            if (currentItemValue != oldItemValue) // If the current item value has incremented
            {

                float convertHp = (float)item.effects[0].IntValue; // Convert hp_mult to float
                EffectModifiers["hp_multiply"] += convertHp; // Add the hp 

                float convertDefense = (float)item.effects[1].IntValue;
                EffectModifiers["defense_add"] += convertDefense;

                if (item.effects[2].StringValue != "") // Unity gets fussy and causes problems if we don't check for an empty string
                {
                    float convertSpeed = ConvertStringToFloat(item.effects[2].StringValue);
                    EffectModifiers["speed_multiply"] += convertSpeed;
                }

                if (item.effects[3].StringValue != "")
                {
                    float convertGoldGain = ConvertStringToFloat(item.effects[3].StringValue);
                    EffectModifiers["gold_gain_multiply"] += convertGoldGain;
                }

                if (item.effects[4].StringValue != "")
                {
                    float convertXpGain = ConvertStringToFloat(item.effects[4].StringValue);
                    EffectModifiers["xp_gain_multiply"] += convertXpGain;
                }


                Debug.Log($@"HP_MULT: {EffectModifiers["hp_multiply"]:0.0#}
                    DEFENSE_ADD: {EffectModifiers["defense_add"]:0.0#}
                    SPEED_MULT: {EffectModifiers["speed_multiply"]:0.0#}
                    GOLD_GAIN_MULT: {EffectModifiers["gold_gain_multiply"]:0.0#}
                    XP_GAIN_MULT: {EffectModifiers["xp_gain_multiply"]:0.0#}"); // MAKING ABSOLUTELY SURE UNITY IS STORING THIS AS A FLOAT

            }
        }
    }

    // Converts a string to a float
    public float ConvertStringToFloat(string text)
    {
        // Convert string to float, InvariantCulture prevents format exceptions
        text = text.Trim();
        float StringToFloatVal = float.Parse(text, CultureInfo.InvariantCulture);
        StringToFloatVal = (float)Math.Round(StringToFloatVal, 2); // Round to hundredths place
        return StringToFloatVal;
    }
}
