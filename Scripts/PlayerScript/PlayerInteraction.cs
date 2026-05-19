using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Analytics.IAnalytic;
using static UnityEngine.UI.Image;

public class PlayerInteraction : MonoBehaviour
{
    private BirdTypes birdTypes;
    private Camera cam;
    [SerializeField] private float maxRayDistance = 10f;

    public float amtEggsHold;
    public bool isEggInNest = false;
    GameObject nest;
    GameObject enemyNest;

    private CharacterData characterData;
    private EggHolder eggHolder;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        characterData = GetComponent<CharacterData>();
        birdTypes = characterData.birdTypes;
        eggHolder = GetComponent<EggHolder>();
    }

    private void TakeEgg(RaycastHit hitData)
    {
        MainNestScript enemyNestScript = hitData.collider.GetComponent<MainNestScript>();

        // Check server-side if nest actually has eggs
        if (enemyNestScript.EggCount <= 0)
        {
            Debug.Log("Nest is empty!");
            return;
        }

        enemyNestScript.TakeEggNestServerRpc(); 
        isEggInNest = false;
    }

    private void PuttEgg(RaycastHit hitData)
    {
        MainNestScript mainNestScript = hitData.collider.GetComponent<MainNestScript>();
        GameObject eggToReturn = eggHolder.PeekEgg();
        if (eggToReturn == null)
        {
            Debug.LogError("PeekEgg returned null!");
            return;
        }
        NetworkObject netObj = eggToReturn.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("Egg has no NetworkObject!");
            return;
        }
        mainNestScript.PutEggNestServerRpc(netObj.NetworkObjectId);
    }

    private void InteractionInput()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hitData;
        int nestMask = LayerMask.GetMask("Nest");
        int droppedEggMask = LayerMask.GetMask("DroppedEgg");

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Physics.Raycast(ray, out hitData, maxRayDistance, nestMask))
            {
                MainNestScript nestScript = hitData.collider.GetComponent<MainNestScript>();
                if (nestScript == null) return;

                if (nestScript.IsMyNest())
                {
                    if (!eggHolder.IsEmpty)
                    {
                        PuttEgg(hitData);
                        SoundManager.PlaySound(SoundType.PICKUPEGG, audioSource);
                    }
                    else
                        Debug.Log("No eggs to put!");
                }
                else if (nestScript.IsEnemyNest())
                {
                    if (!eggHolder.IsFull)
                    {  
                        TakeEgg(hitData);
                        SoundManager.PlaySound(SoundType.PICKUPEGG, audioSource);
                    }
                    else
                        Debug.Log("Can't carry more eggs!");
                }
            }
            else if (Physics.Raycast(ray, out hitData, maxRayDistance, droppedEggMask))
            {
                DroppedEgg droppedEgg = hitData.collider.GetComponent<DroppedEgg>();
                if (droppedEgg != null && !eggHolder.IsFull)
                    droppedEgg.TakeEggServerRpc();
                else
                    Debug.Log("Can't carry more eggs!");
            }
        }
    }

    //[ClientRpc]
    //private void PlayPickEggSoundClientRpc()
    //{
    //    SoundManager.PlaySound(SoundType.PICKUPEGG, audioSource);
    //}

    public void SetCameraForInteraction(Camera camGiven) 
    {
        cam = camGiven;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = characterData.playerCamera;
        //cam = ThirdPersonCamera.Instance.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(cam.transform.position, cam.transform.forward * maxRayDistance, Color.blue);
        InteractionInput();
        //BringEgg();
    }
}
