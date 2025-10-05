using UnityEngine;

public class PlayerLevelSystem : MonoBehaviour
{
    public int currentScore;     
    public int currentLevel = 1; 


    public void AddScore(int score)
    {
        currentScore += score;
        CheckLevelUp();
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
    }
}