using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PuzzleLevelRecord
{
    public string levelName;
    public float completionTimeSeconds;
    public string completionDate; // ISO формат: "2025-10-25T14:30:00"
    public bool usedHints; // Ќовое поле: использовались ли подсказки
    public string gameMode; // Ќовое поле: "classic" или "random"

    public PuzzleLevelRecord(string levelName, float time, string gameMode = "classic", bool usedHints = false)
    {
        this.levelName = levelName;
        this.completionTimeSeconds = time;
        this.completionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        this.usedHints = usedHints;
        this.gameMode = gameMode;
    }
}

public class PuzzleStatsManager : MonoBehaviour
{
    private const string STATS_KEY = "PuzzleLevelRecords";
    private static PuzzleStatsManager _instance;
    public static PuzzleStatsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("PuzzleStatsManager");
                _instance = obj.AddComponent<PuzzleStatsManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    private List<PuzzleLevelRecord> _records = new List<PuzzleLevelRecord>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        LoadStats();
    }

    // ƒобавление записи с указанием режима игры
    public void AddCompletedLevel(string levelName, float timeInSeconds, string gameMode = "classic", bool usedHints = false)
    {
        _records.Add(new PuzzleLevelRecord(levelName, timeInSeconds, gameMode, usedHints));
        SaveStats();
    }

    public List<PuzzleLevelRecord> GetAllRecords()
    {
        return new List<PuzzleLevelRecord>(_records);
    }

    public int GetTotalCompletedLevels()
    {
        return _records.Count;
    }

    // ѕолучение количества пройденных уровней дл€ конкретного режима
    public int GetCompletedLevelsCountByName(string levelName, string gameMode = "classic")
    {
        return _records.FindAll(r => r.levelName == levelName && r.gameMode == gameMode).Count;
    }

    // ѕолучение лучшего времени дл€ уровн€ в конкретном режиме
    public float GetBestTimeForLevel(string levelName, string gameMode = "classic")
    {
        float best = float.MaxValue;
        bool found = false;

        foreach (var record in _records)
        {
            if (record.levelName == levelName && record.gameMode == gameMode && !record.usedHints)
            {
                if (record.completionTimeSeconds < best)
                {
                    best = record.completionTimeSeconds;
                    found = true;
                }
            }
        }

        return found ? best : -1f;
    }

    // ѕолучение лучшего времени без подсказок дл€ уровн€ в конкретном режиме
    public float GetBestTimeForLevelWithoutHints(string levelName, string gameMode = "classic")
    {
        return GetBestTimeForLevel(levelName, gameMode);
    }

    private void SaveStats()
    {
        string json = JsonUtility.ToJson(new StatsWrapper { records = _records });
        PlayerPrefs.SetString(STATS_KEY, json);
        PlayerPrefs.Save();
    }

    private void LoadStats()
    {
        if (PlayerPrefs.HasKey(STATS_KEY))
        {
            string json = PlayerPrefs.GetString(STATS_KEY);
            try
            {
                var wrapper = JsonUtility.FromJson<StatsWrapper>(json);
                _records = wrapper.records ?? new List<PuzzleLevelRecord>();

                // ћиграци€ старых записей (без пол€ gameMode)
                foreach (var record in _records)
                {
                    if (string.IsNullOrEmpty(record.gameMode))
                    {
                        // ѕо умолчанию считаем, что старые записи - это классический режим
                        record.gameMode = "classic";
                    }
                }

                SaveStats(); // —охран€ем обновленные данные после миграции
            }
            catch (Exception e)
            {
                Debug.LogError($"[PuzzleStats] ќшибка при загрузке статистики: {e.Message}");
                _records = new List<PuzzleLevelRecord>();
            }
        }
        else
        {
            _records = new List<PuzzleLevelRecord>();
        }
    }

    [Serializable]
    private class StatsWrapper
    {
        public List<PuzzleLevelRecord> records;
    }

    // ƒополнительные методы дл€ получени€ общей статистики по режимам
    public int GetTotalCompletedLevelsByMode(string gameMode)
    {
        return _records.FindAll(r => r.gameMode == gameMode).Count;
    }

    public Dictionary<string, int> GetLevelsCompletionCountByMode(string gameMode)
    {
        var result = new Dictionary<string, int>();
        var levels = new string[] { "level1", "level2", "level3", "level4" };

        foreach (var level in levels)
        {
            result[level] = GetCompletedLevelsCountByName(level, gameMode);
        }

        return result;
    }
}