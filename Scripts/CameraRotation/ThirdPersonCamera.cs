using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public class ThirdPersonCamera : MonoBehaviour
{
    public static ThirdPersonCamera Instance { get; private set; }
    public float turnSpeed = 4.0f;
    private Transform target;
    [SerializeField] private float setCameraDistance;
    private CharacterData characterDataScript;
    private Transform cameraPosition;
    private float targetDistance;
    public float verticalOffset = 0f;
    public float minTurnAngle = -90.0f;
    public float maxTurnAngle = 0.0f;
    private float rotX;
    [SerializeField] private GameObject playerGameObject;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        turnSpeed = LocalPlayerSettings.Sensitivity;
        LocalPlayerSettings.OnSensitivityChanged += OnSensitivityChanged;
    }

    private void OnSensitivityChanged(float newSensitivity)
    {
        turnSpeed = newSensitivity;
    }

    private void OnDestroy()
    {
        LocalPlayerSettings.OnSensitivityChanged -= OnSensitivityChanged; // ✅ always unsubscribe
    }

    public void SetCameraTarget(Transform playerTarget)
    {
        target = playerTarget;
        characterDataScript = target.GetComponent<CharacterData>();

        if (characterDataScript != null) // ← check first ✅
        {
            cameraPosition = characterDataScript.playerCameraPosition;
            targetDistance = Vector3.Distance(cameraPosition.position, target.position) * 0.5f;
        }
        else
        {
            Debug.LogWarning("CharacterData not found!");
        }
    }

    void Update()
    {
        ControlCamera();
        
    }

    private void ControlCamera()
    {
        if (target == null || cameraPosition == null) return;

        float y = Input.GetAxis("Mouse X") * turnSpeed;
        rotX += Input.GetAxis("Mouse Y") * turnSpeed;
        rotX = Mathf.Clamp(rotX, minTurnAngle, maxTurnAngle);

        transform.eulerAngles = new Vector3(-rotX, transform.eulerAngles.y + y, 0);

        float heightOffset = cameraPosition.position.y - target.position.y;
        transform.position = target.position + new Vector3(0, heightOffset, 0) - (transform.forward * targetDistance);
    }

    public void DoFOV(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }
}
