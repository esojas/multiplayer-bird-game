using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkData : NetworkBehaviour
{
    public static PlayerNetworkData Instance { get; private set; }

    private NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>
        (writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> selectedCharacter = new NetworkVariable<int>
        (writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> isReady = new NetworkVariable<bool> 
        (writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> eggsOwned = new NetworkVariable<int> 
        (writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Vector3> spawnLocation = new NetworkVariable<Vector3>
        (writePerm: NetworkVariableWritePermission.Owner);

    public static List<PlayerNetworkData> AllPlayers = new List<PlayerNetworkData>();
    public event System.Action OnEggsOwnedChanged;
    public override void OnNetworkSpawn()
    {
        //Debug.LogWarning($"OnNetworkSpawn — IsOwner: {IsOwner}, OwnerClientId: {OwnerClientId}, Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");

        if (IsOwner)
        {
            Instance = this;
            MainLobby.Instance?.EnableNameInput();
        }

        AllPlayers.Add(this);
        //Debug.LogWarning($"AllPlayers count after add: {AllPlayers.Count}");
        playerName.OnValueChanged += (oldVal, newVal) =>
        {
            MainLobby.Instance?.UpdatePlayerSlots();
            MainLobby.Instance?.UpdatePlayerCountText();
            LeaderBoard.Instance?.UpdateScoreboardEntry();
        };

        eggsOwned.OnValueChanged += (oldVal, newVal) =>
        {
            LeaderBoard.Instance?.UpdateScoreboardEntry();
            OnEggsOwnedChanged?.Invoke(); // ✅ fires the event too
        };

        isReady.OnValueChanged += (oldVal, newVal) =>
        {
            if (NetworkManager.Singleton.IsHost)
            {
                CharacterSelectionScript.Instance?.OnPlayerReadyChanged();
            }
        };

        MainLobby.Instance?.UpdatePlayerSlots();
        MainLobby.Instance?.UpdatePlayerCountText();
        LeaderBoard.Instance?.UpdateScoreboardEntry();
    }

    public override void OnNetworkDespawn()
    {
        //Debug.LogWarning($"OnNetworkDespawn — IsOwner: {IsOwner}, OwnerClientId: {OwnerClientId}, Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        if (IsOwner) Instance = null;
        AllPlayers.Remove(this);
        //Debug.LogWarning($"AllPlayers count after remove: {AllPlayers.Count}");
        // ✅ Only update UI if we're not shutting down
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            MainLobby.Instance?.UpdatePlayerSlots();
            MainLobby.Instance?.UpdatePlayerCountText();
            LeaderBoard.Instance?.UpdateScoreboardEntry();
        }
    }

    // ✅ Clear the list when a session ends entirely
    public static void ClearAllPlayers()
    {
        AllPlayers.Clear();
    }

    public void SetPlayerName(string inputName)
    {
        if (!IsOwner) return;
        playerName.Value = inputName;
    }

    public string GetPlayerName() => playerName.Value.ToString();

    public void SelectedCharacter(int selectedChar)
    {
        if (!IsOwner) return;
        selectedCharacter.Value = selectedChar;
    }

    public int GetSelectedPlayer() => selectedCharacter.Value;

    public void SetReady(bool ready)
    {
        if (!IsOwner) return;
        isReady.Value = ready;
    }

    public bool IsReady() => isReady.Value;

    public void SetEggsOwned(int eggCount)
    {
        if (!IsOwner) return;
        eggsOwned.Value = eggCount;
    }

    public int GetEggsOwned() => eggsOwned.Value; 

    public void SetSpawnLocation(Vector3 playerSpawnLocation)
    {
        if (!IsOwner) return;
        spawnLocation.Value = playerSpawnLocation;
    }

    public Vector3 GetSpawnLocation() => spawnLocation.Value;

}