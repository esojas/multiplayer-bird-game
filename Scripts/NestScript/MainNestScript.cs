using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;


public class MainNestScript : NetworkBehaviour
{
    public int rows;
    public int columns;
    public float slotGap;

    [SerializeField] private Material enemyNestHighlight;
    [SerializeField] private Material mainNestHighlight;
    [SerializeField] private EggAmountScript eggAmountScript;

    public int eggSpawnAmount; // To tell how many egg amount for spawn
    public GameObject eggPrefab;
    private int eggCurrentAmount = 0; // track egg current amount
    private AudioSource audioSource;

    public class NestSlot
    {
        public Vector3 location;
        public GameObject egg;
        public bool isOccupied;
    }

    public List<NestSlot> slots = new List<NestSlot>();

    // Who owns this nest
    private NetworkVariable<ulong> ownerClientId = new NetworkVariable<ulong>
        (writePerm: NetworkVariableWritePermission.Server);

    private NetworkVariable<int> eggCount = new NetworkVariable<int>
        (writePerm: NetworkVariableWritePermission.Server);

    public int EggCount => eggCount.Value;

    // Call this when spawning the nest to assign ownership
    public void SetOwner(ulong clientId)
    {
        if (!IsServer) return;
        ownerClientId.Value = clientId;
        ApplyHighlight();
    }

    private void SetMainNestHighlight()
    {
        Material[] mats = GetComponent<Renderer>().materials;
        mats[1] = mainNestHighlight; 
        GetComponent<Renderer>().materials = mats;
    }

    private void SetEnemyNestHighlight()
    {
        Material[] mats = GetComponent<Renderer>().materials;
        mats[1] = enemyNestHighlight; 
        GetComponent<Renderer>().materials = mats;
    }

    public bool IsMyNest()
    {
        return ownerClientId.Value == NetworkManager.Singleton.LocalClientId;
    }

    public bool IsEnemyNest()
    {
        return ownerClientId.Value != NetworkManager.Singleton.LocalClientId;
    }

    private void DisplayEggAmountText(int eggAmount)
    {

        eggAmountScript.UpdateText(eggAmount);
    }

    public override void OnNetworkSpawn()
    {
        GenerateSlots();

        if (IsServer)
            SpawnEggs();

        eggCount.OnValueChanged += (oldVal, newVal) =>
        {
            DisplayEggAmountText(newVal);
        };

        ownerClientId.OnValueChanged += (oldVal, newVal) =>
        {
            ApplyHighlight();
        };

        ApplyHighlight();

        if (!IsServer)
            StartCoroutine(InitialDisplayUpdate());
    }

    private void ApplyHighlight()
    {
        if (IsMyNest()) SetMainNestHighlight();
        else SetEnemyNestHighlight();
    }

    private IEnumerator InitialDisplayUpdate()
    {
        // Wait until eggCount actually has the synced value
        yield return new WaitUntil(() => eggCount.Value > 0);
        DisplayEggAmountText(eggCount.Value);
    }

    private void SpawnEggs() // ✅ server only now
    {
        if (!IsServer) return; // ✅ only host spawns eggs

        for (int i = 0; i < eggSpawnAmount; i++)
        {
            GameObject eggObject = Instantiate(eggPrefab);
            Vector3 eggPosition = transform.TransformPoint(slots[i].location);
            eggObject.transform.position = eggPosition;
            slots[i].isOccupied = true;
            slots[i].egg = eggObject;
            eggCurrentAmount++;

            NetworkObject netObj = eggObject.GetComponent<NetworkObject>();
            netObj.Spawn();
            eggObject.transform.SetParent(transform);

            AssignEggToSlotClientRpc(i, netObj.NetworkObjectId);
        }

        eggCount.Value = eggCurrentAmount;
        DisplayEggAmountText(eggCurrentAmount);
    }

    [ClientRpc]
    private void AssignEggToSlotClientRpc(int slotIndex, ulong networkObjectId)
    {
        if (IsServer) return; // host already has the reference

        // Find the spawned egg by its NetworkObjectId
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            slots[slotIndex].egg = netObj.gameObject;
        }
    }



    private void GenerateSlots()
    {
        int pivot = columns / 2;

        for (int y = -2; y < rows; y+=2)
        {
            for(int x = -3; x < columns; x+=2)
            {
                slots.Add(new NestSlot 
                {
                    location = new Vector3(x * slotGap, -0.045f, y * slotGap),
                    isOccupied = false
                });
            }

        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void TakeEggNestServerRpc(RpcParams rpcParams = default)
    {
        ulong requestingClientId = rpcParams.Receive.SenderClientId; // ✅ who is taking the egg

        for (int x = 0; x < slots.Count; x++)
        {
            if (slots[x].isOccupied)
            {
                slots[x].isOccupied = false;
                eggCurrentAmount--;
                eggCount.Value = eggCurrentAmount;

                ulong eggNetId = slots[x].egg.GetComponent<NetworkObject>().NetworkObjectId;

                // ✅ Use this instead of UpdateSlotClientRpc
                MoveEggToPlayerClientRpc(x, eggNetId, requestingClientId, eggCurrentAmount);
                return;
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void PutEggNestServerRpc(ulong eggNetId, RpcParams rpcParams = default)
    {
        ulong fromClientId = rpcParams.Receive.SenderClientId; // ✅ track who is returning

        for (int x = 0; x < slots.Count; x++)
        {
            if (!slots[x].isOccupied)
            {
                slots[x].isOccupied = true;
                eggCurrentAmount++;
                eggCount.Value = eggCurrentAmount;

                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(eggNetId, out NetworkObject netObj))
                    slots[x].egg = netObj.gameObject;

                ReturnEggToSlotClientRpc(x, eggNetId, fromClientId, eggCurrentAmount); // ✅ pass fromClientId
                return;
            }
        }
    }

    [ClientRpc]
    private void ReturnEggToSlotClientRpc(int slotIndex, ulong eggNetId, ulong fromClientId, int newEggCount)
    {
        eggCurrentAmount = newEggCount;
        DisplayEggAmountText(newEggCount);
        slots[slotIndex].isOccupied = true;

        // ✅ Release egg from holder on ALL clients
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(fromClientId, out var client))
        {
            if (client.PlayerObject != null)
            {
                EggHolder holder = client.PlayerObject.GetComponent<EggHolder>();
                holder.ReleaseEgg();
            }
        }

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(eggNetId, out NetworkObject netObj))
        {
            if (IsServer)
            {
                netObj.ChangeOwnership(NetworkManager.ServerClientId);
                netObj.transform.SetParent(transform);
            }

            slots[slotIndex].egg = netObj.gameObject;
            netObj.transform.position = transform.TransformPoint(slots[slotIndex].location);
            netObj.gameObject.SetActive(true);
        }
    }

    [ClientRpc]
    private void MoveEggToPlayerClientRpc(int slotIndex, ulong eggNetId, ulong targetClientId, int newEggCount)
    {
        slots[slotIndex].isOccupied = false;
        eggCurrentAmount = newEggCount;
        DisplayEggAmountText(newEggCount);

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(eggNetId, out NetworkObject netObj))
        {
            if (IsServer)
                netObj.ChangeOwnership(targetClientId);
            // Find the player who took the egg
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(targetClientId, out var client))
            {
                EggHolder holder = client.PlayerObject.GetComponent<EggHolder>();
                holder.HoldEgg(netObj.gameObject); // ✅ runs on ALL clients
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        eggAmountScript = transform.GetChild(0).GetComponent<EggAmountScript>();

        //GenerateSlots();
        //SpawnEggs();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
