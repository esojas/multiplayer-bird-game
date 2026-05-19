using UnityEngine;

public static class LocalPlayerSettings
{
    public static event System.Action<float> OnSensitivityChanged;

    private const string KeyVolume = "settings_volume";
    private const string KeySensitivity = "settings_sensitivity";

    public static float Volume
    {
        get => PlayerPrefs.GetFloat(KeyVolume, .5f);
        set { PlayerPrefs.SetFloat(KeyVolume, value); PlayerPrefs.Save(); }
    }

    public static float Sensitivity
    {
        get => PlayerPrefs.GetFloat(KeySensitivity, .5f);
        set
        {
            PlayerPrefs.SetFloat(KeySensitivity, value);
            PlayerPrefs.Save();
            OnSensitivityChanged?.Invoke(value);
        }
    }
}