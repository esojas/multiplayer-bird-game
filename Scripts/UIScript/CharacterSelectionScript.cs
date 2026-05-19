using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectionScript : NetworkBehaviour
{
    public static CharacterSelectionScript Instance { get; private set; }

    [SerializeField] private List<GameObject> characterList = new List<GameObject>();
    int currentCharacter;
    [SerializeField] private TMP_Text countdownTimer;
    PlayerNetworkData myData;

    [SerializeField] private GameObject startGameButton;
    [SerializeField] private Button readyGameButton;
    [SerializeField] private Button nextCharButton;
    [SerializeField] private Button prevCharButton;

    private void Awake()
    {
        Instance = this;
    }

    public void NextCharacter()
    {
        characterList[currentCharacter].SetActive(false);
        currentCharacter = (currentCharacter + 1)%characterList.Count;
        characterList[currentCharacter].SetActive(true);
        countdownTimer.text = "Not Ready";
        myData.SetReady(false);
    }

    public void PreviousCharacter()
    {
        characterList[currentCharacter].SetActive(false);
        currentCharacter--;
        if(currentCharacter < 0)
        {
            currentCharacter += characterList.Count;
        }
        characterList[currentCharacter].SetActive(true);
        countdownTimer.text = "Not Ready";
        myData.SetReady(false);
    }

    public void OnButtonReady()
    {
        myData.SelectedCharacter(currentCharacter);
        countdownTimer.text = "Ready";
        myData.SetReady(true);
    }

    private void CheckIfPlayersReady()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        foreach (var players in PlayerNetworkData.AllPlayers)
        {
            if (!players.IsReady()) return;
        }

        StartCountdown();
    }

    public void OnPlayerReadyChanged()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        bool allReady = true;
        foreach (var player in PlayerNetworkData.AllPlayers)
        {
            if (!player.IsReady())
            {
                allReady = false;
                break;
            }
        }

        startGameButton.GetComponent<Button>().interactable = allReady;
    }

    public void StartGame()
    {
        CheckIfPlayersReady();
    }

    private void StartCountdown()
    {
        if (NetworkManager.Singleton.IsHost) startGameButton.GetComponent<Button>().interactable = false;
        LockCharSelectionClientRpc();
        CountDownClientRpc();
    }

    [ClientRpc]
    private void CountDownClientRpc()
    {
        StartCoroutine(CountdownRoutine());
    }

    [ClientRpc]
    private void LockCharSelectionClientRpc()
    {
        readyGameButton.interactable = false;
        prevCharButton.interactable = false;
        nextCharButton.interactable = false;
    }

    private IEnumerator CountdownRoutine()
    {
        for (int countdown = 3; countdown > 0; countdown--)
        {
            countdownTimer.text = countdown.ToString();
            yield return new WaitForSeconds(1f);
        }

        //if (NetworkManager.Singleton.IsHost)
        //{
        //SceneLoader.Instance.LoadScene("SampleScene");
        SceneLoader.Instance.LoadScene("MainGame");
        //}
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nextCharButton.onClick.AddListener(NextCharacter);
        prevCharButton.onClick.AddListener(PreviousCharacter);

        myData = PlayerNetworkData.Instance;
        startGameButton.SetActive(false);
        readyGameButton.onClick.AddListener(OnButtonReady);
        if (NetworkManager.Singleton.IsHost)
        {
            startGameButton.SetActive(true);
            startGameButton.GetComponent<Button>().interactable = false;
            startGameButton.GetComponent<Button>().onClick.AddListener(StartGame);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
