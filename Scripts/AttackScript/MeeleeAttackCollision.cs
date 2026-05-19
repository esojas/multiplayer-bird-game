using Unity.Netcode;
using UnityEngine;

public class MeeleeAttackCollision : MonoBehaviour // ← back to MonoBehaviour, no need for NetworkBehaviour
{
    [SerializeField] private float lifetime = 0.2f;
    private int enemyLayer;
    private NetworkObject attackerNetworkObject;
    private float damage;

    public void Initialize(NetworkObject attacker, float damageAmount)
    {
        attackerNetworkObject = attacker;
        damage = damageAmount;
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[HIT] Collider hit: {other.gameObject.name} | Layer: {LayerMask.LayerToName(other.gameObject.layer)}");

        if (other.gameObject.layer != enemyLayer)
        {
            Debug.Log($"[SKIP] Wrong layer. Hit layer={LayerMask.LayerToName(other.gameObject.layer)}, expected={LayerMask.LayerToName(enemyLayer)}");
            return;
        }

        PlayerHealth playerHealthScript = other.GetComponentInParent<PlayerHealth>();
        Debug.Log($"[CHECK] PlayerHealth found: {playerHealthScript != null} on {other.gameObject.name}");

        if (playerHealthScript == null) return;

        Debug.Log($"[CHECK] Hit OwnerClientId: {playerHealthScript.OwnerClientId} | Attacker NetworkObject: {attackerNetworkObject?.OwnerClientId}");
        Debug.Log($"[CHECK] attackerNetworkObject is null: {attackerNetworkObject == null}");

        NetworkObject hitNetObj = playerHealthScript.GetComponent<NetworkObject>();
        Debug.Log($"[CHECK] Same object? {hitNetObj == attackerNetworkObject}");

        if (hitNetObj == attackerNetworkObject)
        {
            Debug.Log("[SKIP] Self hit blocked correctly");
            return;
        }


        playerHealthScript.RequestDamageServerRpc(damage);
    }


    private void Start()
    {
        enemyLayer = LayerMask.NameToLayer("EnemyLayer");
        Destroy(gameObject, lifetime);
    }
}
