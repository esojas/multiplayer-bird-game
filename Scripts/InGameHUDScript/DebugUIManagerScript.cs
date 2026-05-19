using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class DebugUIManagerScript : MonoBehaviour
{
    [SerializeField] private Button startHostButton;

    [SerializeField] private Button startServerButton;

    [SerializeField] private Button startClientButton;

    [SerializeField] private TextMeshProUGUI playersInGameText;

    [SerializeField] private TextMeshProUGUI debugInGameText;

    private void Awake()
    {
        Cursor.visible = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startHostButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartHost())
            {
                debugInGameText.text = "Host Starting...";
            }
            else
            {
                debugInGameText.text = "Host Failed...";
            }
        });

        startClientButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartClient())
            {
                debugInGameText.text = "Client Starting...";
            }
            else
            {
                debugInGameText.text = "Client Failed...";
            }
        });

        startServerButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
            {
                debugInGameText.text = "Server Starting...";
            }
            else
            {
                debugInGameText.text = "Server Failed...";
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        //playersInGameText.text = $"Players in game are: {PlayerInputManager.instance.name}";
    }
}
