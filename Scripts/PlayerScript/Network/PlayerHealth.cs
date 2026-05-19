using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHealth : NetworkBehaviour
{
    private CharacterData characterData;

    [SerializeField] private GameObject vfxPrefab;

    [SerializeField] private GameObject droppedEggPrefab;
    [SerializeField] private Renderer playerRend;
    [SerializeField] private Color hitFlashColour;
    [SerializeField] private float flashDuration;

    private AudioSource audioSource;
    private Color originalColour;

    // Just declare it with a temp value
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(
        0f,                                          // temporary default
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        characterData = GetComponent<CharacterData>();
    }

    private void Start()
    {
        originalColour = playerRend.material.color;
    }

    public override void OnNetworkSpawn()
    {
        currentHealth.OnValueChanged += OnHealthChanged;

        if (IsServer)
            currentHealth.Value = characterData.birdTypes.healthAmt;

        if (IsOwner)
            HealthBarScript.Instance.SetHealth(currentHealth.Value);
    }

    private void OnHealthChanged(float oldValue, float newValue)
    {
        if (IsOwner)
            HealthBarScript.Instance.SetHealth(newValue);
    }

    private void TakeDamage(float damage)
    {
        if (!IsServer) return;

        if (currentHealth.Value > 0f)
        {
            currentHealth.Value = Mathf.Max(0f, currentHealth.Value - damage);
            if (currentHealth.Value == 0f)
                OnCharacterDeath();
        }
    }

    [ClientRpc]
    private void SpawnVFXClientRpc(Vector3 position)
    {
        GameObject vfx = Instantiate(vfxPrefab, position, Quaternion.identity);
        vfx.transform.localScale = gameObject.transform.localScale;
        Destroy(vfx, vfxPrefab.GetComponent<ParticleSystem>().main.duration);
    }

    [ClientRpc]
    private void NotifyOwnerDeathClientRpc(ClientRpcParams rpcParams = default)
    {
        PauseMenuManager.Instance.OnPlayerDied();
    }

    [ClientRpc]
    private void NotifyOwnerRespawnClientRpc(ClientRpcParams rpcParams = default)
    {
        PauseMenuManager.Instance.OnPlayerRespawned();
    }

    [ClientRpc]
    private void PlayOwnerDeathSoundClientRpc()
    {
        GameObject soundObj = new GameObject("DeathSound");
        soundObj.transform.position = transform.position;
        AudioSource tempSource = soundObj.AddComponent<AudioSource>();
        tempSource.spatialBlend = 1f;
        SoundManager.PlaySound(SoundType.DEATH, tempSource);
        Destroy(soundObj, 3f);
    }

    public void NotifyClientOfRespawn(ulong targetClientId)
    {
        NotifyOwnerRespawnClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { targetClientId } }
        });
    }

    private void OnCharacterDeath()
    {
        PlayOwnerDeathSoundClientRpc();
        DropEggOnDeath();
        SpawnVFXClientRpc(gameObject.transform.position);
        NotifyOwnerDeathClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } }
        });
        Invoke(nameof(DestroyCharacter), .01f);
    }

    private void DropEggOnDeath()
    {
        EggHolder eggHolder = GetComponent<EggHolder>();
        if (eggHolder == null || eggHolder.IsEmpty) return;

        // Keep popping eggs until holder is empty
        while (!eggHolder.IsEmpty)
        {
            GameObject egg = eggHolder.PeekEgg();
            if (egg == null) break;

            NetworkObject eggNetObj = egg.GetComponent<NetworkObject>();

            eggNetObj.ChangeOwnership(NetworkManager.ServerClientId);
            egg.transform.SetParent(null);

            // Offset each dropped egg slightly so they don't all stack on the same spot
            Vector3 dropPosition = transform.position + Random.insideUnitSphere * 0.5f;
            dropPosition.y = transform.position.y;

            GameObject dropped = Instantiate(droppedEggPrefab, dropPosition, Quaternion.identity);
            NetworkObject droppedNetObj = dropped.GetComponent<NetworkObject>();
            droppedNetObj.Spawn();

            dropped.GetComponent<DroppedEgg>().SetEgg(eggNetObj.NetworkObjectId);
            HideEggClientRpc(eggNetObj.NetworkObjectId);

            eggHolder.ReleaseEgg();
        }
    }

    [ClientRpc]
    private void HideEggClientRpc(ulong eggNetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(eggNetId, out NetworkObject netObj))
        {
            foreach (Renderer rend in netObj.GetComponentsInChildren<Renderer>())
                rend.enabled = false;
        }
    }

    private void DestroyCharacter()
    {
        GetComponent<NetworkObject>().Despawn();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestDamageServerRpc(float damage)
    {
        TakeDamage(damage);
        FlashHitClientRpc(); 
    }

    [ClientRpc]
    private void FlashHitClientRpc()
    {
        RunFlash();
        SoundManager.PlaySound(SoundType.HURT,audioSource);
    }

    private IEnumerator DoFlashHit()
    {
        playerRend.material.color = hitFlashColour;
        yield return new WaitForSeconds(flashDuration);
        playerRend.material.color = originalColour;
    }

    private void RunFlash()
    {
        StopAllCoroutines();
        StartCoroutine(DoFlashHit());
    }
}
