using UnityEngine;
using Fusion;

[System.Serializable]
public class WorldItem : NetworkBehaviour, IInteractable
{
    [Networked] public bool IsClaimed { get; set; } = false;

    public void Interact(GameObject objectInteracting)
    {
        if (IsClaimed)
        {
            return;
        }

        GetComponentInChildren<Item>().AddItem(objectInteracting);
        if (Runner.IsServer)
        {
            IsClaimed = true;
            Runner.Despawn(Object);
        }
        else
        {
            RPC_UpdateClaimed();
        }
    }

    [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPC_UpdateClaimed()
    {
        IsClaimed = true;
        Runner.Despawn(Object);
    }

}
