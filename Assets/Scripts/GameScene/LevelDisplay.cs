using UnityEngine;
using TMPro;

public class LevelDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private PlayerLevelSystem playerSystem; 

    void Start()
    {
        if (playerSystem == null)
        {
            Debug.LogError("LevelDisplay: PlayerLevelSystem не задан в инспекторе!");
            return;
        }

        UpdateLevel();
    }

    void UpdateLevel()
    {
        levelText.text = "Level: " + playerSystem.GetCurrentLevel();
    }

    // Вызови этот метод, если хочешь обновить уровень вручную (например, после получения очков)
    public void Refresh()
    {
        if (playerSystem != null)
            UpdateLevel();
    }
}