using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class LeaderBoard : MonoBehaviour
{
    public static LeaderBoard Instance { get; private set; }
    [SerializeField] private GameObject[] scoreBoardEntry;

    //private void Awake()
    //{
    //    Instance = this;
    //}

    public void UpdateScoreboardEntry()
    {
        if (!Application.isPlaying) return;
        var players = PlayerNetworkData.AllPlayers;

        for (int i = 0; i < scoreBoardEntry.Length; i++)
        {
            if (scoreBoardEntry[i] == null) continue;
            bool isOccupied = i < players.Count;
            scoreBoardEntry[i].SetActive(isOccupied);

            if (isOccupied)
            {
                string name = players[i].GetPlayerName();
                int score = players[i].GetEggsOwned();
                var scoreText = scoreBoardEntry[i].transform.Find("EggsOwnedText").GetComponent<TextMeshProUGUI>();
                var nameText = scoreBoardEntry[i].transform.Find("PlayersText").GetComponent<TextMeshProUGUI>();
                nameText.text = string.IsNullOrEmpty(name) ? $"Player {i + 1}" : name;
                scoreText.text = score.ToString();
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;

        StartCoroutine(DelayedUpdate());
    }

    private IEnumerator DelayedUpdate()
    {
        yield return new WaitForSeconds(0.5f);
        UpdateScoreboardEntry();

        foreach (var player in PlayerNetworkData.AllPlayers)
        {
            player.OnEggsOwnedChanged += UpdateScoreboardEntry;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
