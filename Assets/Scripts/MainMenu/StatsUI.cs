using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    [Header("������")]
    public GameObject statsPanel;          // ��� ������ (������� ���)
    public TextMeshProUGUI statsContent;   // ����� ����������
    public Button statsToggleButton;       // ������ "����������" (���� � �� �� ��� ��������/��������)
    public GameObject backgroundBlocker;   // ���������� ���-������ (�������� ������� statsPanel)

    [Header("��������")]
    public Vector2 appearPosition = new Vector2(0, 0);
    public Vector2 hiddenPosition = new Vector2(0, -1000); // ���������� �����
    public float appearDuration = 0.4f;
    public Ease appearEase = Ease.OutBack;
    public float disappearDuration = 0.3f;
    public Ease disappearEase = Ease.InBack;

    private RectTransform panelRect;
    private bool isStatsPanelOpen = false;
    private InputPanelController inputPanelController;

    void Start()
    {
        if (statsPanel == null || statsContent == null || statsToggleButton == null || backgroundBlocker == null)
        {
            Debug.LogError("StatsUI: �� ��� ������ ��������� � ����������!");
            return;
        }

        panelRect = statsPanel.GetComponent<RectTransform>();
        statsPanel.SetActive(false);

        // ������������� �� �����
        statsToggleButton.onClick.AddListener(ToggleStats);

        // ������������� �� ��� (����� Button)
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
            HideStats();
        }
        else
        {
            ShowStats();
        }
    }

    public void ShowStats()
    {
        if (isStatsPanelOpen) return;

        isStatsPanelOpen = true;
        statsPanel.SetActive(true);
        UpdateStatsText();

        // �������� ���������
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
                  });
    }

    private void UpdateStatsText()
    {
        var levelSystem = Object.FindFirstObjectByType<PlayerLevelSystem>();

        int totalScore = PlayerPrefs.GetInt("PlayerTotalScore", 0);
        int currentLevel = PlayerPrefs.GetInt("PlayerCurrentLevel", 1);
        string output = "<b><size=40>���������� ������</size></b>\n\n";

        output += $"<b>����� �����:</b> {totalScore}\n";
        output += $"<b>������� �������:</b> {currentLevel}\n\n";

        // ���������� �� �������
        var levelNames = new string[] { "level1", "level2", "level3", "level4" };

        foreach (var level in levelNames)
        {
            int count = PuzzleStatsManager.Instance.GetCompletedLevelsCountByName(level);
            float bestTime = PuzzleStatsManager.Instance.GetBestTimeForLevel(level);

            string levelDisplayName = GetLevelDisplayName(level);
            output += $"<b>{levelDisplayName}</b>\n";
            output += $"��������: {count} ���\n";

            if (bestTime >= 0f)
            {
                int minutes = Mathf.FloorToInt(bestTime / 60);
                int seconds = Mathf.FloorToInt(bestTime % 60);
                output += $"������ �����: {minutes:00}:{seconds:00}\n";
            }
            else
            {
                output += "������ �����: �\n";
            }

            output += "\n";
        }

        statsContent.text = output.Trim();
    }

    private string GetLevelDisplayName(string levelKey)
    {
        return levelKey switch
        {
            "level1" => "������� (4x4)",
            "level2" => "�������� (5x5)",
            "level3" => "������������ (7x7)",
            "level4" => "������ (9x9)",
            _ => levelKey
        };
    }
}