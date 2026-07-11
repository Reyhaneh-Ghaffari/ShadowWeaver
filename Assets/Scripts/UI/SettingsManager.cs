using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    private static SettingsManager instance;
    public static SettingsManager Instance => instance;

    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Slider masterVolumeSlider;

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
    }

    private void Start()
    {
        // پنهان کردن پنل تنظیمات در ابتدا
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // بارگذاری تنظیمات ذخیره شده
        LoadAndApplySettings();

        // تنظیم رویدادها
        SetupUIEvents();
    }

    private void SetupUIEvents()
    {
        if (musicToggle != null)
        {
            musicToggle.onValueChanged.RemoveAllListeners();
            musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        }

        if (sfxToggle != null)
        {
            sfxToggle.onValueChanged.RemoveAllListeners();
            sfxToggle.onValueChanged.AddListener(OnSFXToggleChanged);
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
    }

    private void LoadAndApplySettings()
    {
        // بارگذاری از PlayerPrefs
        bool musicOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        bool sfxOn = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);

        // اعمال به UI
        if (musicToggle != null)
            musicToggle.isOn = musicOn;

        if (sfxToggle != null)
            sfxToggle.isOn = sfxOn;

        if (masterVolumeSlider != null)
            masterVolumeSlider.value = masterVol;

        // اعمال به AudioManager
        AudioManager.Instance?.SetMusicEnabled(musicOn);
        AudioManager.Instance?.SetSFXEnabled(sfxOn);
        AudioManager.Instance?.SetMasterVolume(masterVol);
    }

    // ===== رویدادها =====
    public void OnMusicToggleChanged(bool isOn)
    {
        Debug.Log($"Music Toggle: {isOn}");
        AudioManager.Instance?.SetMusicEnabled(isOn);
        PlayerPrefs.SetInt("MusicEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnSFXToggleChanged(bool isOn)
    {
        Debug.Log($"SFX Toggle: {isOn}");
        AudioManager.Instance?.SetSFXEnabled(isOn);
        PlayerPrefs.SetInt("SFXEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnMasterVolumeChanged(float value)
    {
        Debug.Log($"Master Volume: {value}");
        AudioManager.Instance?.SetMasterVolume(value);
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    // ===== باز و بسته کردن پنل =====
    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            bool isActive = settingsPanel.activeSelf;
            settingsPanel.SetActive(!isActive);
            Debug.Log($"Settings Panel: {!isActive}");
        }
    }

    public void CloseSettingsPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OpenSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            LoadAndApplySettings(); // بارگذاری مجدد تنظیمات
        }
    }
}