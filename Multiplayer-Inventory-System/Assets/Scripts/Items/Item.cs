using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private ItemData data;

    public void AddItem(GameObject _playerToAddItemTo)
    {
        _playerToAddItemTo.GetComponent<LocalInventory>().AddItem(data); // Add New Item to Inventory
        _playerToAddItemTo.GetComponent<LocalInventory>().AppendModifiers(data);

        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.CheckIfItemIsUnlocked(data);
        }
    }
}
