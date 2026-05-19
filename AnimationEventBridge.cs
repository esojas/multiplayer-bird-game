using UnityEngine;

public class AnimationEventBridge : MonoBehaviour
{

    private PlayerAttack playerAttack; // or whatever your attack script is
    private PlayerMovement playerMovement;


    private void Awake()
    {
        // Get the attack script from the root parent
        playerAttack = GetComponentInParent<PlayerAttack>();
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    // Animation event calls this
    public void OnAttackHit()
    {
        playerAttack.OnMeeleeAttackHit(); // forward it up to the real script
    }

    public void OnWingFlap()
    {
        playerMovement.OnWingFlapping();
    }

    public void OnTakingFootStep()
    {
        playerMovement.OnTakingStep();
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
