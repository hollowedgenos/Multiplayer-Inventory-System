using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : NetworkBehaviour
{
    public static ItemManager Instance { get; private set; }

    [SerializeField] private ItemDatabase database;
    [SerializeField] private NetworkObject worldItemContainer;
    private float totalDropWeightAllItems;
    private float totalUnlockedDropWeight;

    public bool spawnItems = false;
    
    private void Awake()
    {
        
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
       
        Instance = this;
        
        DontDestroyOnLoad(Instance);
    }
    
    void Start()
    {
        database.unlockedItems.Clear();
        database.unlockedItems = ItemDatabaseUtils.GenerateUnlockedItemList(database.allItems);
        UpdateDropWeight(database.unlockedItems, ref totalUnlockedDropWeight);
        UpdateDropWeight(database.allItems, ref totalDropWeightAllItems);
    }

    private void AddItemToUnlockedList(ItemData _itemToAdd)
    {
        //Add to list of unlocked items
        database.unlockedItems.Add(_itemToAdd);
        //Add to JsonDatabase
        ItemDatabaseUtils.UnlockItem(_itemToAdd);
    }

    public void CheckIfItemIsUnlocked(ItemData _itemToCheck)
    {
        if (!database.unlockedItems.Contains(_itemToCheck))
        {
            AddItemToUnlockedList(_itemToCheck);
            UpdateDropWeight(database.unlockedItems, ref totalUnlockedDropWeight);
        }
    }

    private void UpdateDropWeight(List<ItemData> _listToCheck, ref float _totalDropWeightToUpdate)
    {
        //Updates our chached total weight to prevent recalculating every time we want to drop an item
        _totalDropWeightToUpdate = 0f;

        foreach (var item in _listToCheck)
        {
            switch (item.rarity)
            {
                case ItemData.ITEM_RARITY.Uncommon:
                    _totalDropWeightToUpdate += database.CommonWeight;
                    break;
                case ItemData.ITEM_RARITY.Rare:
                    _totalDropWeightToUpdate += database.RareWeight;
                    break;
                case ItemData.ITEM_RARITY.Legendary:
                    _totalDropWeightToUpdate += database.LegendaryWeight;
                    break;
                default:
                    Debug.LogWarning($"ItemWeight error, {item.itemName} has invalid rarity assignment");
                    break;
            }
        }
    }

    public ItemData GenerateRandomItem()
    {
        return GenerateItem(database.allItems, totalDropWeightAllItems);
    }

    public ItemData GenerateUnlockedItem()
    {
        return GenerateItem(database.unlockedItems, totalUnlockedDropWeight);
    }

    private ItemData GenerateItem(List<ItemData> _listToCheck, float _dropWeight)
    {
        // Generate a random value between 0 and the total weight
        float randomValue = Random.Range(0f, _dropWeight);

        // Iterate through the items to find the one corresponding to the random value
        float currentWeight = 0f;
        foreach (var item in _listToCheck)
        {
            switch (item.rarity)
            {
                case ItemData.ITEM_RARITY.Uncommon:
                    currentWeight += database.CommonWeight;
                    break;
                case ItemData.ITEM_RARITY.Rare:
                    currentWeight += database.RareWeight;
                    break;
                case ItemData.ITEM_RARITY.Legendary:
                    currentWeight += database.LegendaryWeight;
                    break;
                default:
                    Debug.LogWarning($"ItemWeight error, {item.itemName} has invalid rarity assignment");
                    break;
            }

            if (randomValue <= currentWeight)
            {
                return item; // This is the selected item
            }
        }
        Debug.LogError("random value did not correspond to an item to spawn!");
        return null;
    }

    public int GetItemIndex(ItemData _itemToFind)
    {
        return database.allItems.IndexOf(_itemToFind);
    }

    public  void SpawnItem(int _itemIndex, Vector3 _spawnPos)
    {
        if (Runner.IsServer)
        {
            var newItemContainer = Runner.Spawn(worldItemContainer, _spawnPos);
            var newItem = Runner.Spawn(database.allItems[_itemIndex].itemPrefab, newItemContainer.transform.position);
            newItemContainer.transform.parent = null;
            newItem.transform.parent = newItemContainer.transform;
        }
        else
        {
            Debug.LogError("SpawnItemsCalled on client! only the server is allowed to spawn items!");
        }
    }


}
