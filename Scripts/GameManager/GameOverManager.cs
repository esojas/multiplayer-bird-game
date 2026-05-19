using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameOverManager : NetworkBehaviour
{
    public static GameOverManager Instance { get; private set; }
    [SerializeField] private Canvas gameOverCanvas; // ✅ reference to canvas
    [SerializeField] private TextMeshProUGUI disconnectingText;
    [SerializeField] private GameObject[] gameOverScoreBoardEntry;
    [SerializeField] private NetworkTimerManager countdownTimer;
    [SerializeField] private float gameOverDuration = 10f;
    public event System.Action OnGameOverTimerEnd;

    private void Awake() => Instance = this;

    public void StartGameOver()
    {
        ShowGameOverClientRpc(); // ✅ show canvas on ALL clients

        if (IsServer)
            countdownTimer.StartTimer(gameOverDuration);
    }

    [ClientRpc]
    private void ShowGameOverClientRpc()
    {
        gameOverCanvas.gameObject.SetActive(true);
        UpdateEndScoreList();

        // ✅ All clients subscribe here
        countdownTimer.OnTimerUpdate += UpdateTimerDisplay;
        countdownTimer.OnTimerEnd += OnCountdownEnd;
    }

    private void OnCountdownEnd()
    {
        OnGameOverTimerEnd?.Invoke();
    }

    private void UpdateTimerDisplay(float time)
    {
        int seconds = Mathf.FloorToInt(time);
        disconnectingText.text = $"Disconnecting in {seconds}...";
    }

    private void UpdateEndScoreList()
    {
        var players = PlayerNetworkData.AllPlayers
            .OrderByDescending(p => p.GetEggsOwned())
            .ToList();

        for (int i = 0; i < gameOverScoreBoardEntry.Length; i++)
        {
            bool isOccupied = i < players.Count;
            gameOverScoreBoardEntry[i].SetActive(isOccupied);
            if (isOccupied)
            {
                string name = players[i].GetPlayerName();
                int score = players[i].GetEggsOwned();
                var scoreText = gameOverScoreBoardEntry[i].transform.Find("Score").GetComponent<TextMeshProUGUI>();
                var nameText = gameOverScoreBoardEntry[i].transform.Find("PlayerName").GetComponent<TextMeshProUGUI>();
                nameText.text = string.IsNullOrEmpty(name) ? $"Player {i + 1}" : name;
                scoreText.text = score.ToString();
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
