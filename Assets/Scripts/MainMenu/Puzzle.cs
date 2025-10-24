using UnityEngine;

public class Puzzle : MonoBehaviour
{
    public PlayerLevelSystem levelSystem; 

    // Это должно задаваться вручную
    public float referenceTime = 60f;   // эталон
    public float difficultyMult = 1f;   // 1, 1.3, 1.6 или 2

    private float startTime;

    void Start()
    {
        startTime = Time.time;
    }

    public void OnPuzzleCompleted()
    {
        float actualTime = Time.time - startTime;
        float speedBonus = Mathf.Pow(referenceTime / Mathf.Max(actualTime, 0.1f), 0.8f);
        int baseScore = 50;
        int finalScore = Mathf.RoundToInt(baseScore * difficultyMult * speedBonus);
        
        levelSystem.AddScore(finalScore);
    }
}