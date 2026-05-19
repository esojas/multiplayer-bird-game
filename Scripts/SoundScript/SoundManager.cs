using System;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundType
{
    HIT,
    HURT,
    FLY,
    FOOTSTEP,
    PICKUPEGG,
    EXPLOSION,
    DEATH
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{

    [Header("SFX")]
    [SerializeField] private SoundList[] soundList;
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioClip[] musicTracks;   // your 2 bg music tracks
    [SerializeField] private AudioClip ambienceClip;


    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 👈 add this
            AudioListener.volume = LocalPlayerSettings.Volume;
        }
        else
        {
            Destroy(gameObject); // 👈 destroy duplicates
        }
    }

    public static void PlayTrack(int trackIndex)
    {
        if (Instance.musicTracks.Length == 0) return;
        Instance.musicSource.clip = Instance.musicTracks[trackIndex];
        Instance.musicSource.loop = true;
        Instance.musicSource.Play();
    }

    public static void PlayAmbience()
    {
        if (Instance.ambienceClip == null) return;
        Instance.ambienceSource.clip = Instance.ambienceClip;
        Instance.ambienceSource.loop = true;
        Instance.ambienceSource.Play();
    }

    public static void StopAmbience()
    {
        if (Instance.ambienceClip == null) return;
        Instance.ambienceSource.clip = Instance.ambienceClip;
        Instance.ambienceSource.Stop();
    }

    public static void ApplyVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    private void OnValidate()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++)
            soundList[i].name = names[i];
    }

    public static void PlaySound(SoundType sound, AudioSource source = null, float volume = 1)
    {
        if (Instance == null)
        {
            Debug.LogWarning("SoundManager: No instance found!");
            return;
        }

        SoundList soundList = Instance.soundList[(int)sound];
        AudioClip[] clips = soundList.sounds;

        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"SoundManager: No clips assigned for {sound}");
            return;
        }

        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        float finalVolume = volume * soundList.volume;
        AudioSource targetSource = source != null ? source : Instance.audioSource;

        targetSource.outputAudioMixerGroup = soundList.mixer;

        if (sound == SoundType.FLY)
        {
            targetSource.clip = randomClip;
            targetSource.volume = finalVolume;
            targetSource.Play();
        }
        else
        {
            targetSource.PlayOneShot(randomClip, finalVolume);
        }
    }

}

[Serializable]
public struct SoundList
{
    [HideInInspector] public string name;
    [Range(0, 1)] public float volume;
    public AudioMixerGroup mixer;
    public AudioClip[] sounds;
}
