using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostLobby : MonoBehaviour
{
    [SerializeField] private TMP_Text playerCountText;
    //[SerializeField] TMP_Text joinCodeText; Handled in relay manager

    [SerializeField] private Button startGame;
    [SerializeField] private Button exitLobby;

    private void StartGame()
    {
        SceneLoader.Instance.LoadScene("CharacterSelection");
    }

    private void ExitLobby()
    {
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startGame.onClick.AddListener(StartGame);
        exitLobby.onClick.AddListener(ExitLobby);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
