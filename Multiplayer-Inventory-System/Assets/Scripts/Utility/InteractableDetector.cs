using Fusion;
using UnityEngine;

public class InteractableDetector : NetworkBehaviour
{
    [SerializeField] private Transform interactionDetectionSource;
    [SerializeField] private float interactRange;
    [SerializeField] private IInteractable objectToInteractWith;
    [SerializeField] private NetworkObject objectToInteractWithNetwork;
    [SerializeField] private GameObject InteractButton;
    [SerializeField] private PlayerInput playerInput; // assign in inspector or via GetComponent

    //To see what object the interactor ray can see.
    [SerializeField] private GameObject objectToInteractWithObject;
    private bool previousInteractState = false; // Flag for the player's previous interact state

    public void OnNetworkSpawn()
    {
        if (!HasInputAuthority)
        {
            // Disable interactor for non client players
            this.enabled = false;
        }
    }

    // Keep this here as a failsafe so the interact button doesn't show up out of nowhere
    private void Start()
    {
        InteractButton.SetActive(false);
    }

    void FixedUpdate()
    {
        if (!HasInputAuthority || Runner == null)
            return; // Still initializing

        // Get Network Physics Scene
        var physicsScene = Runner.GetPhysicsScene();

        Vector3 origin = interactionDetectionSource.position;
        Vector3 direction = interactionDetectionSource.forward.normalized;

        Debug.DrawRay(origin, direction * interactRange, Color.red);

        if (physicsScene.Raycast(origin, direction, out RaycastHit objectHit, interactRange))
        {
            // Draw a debug ray to see what object is being interacted with
            //Debug.DrawRay(origin, objectHit.point, Color.green);

            IInteractable interactable = objectHit.collider.GetComponentInParent<IInteractable>();
            NetworkObject netObject = objectHit.collider.GetComponentInParent<NetworkObject>();
            GameObject rootObject = objectHit.collider.GetComponentInParent<NetworkObject>()?.gameObject;

            // If item collider is detected with an IInteractable component
            if (interactable != null)
            {
                // Assign the object variables to the respective Detector variables
                objectToInteractWith = interactable;
                objectToInteractWithNetwork = netObject;
                objectToInteractWithObject = rootObject;

                // Show interact prompt
                InteractButton.SetActive(true);
            }
        }
        else
        {
            // Nothing hit or not interactable
            objectToInteractWithObject = null;
            objectToInteractWith = null;
            objectToInteractWithNetwork = null;
            InteractButton.SetActive(false);
        }
        
        // Get current frame button state
        bool currentInteractState = playerInput.input.Buttons.IsSet(EInputButton.INTERACT);

        // Check network input for interact and compare with previous interact state
        if (currentInteractState && !previousInteractState)
        {
            OnInteract();
        }

        // Store current state to compare to next frame
        previousInteractState = currentInteractState;
    }

    // When the player holds the interact key and when an interactable object is in range, execute the RPC_Interaect method 
    public void OnInteract()
    {
        // If no interactable object is hit by the raycast
        if (objectToInteractWith == null)
            return;

        // Call the interact method
        objectToInteractWith.Interact(this.gameObject);

        RPC_RequestOpenChest(objectToInteractWithNetwork);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestOpenChest(NetworkObject chestObj)
    {
        if (!chestObj || !chestObj.TryGetComponent(out ItemChest chest))
            return;

        chest.ServerTryOpen(this.Object);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestBuyItem(NetworkObject itemObj)
    {
        if (!itemObj || !itemObj.TryGetComponent(out ShopInteractableItem shopItem))
            return;

        shopItem.ServerTryBuy(this.Object);
    }
}
