using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAttack : NetworkBehaviour
{
    private float maxAttackRange;

    private Camera cam;
    private float damage;
    [SerializeField] private GameObject attackPoint;
    [SerializeField] private GameObject meeleeAttackPrefab;
    [SerializeField] private GameObject sudokuAttackPrefab;
    [SerializeField] private GameObject sudokuVFXPrefab;
    private BirdTypes birdTypes;

    private CharacterData characterData;
    private AudioSource audioSource;
    private void Awake()
    {
        characterData = GetComponent<CharacterData>();
        audioSource = GetComponent<AudioSource>();
        cam = Camera.main;
        birdTypes = characterData.birdTypes;
    }

    void Start()
    {
        maxAttackRange = birdTypes.attackRange;
        damage = birdTypes.attackAmt;
    }

    public void OnMeeleeAttackHit()
    {
        if (!IsOwner) return;
        MeeleeAttack();
    }

    public void OnSudokuAttack()
    {
        if (!IsOwner) return;
        SudokuAttack();
    }

    private void SudokuAttack()
    {
        PlaySudokuVFXServerRpc(transform.position);
        GameObject sudokuInstantiate = Instantiate(sudokuAttackPrefab, transform.position, Quaternion.identity);

        sudokuInstantiate.transform.localScale = gameObject.transform.localScale;

        SphereCollider sphereCollider = sudokuInstantiate.GetComponent<SphereCollider>();
        sphereCollider.enabled = false; // ← disable BEFORE physics can fire

        sudokuInstantiate.GetComponent<SudokuAttackCollision>().Initialize(
            NetworkManager.Singleton.LocalClient.PlayerObject
        );

        sphereCollider.enabled = true; // ← now safe to enable
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlaySudokuVFXServerRpc(Vector3 position)
    {
        PlaySudokuVFXClientRpc(position);
    }

    [ClientRpc] 
    private void PlaySudokuVFXClientRpc(Vector3 position)
    {
        GameObject vfx = Instantiate(sudokuVFXPrefab, position, Quaternion.identity);
        vfx.transform.localScale = gameObject.transform.localScale;
        Destroy(vfx, sudokuVFXPrefab.GetComponent<ParticleSystem>().main.duration);

        GameObject soundObj = new GameObject("ExplosionSound");
        soundObj.transform.position = position;
        AudioSource tempSource = soundObj.AddComponent<AudioSource>();
        tempSource.spatialBlend = 1f; 
        SoundManager.PlaySound(SoundType.EXPLOSION, tempSource);
        Destroy(soundObj, 3f);
    }


    private void MeeleeAttack()
    {
        Vector3 shootPoint = attackPoint.transform.position;
        RaycastHit hit;
        Ray crosshairRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;

        if (Physics.Raycast(crosshairRay, out hit, maxAttackRange))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = crosshairRay.GetPoint(75);
        }

        Vector3 direction = targetPoint - shootPoint;
        Quaternion attackRotation = Quaternion.LookRotation(direction);

        GameObject attackInstantiate = Instantiate(meeleeAttackPrefab, shootPoint, attackRotation);

        BoxCollider cubeCollider = attackInstantiate.GetComponent<BoxCollider>();
        cubeCollider.enabled = false; // ← disable BEFORE physics can fire

        attackInstantiate.GetComponent<MeeleeAttackCollision>().Initialize(
            NetworkManager.Singleton.LocalClient.PlayerObject, damage
        );

        cubeCollider.size = new Vector3(1, 1, maxAttackRange);
        cubeCollider.center = new Vector3(0, 0, maxAttackRange / 2);
        cubeCollider.enabled = true;
        PlayMeeleeHitServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayMeeleeHitServerRpc()
    {
        PlayMeeleeHitClientRpc();
    }

    [ClientRpc] 
    private void PlayMeeleeHitClientRpc()
    {
        SoundManager.PlaySound(SoundType.HIT, audioSource);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
