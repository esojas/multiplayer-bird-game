using TMPro;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class MainLobby : MonoBehaviour
{
public static MainLobby Instance { get; private set; }
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private TMP_InputField inputPlayerName;
    [SerializeField] private Button changePlayerNameButton;
    [SerializeField] private GameObject startGame;
    [SerializeField] private GameObject playerDataPrefab;
    [SerializeField] private GameObject[] playerSlots;
    [SerializeField] private GameObject exitLobby;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        exitLobby.GetComponent<Button>().onClick.AddListener(ExitLobby);
        changePlayerNameButton.onClick.AddListener(SetPlayerName);


    }

    private void OnEnable()
    {
        changePlayerNameButton.interactable = false;
        startGame.SetActive(false);
        IsPlayerHost();
        // Subscribe when panel opens
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnected;
        UpdatePlayerCountText();
        UpdatePlayerSlots();
    }

    private void OnDisable()
    {
        // Unsubscribe when panel closes
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnected;
        }
    }

    private void OnDisconnected(ulong clientId)
    {
        UpdatePlayerCountText();

        if (NetworkManager.Singleton.IsHost) return;

        // Fires on clients when host disconnects them
        MainUIManager.Instance.ShowMainMenuScreen();
    }

    private void ExitLobby()
    {
        PlayerNetworkData.ClearAllPlayers(); // wipe the list on exit
        NetworkManager.Singleton.Shutdown();
        MainUIManager.Instance.ShowMainMenuScreen();
    }

    private void OnClientConnect(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsHost) return; // only host spawns
        GameObject dataHolder = Instantiate(playerDataPrefab);
        dataHolder.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

        UpdatePlayerCountText();
    }

    private void IsPlayerHost()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            startGame.SetActive(true);
            startGame.GetComponent<Button>().onClick.AddListener(StartGame);

            GameObject dataHolder = Instantiate(playerDataPrefab);
            dataHolder.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void SetPlayerName()
    {
        PlayerNetworkData.Instance.SetPlayerName(inputPlayerName.text);
    }

    private void StartGame()
    {
        SceneLoader.Instance.LoadScene("CharacterSelection");
    }

    public void UpdatePlayerCountText()
    {
        int count = PlayerNetworkData.AllPlayers.Count;
        playerCountText.text = $"Players connected: {count}/4";
    }

    public void EnableNameInput()
    {
        changePlayerNameButton.interactable = true;
    }

    public void UpdatePlayerSlots()
    {
        var players = PlayerNetworkData.AllPlayers;

        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (playerSlots[i] == null) continue; // skip destroyed objects

            bool isOccupied = i < players.Count;
            playerSlots[i].SetActive(isOccupied);

            if (isOccupied)
            {
                string name = players[i].GetPlayerName();
                var nameText = playerSlots[i].GetComponentInChildren<TMP_Text>();
                if (nameText == null) continue; // safety check
                nameText.text = string.IsNullOrEmpty(name) ? $"Player {i + 1}" : name;
            }
        }
    }

    public void SetJoinCode(string codeText)
    {
        joinCodeText.text = codeText;
    }
}
