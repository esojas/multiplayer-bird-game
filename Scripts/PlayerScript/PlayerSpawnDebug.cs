using UnityEngine;

public class PlayerSpawnDebug : MonoBehaviour
{
    void OnDrawGizmos()
    {

        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(transform.position, new Vector3(1f, 2f, 1f));

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(1f, 2f, 1f));


        Gizmos.DrawLine(transform.position,
                        transform.position + transform.forward * 2f);
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
