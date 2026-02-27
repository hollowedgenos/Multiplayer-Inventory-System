using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using UnityEngine;

public class ItemDatabaseUtils
{
    private static string jsonPath = Application.streamingAssetsPath + $@"/JSONs/Item_Stats.json"; // JSON filepath holder

    // Grabs the JSON data and stores it as a node
    private static JsonNode GrabJsonData()
    {
        // Reads the JSON
        var json = File.ReadAllText(jsonPath);
        // Stores the root node
        return JsonNode.Parse(json);
    }

    public static List<ItemData> GenerateUnlockedItemList(List<ItemData> _fullItemDatabase)
    {
        var unlockedItemsList = new List<ItemData>();

        JsonArray itemArray = GrabJsonData()["items"].AsArray();

        for (int i = 0; i < itemArray.Count; i++)
        {
            if ((bool)itemArray[i]["unlocked"] == true) 
            {
                unlockedItemsList.Add(_fullItemDatabase.FirstOrDefault(item => item.itemID == $"{itemArray[i]["itemID"]}"));
            }
        }
        
        return unlockedItemsList;
    }

    public static void UnlockItem(ItemData _unlockedItem)
    {
        JsonNode rootNode = GrabJsonData();
        JsonArray itemArray = rootNode["items"].AsArray();

        var itemJsonData = itemArray.FirstOrDefault(item => $"{item["itemID"]}" == _unlockedItem.itemID);

        itemJsonData["unlocked"] = true;

        // Serialize to JSON unindented (unreadable)
        string updateJson = rootNode.ToJsonString();

        // Parse JSON again
        var unindentedJson = JsonDocument.Parse(updateJson);

        //Serialize JSON to indented (readable)
        string indentedJson = JsonSerializer.Serialize(unindentedJson, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(jsonPath, indentedJson);
        Debug.Log("JSON has been updated");
    }

}
