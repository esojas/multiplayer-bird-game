using UnityEngine;
using UnityEngine.UI;

public class OptionMenu : MonoBehaviour
{
    [SerializeField] Slider sensitivitySlider;
    [SerializeField] Slider volumeSlider;
    [SerializeField] Button closeOptionMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sensitivitySlider.value = LocalPlayerSettings.Sensitivity;
        volumeSlider.value = LocalPlayerSettings.Volume;

        closeOptionMenu.onClick.AddListener(CloseOptionMenu);

        sensitivitySlider.onValueChanged.AddListener(value =>
        {
            LocalPlayerSettings.Sensitivity = value;
        });

        volumeSlider.onValueChanged.AddListener(value =>
        {
            LocalPlayerSettings.Volume = value;
            SoundManager.ApplyVolume(value);
        });
    }

    private void CloseOptionMenu()
    {
        gameObject.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
