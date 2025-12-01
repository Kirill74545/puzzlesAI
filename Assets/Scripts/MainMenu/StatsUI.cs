using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour, ICloseable
{
    [Header("Ссылки")]
    public GameObject statsPanel;          // Вся панель (включая фон)
    public TextMeshProUGUI statsContent;   // Текст статистики
    public Button statsToggleButton;       // Кнопка "Статистика" (одна и та же для открытия/закрытия)
    public GameObject backgroundBlocker;   // Прозрачный фон-блокер (дочерний элемент statsPanel)
    public ProgressPopupUI progressPopupUI;

    [Header("Кнопки режимов")]
    public Button classicModeButton;
    public Button randomModeButton;
    public TextMeshProUGUI classicModeText;
    public TextMeshProUGUI randomModeText;

    [Header("Анимация")]
    public Vector2 appearPosition = new Vector2(0, 0);
    public Vector2 hiddenPosition = new Vector2(0, -1000); // появляется снизу
    public float appearDuration = 0.4f;
    public Ease appearEase = Ease.OutBack;
    public float disappearDuration = 0.3f;
    public Ease disappearEase = Ease.InBack;

    private RectTransform panelRect;
    public bool isStatsPanelOpen = false;
    private InputPanelController inputPanelController;

    // Текущий выбранный режим для отображения статистики
    private GameMode currentMode = GameMode.Classic;

    // Перечисление режимов игры
    private enum GameMode
    {
        Classic,
        Random
    }

    void Start()
    {
        if (statsPanel == null || statsContent == null || statsToggleButton == null || backgroundBlocker == null)
        {
            Debug.LogError("StatsUI: Не все ссылки назначены в инспекторе!");
            return;
        }

        panelRect = statsPanel.GetComponent<RectTransform>();
        statsPanel.SetActive(false);

        // Подписываемся на клики
        statsToggleButton.onClick.AddListener(ToggleStats);

        // Подписываемся на фон (через Button)
        var blockerButton = backgroundBlocker.GetComponent<Button>();
        if (blockerButton != null)
        {
            blockerButton.onClick.AddListener(HideStats);
        }

        // Подписка на кнопки режимов
        if (classicModeButton != null)
            classicModeButton.onClick.AddListener(() => SwitchMode(GameMode.Classic));
        if (randomModeButton != null)
            randomModeButton.onClick.AddListener(() => SwitchMode(GameMode.Random));

        inputPanelController = Object.FindFirstObjectByType<InputPanelController>();

        // Устанавливаем начальный режим
        UpdateModeButtons();
    }

    public void ToggleStats()
    {
        if (isStatsPanelOpen)
        {
            Close();
        }
        else
        {
            PopupManager.Instance.RequestOpen(this);
            ShowStatsInternal();
        }
    }

    public void Close()
    {
        HideStats();
    }

    private void ShowStatsInternal()
    {
        if (isStatsPanelOpen) return;

        isStatsPanelOpen = true;
        statsPanel.SetActive(true);
        UpdateStatsText();

        panelRect.anchoredPosition = hiddenPosition;
        panelRect.DOAnchorPos(appearPosition, appearDuration).SetEase(appearEase);
    }

    public void ShowStats()
    {
        if (isStatsPanelOpen) return;

        if (progressPopupUI != null && progressPopupUI.isOpen)
        {
            progressPopupUI.HidePopup();
        }

        isStatsPanelOpen = true;
        statsPanel.SetActive(true);
        UpdateStatsText();

        panelRect.anchoredPosition = hiddenPosition;
        panelRect.DOAnchorPos(appearPosition, appearDuration).SetEase(appearEase);
    }

    public void HideStats()
    {
        if (!isStatsPanelOpen) return;

        isStatsPanelOpen = false;

        panelRect.DOAnchorPos(hiddenPosition, disappearDuration)
                  .SetEase(disappearEase)
                  .OnComplete(() =>
                  {
                      statsPanel.SetActive(false);
                      PopupManager.Instance.NotifyClosed(this);
                  });
    }

    // Переключение режима статистики
    private void SwitchMode(GameMode mode)
    {
        currentMode = mode;
        UpdateModeButtons();
        UpdateStatsText();
    }

    private void UpdateModeButtons()
    {
        if (classicModeButton == null || randomModeButton == null) return;

        // Сбрасываем цвета кнопок
        ColorBlock classicColors = classicModeButton.colors;
        ColorBlock randomColors = randomModeButton.colors;

        // Светлый цвет для неактивной кнопки
        classicColors.normalColor = new Color32(70, 130, 180, 255); // Синий цвет для неактивной кнопки
        randomColors.normalColor = new Color32(70, 130, 180, 255);  // Синий цвет для неактивной кнопки

        // Темный цвет для активной кнопки (текущего режима)
        if (currentMode == GameMode.Classic)
        {
            classicColors.normalColor = new Color32(50, 50, 50, 255); // Темный цвет для активной кнопки
        }
        else
        {
            randomColors.normalColor = new Color32(50, 50, 50, 255); // Темный цвет для активной кнопки
        }

        classicModeButton.colors = classicColors;
        randomModeButton.colors = randomColors;

        // Обновляем текст на кнопках, если нужно
        if (classicModeText != null)
            classicModeText.text = "Классический";
        if (randomModeText != null)
            randomModeText.text = "Случайный";
    }

    private void UpdateStatsText()
    {
        var levelSystem = Object.FindFirstObjectByType<PlayerLevelSystem>();

        int totalScore = PlayerPrefs.GetInt("PlayerTotalScore", 0);
        int currentLevel = PlayerPrefs.GetInt("PlayerCurrentLevel", 1);

        // Определяем заголовок в зависимости от режима
        string modeName = currentMode == GameMode.Classic ? "КЛАССИЧЕСКИЙ РЕЖИМ" : "СЛУЧАЙНЫЙ РЕЖИМ";
        string output = $"<b><size=40>СТАТИСТИКА ПАЗЛОВ</size></b>\n<size=24>({modeName})</size>\n\n";

        output += $"<b>Всего очков:</b> {totalScore}\n";
        output += $"<b>Текущий уровень:</b> {currentLevel}\n\n";

        // Статистика по уровням - разная для каждого режима
        var levelNames = new string[] { "level1", "level2", "level3", "level4" };
        string modeForQuery = currentMode == GameMode.Classic ? "classic" : "random";
        foreach (var level in levelNames)
        {
            // Получаем статистику для конкретного режима
            int count = PuzzleStatsManager.Instance.GetCompletedLevelsCountByName(level, modeForQuery);

            float bestTime = PuzzleStatsManager.Instance.GetBestTimeForLevel(level, modeForQuery);

            string levelDisplayName = GetLevelDisplayName(level);
            output += $"<b>{levelDisplayName}</b>\n";
            output += $"Пройдено: {count} раз\n";

            if (bestTime >= 0f)
            {
                int minutes = Mathf.FloorToInt(bestTime / 60);
                int seconds = Mathf.FloorToInt(bestTime % 60);
                output += $"Лучшее время: {minutes:00}:{seconds:00}\n";
            }
            else
            {
                output += "Лучшее время: —\n";
            }

            output += "\n";
        }

        statsContent.text = output.Trim();
    }

    private string GetLevelDisplayName(string levelKey)
    {
        return levelKey switch
        {
            "level1" => "Easy",
            "level2" => "Medium",
            "level3" => "Hard",
            "level4" => "Very hard",
            _ => levelKey
        };
    }
}