using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerScript : NetworkBehaviour
{
    [SerializeField] private GameObject nestPrefab;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private Camera cam;


    private void Awake()
    {

    }

    private void OnEnable()
    {

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {


        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainGame")  // Dont forget to change this
        {
            LoadCharacterManager.Instance.ResetSpawnCounterInt();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadCharacterManager.Instance.ResetSpawnCounterInt();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadCompleted;

        StartCoroutine(WaitAndSubscribeTimers()); 
    }

    private IEnumerator WaitAndSubscribeTimers()
    {
        yield return new WaitUntil(() =>
            GameTimer.Instance != null && GameOverManager.Instance != null);

        // ✅ Just subscribe, timer already started
        GameTimer.Instance.OnGameOver += OnGameTimerEnd;
        GameOverManager.Instance.OnGameOverTimerEnd += EndGame;
    }

    private void OnGameTimerEnd()
    {
        GameOverManager.Instance.StartGameOver();
    }

    private void OnSceneLoadCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName != "MainGame") return;

        // ← THIS IS THE FIX
        LoadCharacterManager.Instance.ResetSpawnCounterInt();
        Debug.Log("=== NEW GAME - Spawn counter reset to 0 ===");
        // ✅ Only spawn characters when loading the game scene
        foreach (var client in NetworkManager.Singleton.ConnectedClientsIds)
        {
            OnClientConnect(client);
        }

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadCompleted;
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)   
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnect;

        if (GameTimer.Instance != null)
            GameTimer.Instance.OnGameOver -= OnGameTimerEnd; 

        if (GameOverManager.Instance != null)
            GameOverManager.Instance.OnGameOverTimerEnd -= EndGame; 
    }

    private void OnClientConnect(ulong clientID)
    {

        PlayerNetworkData clientData = PlayerNetworkData.AllPlayers.Find(p => p.OwnerClientId == clientID);

        if (clientData == null)
        {
            //Debug.LogWarning($"No PlayerNetworkData found for client {clientID}");
            return;
        }

        int selectedCharacter = clientData.GetSelectedPlayer();
        LoadCharacterManager.Instance.LoadPlayerCharacter(selectedCharacter, clientID);
    }

    public void EndGame()
    {
        if (!IsServer) return;

        // Server tells everyone (including itself) to go back cleanly
        EndGameClientRpc();
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        // Start a coroutine so we can wait 1-2 frames after Shutdown
        StartCoroutine(EndGameCoroutine());
    }

    private IEnumerator EndGameCoroutine()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnect;
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();   // only once
        }

        // Wait for Netcode to actually finish shutting down (this is the magic line)
        yield return null;   // let LateUpdate run
        yield return null;   // extra frame for safety (cheap, fixes 99% of freezes)

        // Now it's safe to destroy and load
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }

        DOTween.KillAll(true);
        Cursor.lockState = CursorLockMode.None;
        PlayerNetworkData.ClearAllPlayers();

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
