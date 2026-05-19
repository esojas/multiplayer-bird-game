using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    private BirdTypes birdTypes;
    private float horizontalInput;
    private float verticalInput;
    private bool flyingInput;
    private bool meeleeAttackInput;
    private bool sprintInput;
    private bool sprintInputLetGo;
    private bool flightToggleInput;
    private bool sudokuAttackInput;
    private bool hasExploded = false;
    private bool isMissile = false;
    private bool missileMovementFinished = false;
    private bool flightToggle = false;
    private bool isSprinting = false;

    [SerializeField] private float currentSprintMultiplier;
    [SerializeField] private float flightUpForce;
    [SerializeField] private CharacterController characterController;

    private Animator animator;
    private float totalStamina;
    private string currentAnimation = "";
    private StaminaBarScript staminaBarScript;
    private float sprintSpeed;
    private float walkSpeed;
    private float flightSpeed;
    private Coroutine staminaCoroutine;
    private string birdName;

    private Transform cam_Transform;
    private Camera cam;
    public GameObject cubeColliderAttack;
    //private ThirdPersonCamera thirdPersonCameraScript;
    Vector3 velocity;
    private Transform playerModel;
    private CharacterData characterData;
    private PlayerAttack playerAttackScript;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        characterData = GetComponent<CharacterData>();  
        birdTypes = characterData.birdTypes;
        characterController = GetComponent<CharacterController>();
        animator = characterData.animator;
        playerModel = characterData.playerModel;
        playerAttackScript = GetComponent<PlayerAttack>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        staminaBarScript = StaminaBarScript.Instance;
        cam = characterData.playerCamera;
        cam_Transform = cam.transform;

        birdName = birdTypes.name.ToLower();
        totalStamina = birdTypes.staminaAmt;
        SetPlayerMaxStaminaBar(staminaBarScript);
        walkSpeed = birdTypes.walkSpeed;
        flightSpeed = birdTypes.flightSpeed;
        sprintSpeed = birdTypes.flightSpeed * currentSprintMultiplier;
    }

    public void SetPlayerMaxStaminaBar(StaminaBarScript staminaBarScriptPrefab)
    {
        staminaBarScriptPrefab.SetMaxStamina(birdTypes.staminaAmt);
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseMenuManager.Instance.togglePause) return;
        if (isMissile) return;
        staminaBarScript.SetStamina(totalStamina);

        MyInput();

        HandleAttackInput();

        HandleAnimation();
        FlightToggle();
        if (flightToggle)
        {
            HandleSudokuInput();
            FlyingMovements();
        }
        else
        {
            ApplyGravity();
            LandMovements();
        }

    }

    private void HandleSudokuInput()
    {
        if (sudokuAttackInput && !isMissile) StartMissileAttack();
    }

    public void StartMissileAttack()
    {
        StartCoroutine(MissileMovement());
    }

    private IEnumerator MissileMovement()
    {

        Vector3 camForward = cam_Transform.forward;
        camForward.Normalize();

        isMissile = true;
        float duration = 1f;
        float speed = 60f;
        float elapsed = 0f;

        CharacterController cc = GetComponent<CharacterController>();

        while (elapsed < duration)
        {
            cc.Move(camForward * speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isMissile = false;
        hasExploded = false;
        playerAttackScript.OnSudokuAttack();
    }

    private void OnControllerColliderHit(ControllerColliderHit other)
    {
        int groundLayer = 9;
        int treeLayer = 10;
        int enemyLayer = 7;

        if ((other.gameObject.layer == groundLayer || other.gameObject.layer == treeLayer || other.gameObject.layer == enemyLayer) && isMissile && !hasExploded) // Dont forget to add enemy layer later
        {
            Debug.LogError("SUDOKU!!!");
            hasExploded = true;

            playerAttackScript.OnSudokuAttack();
        }

    }

    private void HandleAttackInput()
    {
        if (meeleeAttackInput)
        {
            ChangeCurrentAnimation($"{birdName}_fly_attack");
        }
    }

    private void HandleAnimation()
    {
        if (currentAnimation == $"{birdName}_fly_attack") return;

        if (flightToggle)
        {
            ChangeCurrentAnimation($"{birdName}_fly");
        }
        else
        {
            Vector3 horivertmovement = new Vector3(verticalInput, 0, horizontalInput);
            if (horivertmovement != new Vector3(0,0,0)) ChangeCurrentAnimation($"{birdName}_walking");
            else { ChangeCurrentAnimation($"{birdName}_idle"); }
        }
    }

    private void ApplyGravity()
    {

        float gravityForce = -9.81f;
        float mass = 1f;

        float accelaration = gravityForce / mass;

        velocity.y += gravityForce * Time.deltaTime;

        characterController.Move(velocity * Time.deltaTime);

    }

    public void ChangeCurrentAnimation(string animation, float crossFade = 0.2f, float time = 0)
    {
        if (time > 0)
        {
            StartCoroutine(Wait());
        }
        else
        {
            Validate();
        }

        IEnumerator Wait()
        {
            yield return new WaitForSeconds(time - crossFade);
            Validate();
        }


        void Validate()
        {
            if (currentAnimation != animation)
            {
                currentAnimation = animation;

                if (currentAnimation == "")
                {
                    HandleAnimation();
                }
                else
                {
                    animator.CrossFade(animation, crossFade);
                }
            }
        }
    }

    private void SprintCameraFOV()
    {
        ThirdPersonCamera.Instance.DoFOV(95);
    }

    private void WalkCameraFOV()
    {
        ThirdPersonCamera.Instance.DoFOV(85);
    }

    private void MyInput()
    {
        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
        flyingInput = Input.GetKey(KeyCode.Space);
        sprintInput = Input.GetKey(KeyCode.LeftShift);
        sprintInputLetGo = Input.GetKeyUp(KeyCode.LeftShift);
        flightToggleInput = Input.GetKeyDown(KeyCode.Q);
        meeleeAttackInput = Input.GetMouseButtonDown(0);
        sudokuAttackInput = Input.GetKeyDown(KeyCode.Alpha1);
    }

    private bool FlightToggle()
    {
        if (flightToggleInput)
        {
            flightToggle = !flightToggle;
        }
        return flightToggle;
    }

    private void LandMovements()
    {

        WalkCameraFOV();

        Vector3 camForward = cam_Transform.forward;
        Vector3 camRight = cam_Transform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 movement = (camForward * verticalInput + camRight * horizontalInput);
        movement = Vector3.ClampMagnitude(movement, 1);

        Quaternion targetRotation = Quaternion.Euler(
        0,                              // X always 0, never tilt
        cam_Transform.eulerAngles.y,    // Y follows camera horizontal
        0                               // Z always 0
        );

        //Quaternion targetRotation = Quaternion.LookRotation(camForward);
        playerModel.rotation = Quaternion.Slerp(
             playerModel.rotation,
             targetRotation,
             10f * Time.deltaTime
         );


        if (characterController.isGrounded && velocity.y == 0)
        {
            velocity.y = -2f;
        }

        if (sprintInput && totalStamina > 0)
        {
            SprintCameraFOV();
            characterController.Move(movement * sprintSpeed * Time.deltaTime);
        }

        if (sprintInput && !isSprinting && totalStamina > 0)
        {
            if (staminaCoroutine != null) StopCoroutine(staminaCoroutine);
            IsSprinting();
            staminaCoroutine = StartCoroutine(StaminaRoutine());
        }
        else if (sprintInputLetGo)
        {
            StopSprinting();
            if (staminaCoroutine != null) StopCoroutine(staminaCoroutine);
            staminaCoroutine = StartCoroutine(StaminaRegenRoutine());
        }


        characterController.Move(movement * walkSpeed * Time.deltaTime);
    }

    private void FlightUp()
    {
        if (flyingInput)
        {

            Vector3 movement = new Vector3(0, flightUpForce, 0);

            characterController.Move(movement*Time.deltaTime);

        }
    }

    private void IsSprinting()
    {
        isSprinting = true;
    }

    private void StopSprinting()
    {
        isSprinting = false;
    }

    private void FlyingMovements()
    {

        WalkCameraFOV();

        Vector3 camForward = cam_Transform.forward;
        Vector3 camRight = cam_Transform.right;
        
        Vector3 movement = (camForward * verticalInput + camRight * horizontalInput);
        movement = Vector3.ClampMagnitude(movement, 1);
        velocity.y = 0;

        //if (verticalInput != 0 || horizontalInput != 0) 
        //{
        RotateGameObject();


        if (sprintInput && totalStamina > 0)
        {
            SprintCameraFOV();
            characterController.Move(movement * sprintSpeed * Time.deltaTime);

            //transform.Translate(movement * sprintSpeed * Time.deltaTime, Space.World);
        }

        if (sprintInput && !isSprinting && totalStamina > 0)
        {
            if (staminaCoroutine != null) StopCoroutine(staminaCoroutine);
            IsSprinting();
            staminaCoroutine = StartCoroutine(StaminaRoutine());
        }
        else if (sprintInputLetGo)
        {
            StopSprinting();
            if (staminaCoroutine != null) StopCoroutine(staminaCoroutine);
            staminaCoroutine = StartCoroutine(StaminaRegenRoutine());
        }

        //}

        FlightUp();


        Vector3 horivertmovement = new Vector3(verticalInput, 0, horizontalInput);
        if (horivertmovement != new Vector3(0, 0, 0)) characterController.Move(movement * flightSpeed * Time.deltaTime);

        ResetRotationGameObject(); // Resets object rotation when idle
    }

    private void RotateGameObject() 
    {
        playerModel.rotation = Quaternion.Slerp(
            playerModel.rotation,
            cam_Transform.rotation,
            10f * Time.deltaTime
        );
    }

    private void ResetRotationGameObject()
    {
        Vector3 euler = playerModel.eulerAngles;
        euler.x = 0f;

        Quaternion target = Quaternion.Euler(euler);

        playerModel.rotation = Quaternion.Slerp(
            playerModel.rotation,
            target,
            10f * Time.deltaTime
        );
    }

    private void UpdateStaminaGauge(float currentStamina)
    {
        if (totalStamina - currentStamina <= 0)
        {
            StopSprinting();
        }
        totalStamina += currentStamina;
    }

    IEnumerator StaminaRoutine()
    {
        while (isSprinting && totalStamina > 0)
        {
            UpdateStaminaGauge(-20f);
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator StaminaRegenRoutine()
    {
        while (!isSprinting && totalStamina < birdTypes.staminaAmt)
        {
            UpdateStaminaGauge(20f);
            yield return new WaitForSeconds(1f);
        }
    }

    public void OnWingFlapping()
    {
        PlayWingFlapClientRpc();
    }

    [ClientRpc]
    private void PlayWingFlapClientRpc()
    {
        SoundManager.PlaySound(SoundType.FLY, audioSource);
    }

    public void OnTakingStep()
    {
        PlayTakingStepClientRpc();
    }

    [ClientRpc]
    private void PlayTakingStepClientRpc()
    {
        SoundManager.PlaySound(SoundType.FOOTSTEP, audioSource);
    }
}