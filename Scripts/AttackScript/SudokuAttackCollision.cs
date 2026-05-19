using Unity.Netcode;
using UnityEngine;

public class SudokuAttackCollision : MonoBehaviour
{

    [SerializeField] private float lifetime = 0.2f;
    private int enemyLayer;
    private NetworkObject attackerNetworkObject;

    public void Initialize(NetworkObject attacker)
    {
        attackerNetworkObject = attacker;
    }
    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.layer != enemyLayer)
        {
            return;
        }

        PlayerHealth playerHealthScript = other.GetComponentInParent<PlayerHealth>();


        if (playerHealthScript == null) return;

        
        NetworkObject hitNetObj = playerHealthScript.GetComponent<NetworkObject>();

        Debug.Log("[DAMAGE] Applying damage");
        playerHealthScript.RequestDamageServerRpc(100000);
    }
    private void Start()
    {
        enemyLayer = LayerMask.NameToLayer("EnemyLayer");
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
