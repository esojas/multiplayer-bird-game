using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    public static HealthBarScript Instance { get; private set; }
    public Slider slider;

    private void Awake()
    {
        Instance = this;
    }

    public void SetHealth(float health)
    {
        slider.value = health;
    }

    public void SetMaxHealth(float health)
    {
        slider.maxValue = health;
        slider.value = health;
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
