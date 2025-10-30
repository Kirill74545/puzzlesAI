using UnityEngine;
using TMPro;

public class ProgressPopupUI : MonoBehaviour
{
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private PlayerLevelSystem playerSystem; // ← новое поле!

    public void Show()
    {
        if (playerSystem == null)
        {
            Debug.LogError("PlayerLevelSystem не задан в инспекторе!");
            return;
        }

        levelText.text = "Level: " + playerSystem.GetCurrentLevel();
        progressText.text = "You have " + playerSystem.GetScoreToNextLevel() + " points left to advance to the next level";
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}