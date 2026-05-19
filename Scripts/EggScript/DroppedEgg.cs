using Unity.Netcode;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class DroppedEgg : NetworkBehaviour
{
    // Stores which egg this dropped object is holding
    private NetworkVariable<ulong> heldEggNetId = new NetworkVariable<ulong>(
        writePerm: NetworkVariableWritePermission.Server);

    // Called by server when spawning this after player death
    public void SetEgg(ulong eggNetId)
    {
        if (!IsServer) return;
        heldEggNetId.Value = eggNetId;

        // Parent the actual egg under this object visually
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(eggNetId, out NetworkObject netObj))
        {
            netObj.transform.SetParent(transform);
            netObj.transform.localPosition = Vector3.zero;
        }
        ShowEggClientRpc(eggNetId); // ← show on all clients
    }

    [ClientRpc]
    private void ShowEggClientRpc(ulong eggNetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(eggNetId, out NetworkObject netObj))
        {
            foreach (Renderer rend in netObj.GetComponentsInChildren<Renderer>())
                rend.enabled = true;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void TakeEggServerRpc(RpcParams rpcParams = default)
    {
        ulong requestingClientId = rpcParams.Receive.SenderClientId;
        ulong eggNetId = heldEggNetId.Value;

        MoveEggToPlayerClientRpc(eggNetId, requestingClientId);

        // Despawn this dropped egg marker
        GetComponent<NetworkObject>().Despawn();
    }

    [ClientRpc]
    private void MoveEggToPlayerClientRpc(ulong eggNetId, ulong targetClientId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(eggNetId, out NetworkObject netObj))
        {
            if (IsServer)
                netObj.ChangeOwnership(targetClientId);

            // ← Re-enable renderers when picked up
            foreach (Renderer rend in netObj.GetComponentsInChildren<Renderer>())
                rend.enabled = true;

            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(targetClientId, out var client))
            {
                EggHolder holder = client.PlayerObject.GetComponent<EggHolder>();
                holder.HoldEgg(netObj.gameObject);
            }
        }
    }




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
