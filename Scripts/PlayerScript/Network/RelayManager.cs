using System.Net.Security;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    [SerializeField] TMP_Text joinCodeText;
    [SerializeField] TMP_InputField joinInputText;
    [SerializeField] GameObject buttons;
    [SerializeField] GameObject networkManager;

    private UnityTransport transport;
    private const int maxPlayers = 4;


    private async void Awake()
    {
        Instance = this;

        //Retrieve the Unity transport used by the NetworkManager
        transport = networkManager.GetComponent<UnityTransport>();

        await Authenticate();

    }

    public void Cleanup()
    {
        Destroy(gameObject);
    }

    //public async Task AwaitAuthenticate()
    //{
    //    await Authenticate();
    //}

    private static async Task Authenticate()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async Task<string> CreateGame()
    {

        Allocation a = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

        transport.SetHostRelayData(
            a.RelayServer.IpV4,
            (ushort)a.RelayServer.Port,
            a.AllocationIdBytes,
            a.Key,
            a.ConnectionData
        );

        NetworkManager.Singleton.StartHost();
        return joinCode;
    }

    public async Task JoinGame(string joinInputText)
    {

        JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(joinInputText);

        transport.SetClientRelayData(
        a.RelayServer.IpV4,
        (ushort)a.RelayServer.Port,
        a.AllocationIdBytes,
        a.Key,
        a.ConnectionData,
        a.HostConnectionData
        );

        NetworkManager.Singleton.StartClient();
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
