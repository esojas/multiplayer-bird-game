using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static MainNestScript;

public class EggAmountScript : MonoBehaviour
{

    [SerializeField] private Camera cam;

    //[SerializeField] private List<GameObject> eggs = new List<GameObject>();

    [SerializeField] private TMP_Text interactText;




    public void UpdateCanvasPosition()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
    }

    public void UpdateText(int eggs)
    {
        MainNestScript nestScript = GetComponentInParent<MainNestScript>();
        if (nestScript != null && nestScript.IsMyNest()) // ✅ only update your own score
        {
            Debug.Log($"ClientId: {PlayerNetworkData.Instance?.OwnerClientId} eggs: {eggs}");
            PlayerNetworkData.Instance?.SetEggsOwned(eggs);
        }

        interactText.text = "Eggs Left: " + eggs;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactText = GetComponentInChildren<TMP_Text>();
        cam = ThirdPersonCamera.Instance.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (cam == null && ThirdPersonCamera.Instance != null)
        {
            cam = ThirdPersonCamera.Instance.GetComponent<Camera>();
        }

        if (cam != null)
            UpdateCanvasPosition();
    }
}
