using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PuzzleLevelRecord
{
    public string levelName;
    public float completionTimeSeconds;
    public string completionDate; // ISO формат: "2025-10-25T14:30:00"

    public PuzzleLevelRecord(string levelName, float time)
    {
        this.levelName = levelName;
        this.completionTimeSeconds = time;
        this.completionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
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

    public void AddCompletedLevel(string levelName, float timeInSeconds)
    {
        _records.Add(new PuzzleLevelRecord(levelName, timeInSeconds));
        SaveStats();
        Debug.Log($"[PuzzleStats] Уровень '{levelName}' завершён за {timeInSeconds:F2} сек. Добавлено в статистику.");
    }

    public List<PuzzleLevelRecord> GetAllRecords()
    {
        return new List<PuzzleLevelRecord>(_records); 
    }

    public int GetTotalCompletedLevels()
    {
        return _records.Count;
    }

    public int GetCompletedLevelsCountByName(string levelName)
    {
        return _records.FindAll(r => r.levelName == levelName).Count;
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
            }
            catch (Exception e)
            {
                Debug.LogError($"[PuzzleStats] Ошибка при загрузке статистики: {e.Message}");
                _records = new List<PuzzleLevelRecord>();
            }
        }
        else
        {
            _records = new List<PuzzleLevelRecord>();
        }
    }

    public float GetBestTimeForLevel(string levelName)
    {
        float best = float.MaxValue;
        bool found = false;

        foreach (var record in _records)
        {
            if (record.levelName == levelName)
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

    [Serializable]
    private class StatsWrapper
    {
        public List<PuzzleLevelRecord> records;
    }
}