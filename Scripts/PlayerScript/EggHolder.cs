using System.Collections.Generic;
using UnityEngine;

public class EggHolder : MonoBehaviour
{
    [SerializeField] private List<Transform> holdPoints = new List<Transform>();

    private List<GameObject> heldEggs = new List<GameObject>();

    public int MaxCapacity => holdPoints.Count;
    public int CurrentCount => heldEggs.Count;
    public bool IsFull => heldEggs.Count >= holdPoints.Count;
    public bool IsEmpty => heldEggs.Count == 0;

    public void HoldEgg(GameObject egg)
    {
        if (IsFull) return;

        Transform holdPoint = holdPoints[heldEggs.Count];

        // ✅ Don't parent, just move to position
        egg.transform.position = holdPoint.position;
        egg.transform.rotation = holdPoint.rotation;

        heldEggs.Add(egg);
    }

    public GameObject ReleaseEgg()
    {
        if (IsEmpty) return null;

        GameObject egg = heldEggs[heldEggs.Count - 1];
        egg.transform.SetParent(null);
        heldEggs.RemoveAt(heldEggs.Count - 1);
        return egg;
    }

    public void ClearAll()
    {
        heldEggs.Clear();
    }

    public GameObject PeekEgg()
    {
        if (IsEmpty) return null;
        return heldEggs[heldEggs.Count - 1];
    }

    private void Update()
    {
        for (int i = 0; i < heldEggs.Count; i++)
        {
            if (heldEggs[i] != null)
            {
                heldEggs[i].transform.position = holdPoints[i].position;
                heldEggs[i].transform.rotation = holdPoints[i].rotation;
            }
        }
    }
}
