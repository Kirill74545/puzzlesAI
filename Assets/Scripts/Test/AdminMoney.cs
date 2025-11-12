using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdminCoinGiver : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Если true, объект будет скрыт в билде")]
    public bool hideInBuild = true;

    [Header("UI элементы")]
    public Button adminButton; 

    [Header("Настройки наград")]
    public int coinsToAdd = 1000;
    public int targetLevel = 15; 

    void Start()
    {
        // Скрываем объект в релизной сборке
        if (hideInBuild && !Debug.isDebugBuild)
        {
            gameObject.SetActive(false);
            return;
        }

        // Находим кнопку, если она не назначена в инспекторе
        if (adminButton == null)
        {
            adminButton = GetComponent<Button>();
            if (adminButton == null)
            {
                adminButton = GetComponentInChildren<Button>();
            }
        }

        // Назначаем обработчик клика
        if (adminButton != null)
        {
            adminButton.onClick.AddListener(GiveRewards);
        }
    }

    private void GiveRewards()
    {
        // Добавляем монеты
        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        int newCoins = currentCoins + coinsToAdd;
        PlayerPrefs.SetInt("TotalCoins", newCoins);
        PlayerPrefs.Save();

        // Устанавливаем уровень
        SetPlayerLevel(targetLevel);

        Debug.Log($"Админ: Добавлено {coinsToAdd} монет и установлен {targetLevel} уровень. Всего монет: {newCoins}");

        UpdateDisplay();
    }

    private void SetPlayerLevel(int level)
    {
        PlayerLevelSystem levelSystem = Object.FindFirstObjectByType<PlayerLevelSystem>();

        // Рассчитываем количество очков, необходимых для достижения уровня
        int scoreRequired = GetScoreRequiredForLevel(level);

        // Устанавливаем очки и уровень
        PlayerPrefs.SetInt("PlayerTotalScore", scoreRequired);
        PlayerPrefs.SetInt("PlayerCurrentLevel", level);
        PlayerPrefs.Save();

        if (levelSystem != null)
        {
            levelSystem.LoadProgress();
            Debug.Log($"Админ: Уровень установлен на {level}, очков: {scoreRequired}");
        }
        else
        {
            Debug.LogWarning("PlayerLevelSystem не найдена. Уровень и очки установлены только в PlayerPrefs.");
        }
    }

    private int GetScoreRequiredForLevel(int level)
    {
        if (level <= 1) return 0;
        return Mathf.RoundToInt(50 * Mathf.Pow(level, 1.2f));
    }

    private void UpdateDisplay()
    {
        UpdateCoinsDisplay();
        UpdateLevelDisplay();
    }

    private void UpdateCoinsDisplay()
    {
        var coinTexts = FindObjectsOfType<TMP_Text>();
        foreach (var text in coinTexts)
        {
            if (text.name.ToLower().Contains("coin") || text.name.ToLower().Contains("coins"))
            {
                text.text = PlayerPrefs.GetInt("TotalCoins", 0).ToString();
            }
        }
    }

    private void UpdateLevelDisplay()
    {
        var levelTexts = FindObjectsOfType<TMP_Text>();
        foreach (var text in levelTexts)
        {
            if (text.name.ToLower().Contains("level"))
            {
                text.text = PlayerPrefs.GetInt("PlayerCurrentLevel", 1).ToString();
            }
        }

        var progressPopup = FindObjectOfType<ProgressPopupUI>();
        if (progressPopup != null)
        {
            progressPopup.UpdateText(); // Обновляем текст уровня и опыта
        }

        // Обновляем систему уровней, если она существует
        var levelSystem = FindObjectOfType<PlayerLevelSystem>();
        if (levelSystem != null)
        {
            levelSystem.LoadProgress(); // Перезагружаем прогресс
        }
    }

    // Метод для ручного добавления наград из других скриптов
    public void GiveRewards(int coins, int level)
    {
        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        int newCoins = currentCoins + coins;
        PlayerPrefs.SetInt("TotalCoins", newCoins);

        SetPlayerLevel(level);
        PlayerPrefs.Save();

        Debug.Log($"Админ: Добавлено {coins} монет и установлен {level} уровень. Всего монет: {newCoins}");
        UpdateDisplay();
    }

    // Методы для получения текущего состояния
    public int GetCurrentCoins()
    {
        return PlayerPrefs.GetInt("TotalCoins", 0);
    }

    public int GetCurrentLevel()
    {
        return PlayerPrefs.GetInt("PlayerCurrentLevel", 1);
    }

    // Удаляем обработчик при уничтожении объекта
    void OnDestroy()
    {
        if (adminButton != null)
        {
            adminButton.onClick.RemoveListener(GiveRewards);
        }
    }
}