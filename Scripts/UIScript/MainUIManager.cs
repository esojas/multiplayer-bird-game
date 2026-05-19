using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    public static MainUIManager Instance { get; private set; }

    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private GameObject lobbyScreen;
    [SerializeField] private GameObject optionScreen;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowOptionScreen()
    {
        lobbyScreen.SetActive(false);
        optionScreen.SetActive(true);
    }

    public void ShowMainMenuScreen()
    {
        mainMenuScreen.SetActive(true);
        lobbyScreen.SetActive(false);
        optionScreen.SetActive(false);
    }

    public void ShowLobbyScreen()
    {
        mainMenuScreen.SetActive(false);
        lobbyScreen.SetActive(true);
        optionScreen.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundManager.PlayTrack(0);
        SoundManager.StopAmbience();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
