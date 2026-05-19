using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LoadCharacterManager : NetworkBehaviour
{
    public static LoadCharacterManager Instance { get; private set; }
    [SerializeField] private List<Transform> nestSpawnPoint = new List<Transform>();
    [SerializeField] private List<Transform> spawnPoint = new List<Transform>();
    [SerializeField] private List<GameObject> characterList = new List<GameObject>();
    [SerializeField] private GameObject nestPrefab;
    private int currentSpawnInt = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void ResetSpawnCounterInt()
    {
        currentSpawnInt = 0;
    }

    public void LoadPlayerCharacter(int currentCharacter, ulong clientID)
    {
        if (spawnPoint == null || spawnPoint.Count == 0)
        {
            Debug.LogError("spawnPoint list is empty!");
            return;
        
        }

        //PlayerNetworkData clientData = PlayerNetworkData.AllPlayers.Find(p => p.OwnerClientId == clientID);

        int spawnIndex = currentSpawnInt % spawnPoint.Count;
        //Debug.LogWarning($"[Spawn] Client {clientID} → Index {spawnIndex} (total spawnInt was {currentSpawnInt}) → Pos {spawnPoint[spawnIndex].position}");

        GameObject characterPrefab = characterList[currentCharacter] ?? characterList[0];
        Transform spawnPos = spawnPoint[spawnIndex];

        //Debug.Log($"Spawning {characterPrefab.name} at index {spawnIndex} → {spawnPos.position}");

        // Player
        GameObject player = Instantiate(characterPrefab, spawnPos.position, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
        Vector3 targetPos = spawnPos.position;
        //clientData.SetSpawnLocation(targetPos);        
        StartCoroutine(TeleportAfterSpawn(player, targetPos, clientID));

        // Nest
        Transform nestSpawnPos = nestSpawnPoint[spawnIndex];   // assuming same count
        GameObject nest = Instantiate(nestPrefab, nestSpawnPos.position, Quaternion.identity);
        nest.GetComponent<NetworkObject>().Spawn();
        nest.GetComponent<MainNestScript>().SetOwner(clientID);

        currentSpawnInt++;
    }

    public void RespawnPlayer(int currentCharacter, ulong clientID)
    {
        PlayerNetworkData clientData = PlayerNetworkData.AllPlayers.Find(p => p.OwnerClientId == clientID);

        GameObject characterPrefab = characterList[currentCharacter] ?? characterList[0];
        Vector3 spawnPos = clientData.GetSpawnLocation();

        // Player
        //Debug.Log($"[Respawn] Client {clientID} respawning at {spawnPos}");
        GameObject player = Instantiate(characterPrefab, spawnPos, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
        Vector3 targetPos = spawnPos;
        StartCoroutine(TeleportAfterSpawn(player, targetPos, clientID));
    }


    private IEnumerator TeleportAfterSpawn(GameObject player, Vector3 pos, ulong clientID)
    {
        yield return null;
        if (player == null) yield break;
        TeleportPlayerClientRpc(pos, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientID } }
        });
    }

    [ClientRpc]
    private void TeleportPlayerClientRpc(Vector3 position, ClientRpcParams rpcParams = default)
    {
        GameObject playerObj = NetworkManager.Singleton.LocalClient.PlayerObject?.gameObject;
        if (playerObj == null) return;

        // Now runs on client — find and set local PlayerNetworkData
        PlayerNetworkData clientData = PlayerNetworkData.AllPlayers
            .Find(p => p.OwnerClientId == NetworkManager.Singleton.LocalClientId);
        if (clientData != null)
            clientData.SetSpawnLocation(position);

        var cc = playerObj.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        playerObj.transform.position = position;
        if (cc) cc.enabled = true;
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
