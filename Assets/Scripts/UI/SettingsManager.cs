using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SettingsManager : MonoBehaviour
{
    private static SettingsManager instance;
    public static SettingsManager Instance => instance;

    [System.Serializable]
    public class GameSettings
    {
        public float masterVolume = 1f;
        public float sfxVolume = 1f;
        public float musicVolume = 1f;
        public bool isMuted = false;
        public int qualityLevel = 2;
        public bool showFPS = false;
    }

    [Header("UI")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle muteToggle;
    [SerializeField] private GameObject settingsPanel;

    private GameSettings settings = new GameSettings();
    private string settingsPath;

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

        settingsPath = Path.Combine(Application.persistentDataPath, "settings.json");
        LoadSettings();
        ApplySettings();

        // اتصال به UI
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            masterSlider.value = settings.masterVolume;
        }
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            sfxSlider.value = settings.sfxVolume;
        }
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            musicSlider.value = settings.musicVolume;
        }
        if (muteToggle != null)
        {
            muteToggle.onValueChanged.AddListener(OnMuteToggled);
            muteToggle.isOn = settings.isMuted;
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void OnMasterVolumeChanged(float value)
    {
        settings.masterVolume = value;
        ApplySettings();
        SaveSettings();
        AudioManager.Instance?.SetMasterVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        settings.sfxVolume = value;
        ApplySettings();
        SaveSettings();
        AudioManager.Instance?.SetSFXVolume(value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        settings.musicVolume = value;
        ApplySettings();
        SaveSettings();
        AudioManager.Instance?.SetMusicVolume(value);
    }

    private void OnMuteToggled(bool isOn)
    {
        settings.isMuted = isOn;
        ApplySettings();
        SaveSettings();
        AudioManager.Instance?.SetMute(isOn);
    }

    private void ApplySettings()
    {
        // اعمال تنظیمات به بازی
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(settings.masterVolume);
            AudioManager.Instance.SetSFXVolume(settings.sfxVolume);
            AudioManager.Instance.SetMusicVolume(settings.musicVolume);
            AudioManager.Instance.SetMute(settings.isMuted);
        }

        // کیفیت گرافیک
        QualitySettings.SetQualityLevel(settings.qualityLevel);
    }

    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(settings, true);
        File.WriteAllText(settingsPath, json);
        Debug.Log($"Settings saved to: {settingsPath}");
    }

    public void LoadSettings()
    {
        if (File.Exists(settingsPath))
        {
            string json = File.ReadAllText(settingsPath);
            JsonUtility.FromJsonOverwrite(json, settings);
            Debug.Log($"Settings loaded from: {settingsPath}");
        }
        else
        {
            // تنظیمات پیش‌فرض
            settings = new GameSettings();
            SaveSettings();
            Debug.Log("Default settings created.");
        }
    }

    public GameSettings GetSettings() => settings;

    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void ResetToDefault()
    {
        settings = new GameSettings();
        ApplySettings();
        SaveSettings();

        if (masterSlider != null) masterSlider.value = settings.masterVolume;
        if (sfxSlider != null) sfxSlider.value = settings.sfxVolume;
        if (musicSlider != null) musicSlider.value = settings.musicVolume;
        if (muteToggle != null) muteToggle.isOn = settings.isMuted;
    }
}