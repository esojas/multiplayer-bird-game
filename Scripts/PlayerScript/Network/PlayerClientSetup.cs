using Unity.Netcode;
using UnityEngine;

public class PlayerClientSetup : NetworkBehaviour
{
    [SerializeField] private PlayerMovement playerMovementScript;
    [SerializeField] private PlayerInteraction playerInteractionScript;
    [SerializeField] private CharacterData characterDataScript;

    public override void OnNetworkSpawn()
    {
        if (IsOwner && IsClient)
        {
            ThirdPersonCamera.Instance.SetCameraTarget(transform);
            characterDataScript.SetCamera(ThirdPersonCamera.Instance.GetComponent<Camera>());
            playerInteractionScript.enabled = true;
            playerMovementScript.enabled = true;

        }
    }

    private void Awake()
    {
        playerInteractionScript.enabled = false;
        playerMovementScript.enabled = false;
        //characterDataScript.enabled = false;
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
