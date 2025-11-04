using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class ProgressPopupUI : MonoBehaviour, ICloseable
{
    [Header("Ссылки")]
    public GameObject popupPanel;          // Вся панель попапа
    public TMP_Text levelText;             // "Level: X (Y / Z EXP)"
    public Button toggleButton;            // Одна кнопка — открыть/закрыть
    public GameObject backgroundBlocker;   // Фон 
    public TMP_Text coinsText;
    public StatsUI statsUI;

    [Header("Система")]
    public PlayerLevelSystem playerLevelSystem; 

    [Header("Анимация")]
    public Vector2 appearPosition = new Vector2(0f, 0f);
    public Vector2 hiddenPosition = new Vector2(0f, -1000f);
    public float appearDuration = 0.4f;
    public Ease appearEase = Ease.OutBack;
    public float disappearDuration = 0.3f;
    public Ease disappearEase = Ease.InBack;

    private RectTransform panelRect;
    public bool isOpen = false;

    void Start()
    {
        if (popupPanel == null || levelText == null || toggleButton == null)
        {
            Debug.LogError("ProgressPopupUI: Не все ссылки назначены в инспекторе!");
            enabled = false;
            return;
        }

        panelRect = popupPanel.GetComponent<RectTransform>();
        popupPanel.SetActive(false);

        toggleButton.onClick.AddListener(TogglePopup);

        if (backgroundBlocker != null)
        {
            var blockerButton = backgroundBlocker.GetComponent<Button>();
            if (blockerButton != null)
                blockerButton.onClick.AddListener(HidePopup);
        }

        if (playerLevelSystem == null)
            playerLevelSystem = Object.FindFirstObjectByType<PlayerLevelSystem>();

        panelRect.anchoredPosition = hiddenPosition;
    }

    public void TogglePopup()
    {
        if (isOpen)
        {
            Close();
        }
        else
        {
            PopupManager.Instance.RequestOpen(this);
            ShowPopupInternal();
        }
    }

    public void Close()
    {
        HidePopup();
    }

    private void ShowPopupInternal()
    {
        if (isOpen || playerLevelSystem == null) return;

        isOpen = true;
        popupPanel.SetActive(true);
        UpdateText();

        panelRect.anchoredPosition = hiddenPosition;
        panelRect.DOAnchorPos(appearPosition, appearDuration).SetEase(appearEase);
    }

    public void ShowPopup()
    {
        if (isOpen || playerLevelSystem == null) return;

        if (statsUI != null && statsUI.isStatsPanelOpen)
        {
            statsUI.HideStats();
        }

        isOpen = true;
        popupPanel.SetActive(true);
        UpdateText();

        panelRect.anchoredPosition = hiddenPosition;
        panelRect.DOAnchorPos(appearPosition, appearDuration).SetEase(appearEase);
    }

    public void HidePopup()
    {
        if (!isOpen) return;

        isOpen = false;
        panelRect.DOAnchorPos(hiddenPosition, disappearDuration)
                  .SetEase(disappearEase)
                  .OnComplete(() =>
                  {
                      popupPanel.SetActive(false);
                      PopupManager.Instance.NotifyClosed(this);
                  });
    }

    private void UpdateText()
    {
        int currentLevel = playerLevelSystem.GetCurrentLevel();
        int progressInLevel = playerLevelSystem.GetCurrentLevelProgress();
        int totalNeededInLevel = playerLevelSystem.GetCurrentLevelTotalNeeded();
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        levelText.text = $"Level: {currentLevel} ({progressInLevel} / {totalNeededInLevel} EXP)";
        if (coinsText != null)
            coinsText.text = $"Coins: {totalCoins}";
    }

}