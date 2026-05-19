using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuManager : NetworkBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [SerializeField] Canvas pauseMenuCanvas;
    [SerializeField] Button respawnButton;
    [SerializeField] Button optionButton;
    [SerializeField] GameObject optionScreen;

    public bool characterIsDead = false;
    public bool togglePause { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void CallOption()
    {
        optionScreen.SetActive(true);
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RespawnCharacterServerRpc(ulong clientID)
    {

        NetworkObject existingPlayer = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject;
        if (existingPlayer != null)
            existingPlayer.GetComponent<PlayerHealth>()?.RequestDamageServerRpc(1000000f);

        PlayerNetworkData clientData = PlayerNetworkData.AllPlayers.Find(p => p.OwnerClientId == clientID);

        int selectedCharacter = clientData.GetSelectedPlayer();
        StartCoroutine(DelayedRespawn(selectedCharacter, clientID));
    }

    public void OnPlayerDied()
    {
        characterIsDead = true;
        CallPauseMenu();
    }

    public void OnPlayerRespawned()
    {
        characterIsDead = false;
        if (togglePause)
            CallPauseMenu(); // Only close if actually open
    }

    private IEnumerator DelayedRespawn(int selectedCharacter, ulong clientID)
    {
        yield return new WaitForSeconds(0.1f);
        LoadCharacterManager.Instance.RespawnPlayer(selectedCharacter, clientID);


        NetworkManager.Singleton.ConnectedClients[clientID]
            .PlayerObject.GetComponent<PlayerHealth>()
            .NotifyClientOfRespawn(clientID);
    }

    private void CallPauseMenu()
    {
        togglePause = !togglePause;

        Cursor.lockState = togglePause ? CursorLockMode.None: CursorLockMode.Locked;
        pauseMenuCanvas.gameObject.SetActive(togglePause);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        respawnButton.onClick.AddListener(()=>RespawnCharacterServerRpc(NetworkManager.Singleton.LocalClientId));
        optionButton.onClick.AddListener(CallOption);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CallPauseMenu();
        }
    }
}
