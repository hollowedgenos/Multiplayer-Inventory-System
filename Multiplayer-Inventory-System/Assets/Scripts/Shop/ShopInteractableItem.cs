using Fusion;
using UnityEngine;

public class ShopInteractableItem : NetworkBehaviour, IInteractable
{
    [Networked] public bool IsClaimed { get; set; } = false;

    [SerializeField] private int itemPrice = 20;

    public void Interact(GameObject objectInteracting)
    {
        if(IsClaimed)
        {
            return;
        }

        var playerNetObj = objectInteracting.GetComponentInParent<NetworkObject>();
        var detector = objectInteracting.GetComponentInParent<InteractableDetector>();

        if (playerNetObj == null || detector == null)
            return;

        if (!playerNetObj.HasInputAuthority)
            return;

        detector.RPC_RequestBuyItem(Object);

    }

    public void ServerTryBuy(NetworkObject playerObj)
    {
        if (!Object.HasStateAuthority)
            return;

        if (IsClaimed)
            return;

        if (!playerObj.TryGetComponent(out LocalInventory inventory))
            return;

        if (inventory.Money < itemPrice)
        {
            Debug.Log("Not enough money to buy item!");
            return;
        }

        var item = GetComponentInChildren<Item>();
        if (item == null)
        {
            Debug.LogError("Item component missing!");
            return;
        }

        inventory.AddMoneyServer(-itemPrice);

        item.AddItem(playerObj.gameObject);

        IsClaimed = true;
        Runner.Despawn(Object);
    }
}
