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
        inputPanelController = Object.FindFirstObjectByType<InputPanelController>();

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

    private void UpdateStatsText()
    {
        var levelSystem = Object.FindFirstObjectByType<PlayerLevelSystem>();

        int totalScore = PlayerPrefs.GetInt("PlayerTotalScore", 0);
        int currentLevel = PlayerPrefs.GetInt("PlayerCurrentLevel", 1);
        string output = "<b><size=40>СТАТИСТИКА ПАЗЛОВ</size></b>\n\n";

        output += $"<b>Всего очков:</b> {totalScore}\n";
        output += $"<b>Текущий уровень:</b> {currentLevel}\n\n";

        // Статистика по уровням
        var levelNames = new string[] { "level1", "level2", "level3", "level4" };

        foreach (var level in levelNames)
        {
            int count = PuzzleStatsManager.Instance.GetCompletedLevelsCountByName(level);
            float bestTime = PuzzleStatsManager.Instance.GetBestTimeForLevel(level);

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
            "level1" => "Новичок (4x4)",
            "level2" => "Любитель (5x5)",
            "level3" => "Профессионал (7x7)",
            "level4" => "Мастер (9x9)",
            _ => levelKey
        };
    }
}