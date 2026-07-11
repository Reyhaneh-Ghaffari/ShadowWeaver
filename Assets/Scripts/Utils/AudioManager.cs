using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;

    [HideInInspector] public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance => instance;

    [Header("Audio Settings")]
    [SerializeField] private Sound[] sounds;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private float masterVolume = 1f;
    private float sfxVolume = 1f;
    private float musicVolume = 1f;
    private bool isMuted = false;
    private bool isMusicEnabled = true;
    private bool isSFXEnabled = true;

    private Dictionary<string, Sound> soundDict = new Dictionary<string, Sound>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;

            soundDict[sound.name] = sound;
        }

        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        // بارگذاری تنظیمات
        LoadAudioSettings();
    }

    private void Start()
    {
        PlayMusic("Background");
    }

    // ===== پخش صداها =====
    public void PlaySFX(string name)
    {
        if (isMuted || !isSFXEnabled) return;

        if (soundDict.TryGetValue(name, out Sound sound))
        {
            sound.source.volume = sound.volume * sfxVolume * masterVolume;
            sound.source.pitch = sound.pitch;
            sound.source.Play();
        }
        else
        {
            Debug.LogWarning($"Sound '{name}' not found!");
        }
    }

    public void PlayMusic(string name)
    {
        if (soundDict.TryGetValue(name, out Sound sound))
        {
            musicSource.clip = sound.clip;
            musicSource.volume = sound.volume * musicVolume * masterVolume;
            musicSource.loop = true;
            musicSource.pitch = sound.pitch;

            if (isMusicEnabled && !isMuted)
                musicSource.Play();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (isMusicEnabled && !isMuted)
            musicSource.Play();
    }

    // ===== تنظیمات حجم صدا =====
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        SaveAudioSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        SaveAudioSettings();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
        SaveAudioSettings();
    }

    // ===== فعال/غیرفعال کردن صداها =====
    public void SetMute(bool mute)
    {
        isMuted = mute;
        AudioListener.pause = mute;
        SaveAudioSettings();
    }

    public void SetMusicEnabled(bool enabled)
    {
        isMusicEnabled = enabled;
        if (musicSource != null)
        {
            if (enabled && !isMuted)
                musicSource.Play();
            else
                musicSource.Pause();
        }
        SaveAudioSettings();
    }

    public void SetSFXEnabled(bool enabled)
    {
        isSFXEnabled = enabled;
        SaveAudioSettings();
    }

    // ===== دریافت وضعیت‌ها =====
    public bool IsMuted() => isMuted;
    public bool IsMusicEnabled() => isMusicEnabled;
    public bool IsSFXEnabled() => isSFXEnabled;
    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;

    // ===== به‌روزرسانی حجم صداها =====
    private void UpdateAllVolumes()
    {
        foreach (var sound in soundDict.Values)
        {
            if (sound.source != null)
            {
                sound.source.volume = sound.volume * sfxVolume * masterVolume;
            }
        }

        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
    }

    // ===== ذخیره و بارگذاری تنظیمات =====
    private void SaveAudioSettings()
    {
        PlayerPrefs.SetInt("MusicEnabled", isMusicEnabled ? 1 : 0);
        PlayerPrefs.SetInt("SFXEnabled", isSFXEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Muted", isMuted ? 1 : 0);
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
        Debug.Log("Audio settings saved!");
    }

    private void LoadAudioSettings()
    {
        isMusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        isSFXEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;
        isMuted = PlayerPrefs.GetInt("Muted", 0) == 1;
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // اعمال تنظیمات
        AudioListener.pause = isMuted;
        UpdateAllVolumes();

        Debug.Log("Audio settings loaded!");
    }

    // ===== ریست تنظیمات به حالت پیش‌فرض =====
    public void ResetToDefault()
    {
        isMusicEnabled = true;
        isSFXEnabled = true;
        isMuted = false;
        masterVolume = 1f;
        musicVolume = 1f;
        sfxVolume = 1f;

        AudioListener.pause = false;
        UpdateAllVolumes();
        SaveAudioSettings();

        Debug.Log("Audio settings reset to default!");
    }
}