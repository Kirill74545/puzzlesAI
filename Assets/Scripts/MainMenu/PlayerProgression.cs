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

    void LoadProgress()
    {
        currentScore = PlayerPrefs.GetInt(SCORE_KEY, 0);
        currentLevel = PlayerPrefs.GetInt(LEVEL_KEY, 1);
    }

    public int GetTotalScore() => currentScore;
    public int GetCurrentLevel() => currentLevel;
}