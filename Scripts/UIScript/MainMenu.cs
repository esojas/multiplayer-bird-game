using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject createGameButton;
    [SerializeField] Button joinGameButton;
    [SerializeField] TMP_Text feedbackText;
    [SerializeField] TMP_InputField joinInputText;
    [SerializeField] Button optionButton;
    [SerializeField] Button exitButton;

    private async void CreateGame()
    {
        createGameButton.SetActive(false);
        string joinCode = await RelayManager.Instance.CreateGame();
        MainUIManager.Instance.ShowLobbyScreen();
        MainLobby.Instance.SetJoinCode(joinCode);
    }

    private async void JoinGame()
    {
        if (joinInputText == null) return;

        string code = joinInputText.text.Trim(); // remove accidental spaces

        // Basic validation before even calling Relay
        if (string.IsNullOrEmpty(code))
        {
            feedbackText.text = "Please enter a join code!";
            return;
        }

        if (code.Length != 6) // Relay codes are always 6 characters
        {
            feedbackText.text = "Invalid code length!";
            return;
        }

        createGameButton.SetActive(false);

        // Let Relay validate the actual code
        try
        {
            await RelayManager.Instance.JoinGame(code);
            MainUIManager.Instance.ShowLobbyScreen();
        }
        catch (RelayServiceException e)
        {
            createGameButton.SetActive(true); 
            feedbackText.text = "Invalid or expired code!";
            Debug.Log(e);
        }
    }

    private void PressOption()
    {
        MainUIManager.Instance.ShowOptionScreen();
    }

    private void ExitGame()
    {
        if (Application.isEditor)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#endif
        }
        else
        {
            Application.Quit();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var player in PlayerNetworkData.AllPlayers)
        {
            if (player != null)
                Destroy(player.gameObject);
        }
        PlayerNetworkData.ClearAllPlayers();

        createGameButton.GetComponent<Button>().onClick.AddListener(CreateGame);
        joinGameButton.GetComponent<Button>().onClick.AddListener(JoinGame);
        optionButton.onClick.AddListener(PressOption);
        exitButton.onClick.AddListener(ExitGame);
    }

    private void OnEnable()
    {
        createGameButton.SetActive(true);
        joinInputText.text = "";
        

    }

    //private async void SettingUpCreateButton()
    //{
    //    createGameButton.SetActive(false);

    //    // Wait until RelayManager exists
    //    while (RelayManager.Instance == null)
    //    {
    //        await Task.Yield(); // wait one frame and check again
    //    }

    //    await RelayManager.Instance.AwaitAuthenticate();
    //    createGameButton.SetActive(true);
    //}

    // Update is called once per frame
    void Update()
    {
        
    }
}
