using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    private static DataManager instance;
    public static DataManager Instance => instance;

    [System.Serializable]
    public class GameData
    {
        public int currentLevel = 0;
        public int highestLevel = 0;
        public int totalScore = 0;
        public float playTime = 0f;
        public bool hasCompletedGame = false;
        public Dictionary<string, bool> levelStates = new Dictionary<string, bool>();
    }

    [System.Serializable]
    public class PlayerProgress
    {
        public Vector3 lastCheckpoint;
        public string lastLevel;
        public int collectedItems = 0;
        public int totalDeaths = 0;
    }

    private GameData gameData = new GameData();
    private PlayerProgress playerProgress = new PlayerProgress();
    private string dataPath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            dataPath = Path.Combine(Application.persistentDataPath, "gamedata.json");
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        SaveGameData();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGameData();
        }
    }

    // ===== ذخیره‌سازی داده‌ها =====
    public void SaveGameData()
    {
        try
        {
            string json = JsonUtility.ToJson(gameData, true);
            File.WriteAllText(dataPath, json);
            Debug.Log($"Game data saved to: {dataPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving game data: {e.Message}");
        }
    }

    public void LoadGameData()
    {
        try
        {
            if (File.Exists(dataPath))
            {
                string json = File.ReadAllText(dataPath);
                JsonUtility.FromJsonOverwrite(json, gameData);
                Debug.Log($"Game data loaded from: {dataPath}");
            }
            else
            {
                gameData = new GameData();
                SaveGameData();
                Debug.Log("New game data created.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading game data: {e.Message}");
            gameData = new GameData();
        }
    }

    // ===== دسترسی به داده‌ها =====
    public int GetCurrentLevel() => gameData.currentLevel;
    public int GetHighestLevel() => gameData.highestLevel;
    public int GetTotalScore() => gameData.totalScore;
    public bool HasCompletedGame() => gameData.hasCompletedGame;

    public void SetCurrentLevel(int level)
    {
        gameData.currentLevel = level;
        if (level > gameData.highestLevel)
            gameData.highestLevel = level;
        SaveGameData();
    }

    public void AddScore(int points)
    {
        gameData.totalScore += points;
        SaveGameData();
    }

    public void CompleteGame()
    {
        gameData.hasCompletedGame = true;
        SaveGameData();
    }

    public void ResetGameData()
    {
        gameData = new GameData();
        SaveGameData();
    }

    // ===== مدیریت چک‌پوینت =====
    public void SaveCheckpoint(Vector3 position, string levelName)
    {
        playerProgress.lastCheckpoint = position;
        playerProgress.lastLevel = levelName;

        PlayerPrefs.SetFloat("CheckpointX", position.x);
        PlayerPrefs.SetFloat("CheckpointY", position.y);
        PlayerPrefs.SetFloat("CheckpointZ", position.z);
        PlayerPrefs.SetString("CheckpointLevel", levelName);
        PlayerPrefs.Save();

        Debug.Log($"Checkpoint saved: {position} in {levelName}");
    }

    public Vector3 LoadCheckpoint()
    {
        if (PlayerPrefs.HasKey("CheckpointX"))
        {
            float x = PlayerPrefs.GetFloat("CheckpointX");
            float y = PlayerPrefs.GetFloat("CheckpointY");
            float z = PlayerPrefs.GetFloat("CheckpointZ");
            return new Vector3(x, y, z);
        }
        return Vector3.zero;
    }

    public string LoadCheckpointLevel()
    {
        return PlayerPrefs.GetString("CheckpointLevel", "Level1");
    }

    // ===== مدیریت تنظیمات با PlayerPrefs =====
    public void SaveSetting(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
    }

    public void SaveSetting(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    public void SaveSetting(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    public string LoadSetting(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    public int LoadSetting(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public float LoadSetting(string key, float defaultValue = 0f)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    public bool HasSetting(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    public void DeleteSetting(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }

    public void DeleteAllSettings()
    {
        PlayerPrefs.DeleteAll();
    }

    // ===== مدیریت سطح‌های بازی =====
    public void UnlockLevel(int levelIndex)
    {
        string key = $"Level_{levelIndex}_Unlocked";
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();

        if (levelIndex > gameData.highestLevel)
        {
            gameData.highestLevel = levelIndex;
            SaveGameData();
        }
    }

    public bool IsLevelUnlocked(int levelIndex)
    {
        string key = $"Level_{levelIndex}_Unlocked";
        return PlayerPrefs.GetInt(key, 0) == 1 || levelIndex == 0;
    }

    public void SaveLevelScore(int levelIndex, int score)
    {
        string key = $"Level_{levelIndex}_Score";
        int currentScore = PlayerPrefs.GetInt(key, 0);
        if (score > currentScore)
        {
            PlayerPrefs.SetInt(key, score);
            PlayerPrefs.Save();
        }
    }

    public int GetLevelScore(int levelIndex)
    {
        string key = $"Level_{levelIndex}_Score";
        return PlayerPrefs.GetInt(key, 0);
    }

    // ===== مدیریت زمان بازی =====
    public void UpdatePlayTime(float deltaTime)
    {
        gameData.playTime += deltaTime;
    }

    public float GetTotalPlayTime() => gameData.playTime;

    public string GetFormattedPlayTime()
    {
        float time = gameData.playTime;
        int hours = Mathf.FloorToInt(time / 3600);
        int minutes = Mathf.FloorToInt((time % 3600) / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        if (hours > 0)
            return $"{hours}h {minutes}m {seconds}s";
        else if (minutes > 0)
            return $"{minutes}m {seconds}s";
        else
            return $"{seconds}s";
    }

    // ===== ذخیره‌سازی وضعیت‌های سفارشی =====
    public void SaveCustomData<T>(string key, T data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
    }

    public T LoadCustomData<T>(string key) where T : new()
    {
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            return JsonUtility.FromJson<T>(json);
        }
        return new T();
    }

    // ===== دیباگ =====
    public void PrintAllData()
    {
        Debug.Log("=== Game Data ===");
        Debug.Log($"Current Level: {gameData.currentLevel}");
        Debug.Log($"Highest Level: {gameData.highestLevel}");
        Debug.Log($"Total Score: {gameData.totalScore}");
        Debug.Log($"Play Time: {GetFormattedPlayTime()}");
        Debug.Log($"Game Completed: {gameData.hasCompletedGame}");
        Debug.Log($"Data Path: {dataPath}");
    }
}