using UnityEngine;
using UnityEngine.UI;

public class StaminaBarScript : MonoBehaviour
{
    public static StaminaBarScript Instance { get; private set; }
    public Slider slider;

    private void Awake()
    {
        Instance = this;
    }

    public void SetStamina(float stamina)
    {
        slider.value = stamina;
    }

    public void SetMaxStamina(float stamina)
    {
        slider.maxValue = stamina;
        slider.value = stamina;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider = GetComponent<Slider>();

        if (slider == null) Debug.LogWarning("Slider is missing!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
