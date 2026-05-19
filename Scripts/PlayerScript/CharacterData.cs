using UnityEngine;

public class CharacterData : MonoBehaviour
{
    public Camera playerCamera { get; private set; }
    public BirdTypes birdTypes;
    public Transform playerModel;
    public Transform playerCameraPosition; // gets the camera_pos inside bird child
    public Animator animator;

    public void SetCamera(Camera cam)
    {
        // You can add validation here
        if (cam == null)
        {
            Debug.LogWarning("Trying to set null camera!");
            return;
        }
        playerCamera = cam;
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
