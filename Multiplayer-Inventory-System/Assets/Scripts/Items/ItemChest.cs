using Fusion;
using UnityEngine;

public class ItemChest : NetworkBehaviour, IInteractable
{
    [Networked] public bool IsClaimed { get; set; } = false;
    private ItemManager itemManager;
    [SerializeField] private int itemPrice;

    public override void Spawned()
    {
        itemManager = ItemManager.Instance;
    }

    public void Interact(GameObject objectInteracting)
    {
        if (IsClaimed)
        {
            return;
        }

        var inventory = objectInteracting.GetComponentInParent<LocalInventory>();

        if (inventory == null)
        {
            Debug.LogError("LocalInventory missing on: " + objectInteracting.name);
            return;
        }

        //Generate the Item to spawn
        var itemToSpawn = itemManager.GenerateUnlockedItem();
        if (itemToSpawn == null)
        {
            Debug.LogError("Unlocked Items List Empty! Could not Generate an Unlocked Item!");
            return;
        }
        var itemIndex = itemManager.GetItemIndex(itemToSpawn);
        var playerNetObj = objectInteracting.GetComponentInParent<NetworkObject>();
        var interactableDetector = objectInteracting.GetComponentInParent<InteractableDetector>();


        if (playerNetObj == null || interactableDetector == null)
            return;

        if (!playerNetObj.HasInputAuthority)
            return;

    }

    private void SpawnItem(int _itemIndex)
    {
        //Spawn the Item on the server
        itemManager.SpawnItem(_itemIndex, transform.position);
        //Destroy the chest
        Runner.Despawn(Object);
    }

    public void ServerTryOpen(NetworkObject playerObj)
    {
        if (!Object.HasStateAuthority)
            return;

        if (IsClaimed)
            return;

        if (!playerObj.TryGetComponent(out LocalInventory inventory))
            return;

        if (inventory.Money < itemPrice)
        {
            Debug.Log("Not enough money!");
            return;
        }

        var itemToSpawn = itemManager.GenerateUnlockedItem();
        if (itemToSpawn == null)
            return;

        var itemIndex = itemManager.GetItemIndex(itemToSpawn);

        inventory.AddMoneyServer(-itemPrice);
        IsClaimed = true;
        //itemManager.SpawnItem(itemIndex, transform.position);
        Runner.Despawn(Object);
    }

}
