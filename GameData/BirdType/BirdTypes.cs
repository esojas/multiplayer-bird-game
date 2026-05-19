using UnityEngine;

[CreateAssetMenu(fileName = "BirdTypes", menuName = "Scriptable Objects/BirdTypes")]
public class BirdTypes : ScriptableObject
{
    public float healthAmt;
    public float staminaAmt;
    public float flightSpeed;
    public float walkSpeed;
    public float attackAmt;
    //public float attackSpeed;
    public float attackRange;
    public int amtEggsThatCanHold; // The amount of eggs the bird can carry

}
