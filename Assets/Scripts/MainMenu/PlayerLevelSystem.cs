using UnityEngine;

public class PlayerLevelSystem : MonoBehaviour
{
    private int currentScore;
    private int currentLevel = 1;

    private const string SCORE_KEY = "PlayerTotalScore";
    private const string LEVEL_KEY = "PlayerCurrentLevel";

    void Awake()
    {
        LoadProgress();
    }

    public void AddScore(int score)
    {
        currentScore += score;
        CheckLevelUp();
        SaveProgress(); 
    }

    void CheckLevelUp()
    {
        while (currentLevel < 100)
        {
            int needed = Mathf.RoundToInt(50 * Mathf.Pow(currentLevel + 1, 1.2f));
            if (currentScore >= needed)
            {
                currentLevel++;
            }
            else
            {
                break;
            }
        }
        SaveProgress(); 
    }

    void SaveProgress()
    {
        PlayerPrefs.SetInt(SCORE_KEY, currentScore);
        PlayerPrefs.SetInt(LEVEL_KEY, currentLevel);
        PlayerPrefs.Save(); 
    }

    public void LoadProgress()
    {
        currentScore = PlayerPrefs.GetInt(SCORE_KEY, 0);
        currentLevel = PlayerPrefs.GetInt(LEVEL_KEY, 1);
    }

    public int GetTotalScore() => currentScore;
    public int GetCurrentLevel() => currentLevel;

    // Новый метод: сколько очков нужно до следующего уровня
    public int GetScoreToNextLevel()
    {
        int nextLevel = currentLevel + 1;
        int needed = Mathf.RoundToInt(50 * Mathf.Pow(nextLevel, 1.2f));
        return Mathf.Max(0, needed - currentScore);
    }

    // Сколько очков нужно для достижения уровня (минимальный порог)
    public int GetScoreRequiredForLevel(int level)
    {
        if (level <= 1) return 0;
        return Mathf.RoundToInt(50 * Mathf.Pow(level, 1.2f));
    }

    // Сколько очков накоплено в рамках текущего уровня
    public int GetCurrentLevelProgress()
    {
        int scoreForCurrent = GetScoreRequiredForLevel(currentLevel);
        return currentScore - scoreForCurrent;
    }

    // Сколько очков всего нужно в текущем уровне (от начала уровня до следующего)
    public int GetCurrentLevelTotalNeeded()
    {
        int scoreForCurrent = GetScoreRequiredForLevel(currentLevel);
        int scoreForNext = GetScoreRequiredForLevel(currentLevel + 1);
        return scoreForNext - scoreForCurrent;
    }
}