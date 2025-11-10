using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ProgressPopupUI : MonoBehaviour, ICloseable
{
    [Header("Основные ссылки")]
    public GameObject popupPanel;          // Вся панель попапа
    public TMP_Text levelText;             // "Уровень: X (Y / Z EXP)"
    public Button toggleButton;            // Кнопка открытия/закрытия
    public GameObject backgroundBlocker;   // Фон-затемнитель
    public TMP_Text coinsText;             // Текст с монетами
    public StatsUI statsUI;                // Статистика (если есть)

    [Header("Система")]
    public PlayerLevelSystem playerLevelSystem; // Система уровней

    [Header("Анимация")]
    public Vector2 appearPosition = new Vector2(0f, 0f);   // Позиция при открытии
    public Vector2 hiddenPosition = new Vector2(0f, -1000f); // Позиция при закрытии
    public float appearDuration = 0.4f;
    public Ease appearEase = Ease.OutBack;
    public float disappearDuration = 0.3f;
    public Ease disappearEase = Ease.InBack;

    // === НОВЫЕ ЭЛЕМЕНТЫ ДЛЯ ФОНов ===
    [Header("Фоны — интерфейс")]
    public GameObject backgroundItemPrefab;      // Префаб одного элемента фона
    public GameObject backgroundsGrid;           // Объект с GridLayoutGroup
    public Button nextPageButton;                // Кнопка "Далее"
    public Button prevPageButton;                // Кнопка "Назад"
    public TMP_Text currentPageText;             // Текст "Страница 1 из 3"

    [Header("DOTween Анимации")]
    public float itemScaleDuration = 0.2f;
    public float itemScaleAmount = 1.1f;
    public float itemReturnDuration = 0.1f;

    [Header("Фоны — ScriptableObject")]
    public BackgroundListSO backgroundListSO; // Назначь в инспекторе

    private RectTransform panelRect;
    public bool isOpen = false;

    private int currentPage = 0;
    private const int itemsPerPage = 4;
    private List<BackgroundData> backgroundDataList = new List<BackgroundData>();
    private List<GameObject> activeBackgroundItems = new List<GameObject>();

    private const string CURRENT_BACKGROUND_KEY = "CurrentBackgroundId";

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

        toggleButton.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayButtonClick();
            TogglePopup();
        });

        if (backgroundBlocker != null)
        {
            var blockerButton = backgroundBlocker.GetComponent<Button>();
            if (blockerButton != null)
                blockerButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayButtonClick();
                    HidePopup();
                });
        }

        if (playerLevelSystem == null)
            playerLevelSystem = Object.FindFirstObjectByType<PlayerLevelSystem>();

        panelRect.anchoredPosition = hiddenPosition;

        InitializeBackgrounds();

        if (nextPageButton != null)
            nextPageButton.onClick.AddListener(NextPage);

        if (prevPageButton != null)
            prevPageButton.onClick.AddListener(PrevPage);

        UpdateNavigationButtons();
    }

    private void InitializeBackgrounds()
    {
        if (backgroundListSO == null || backgroundListSO.backgrounds == null)
        {
            Debug.LogError("BackgroundListSO не назначен или пуст!");
            return;
        }

        foreach (var bgSO in backgroundListSO.backgrounds)
        {
            backgroundDataList.Add(new BackgroundData
            {
                id = bgSO.id,
                requiredLevel = bgSO.requiredLevel,
                requiredCoins = bgSO.requiredCoins,
                previewSprite = bgSO.previewSprite,
                isPurchased = bgSO.id == 0, // Первый фон всегда доступен
                backgroundName = bgSO.backgroundName,
                description = bgSO.description
            });
        }

        if (!PlayerPrefs.HasKey(CURRENT_BACKGROUND_KEY))
        {
            PlayerPrefs.SetInt(CURRENT_BACKGROUND_KEY, 0);
            PlayerPrefs.Save();
        }

        for (int i = 0; i < backgroundDataList.Count; i++)
        {
            string purchaseKey = $"BG_Purchased_{backgroundDataList[i].id}";
            if (!PlayerPrefs.HasKey(purchaseKey))
            {
                PlayerPrefs.SetInt(purchaseKey, backgroundDataList[i].id == 0 ? 1 : 0);
                PlayerPrefs.Save();
            }
            else
            {
                backgroundDataList[i].isPurchased = PlayerPrefs.GetInt(purchaseKey) == 1;
            }
        }
    }

    public void TogglePopup()
    {
        if (isOpen)
            Close();
        else
        {
            PopupManager.Instance.RequestOpen(this);
            ShowPopupInternal();
        }
    }

    public void Close() => HidePopup();

    private void ShowPopupInternal()
    {
        if (isOpen || playerLevelSystem == null) return;

        isOpen = true;
        popupPanel.SetActive(true);
        UpdateText();
        ResetBackgroundGrid();

        panelRect.anchoredPosition = hiddenPosition;
        panelRect.DOAnchorPos(appearPosition, appearDuration).SetEase(appearEase);
    }

    public void ShowPopup()
    {
        if (isOpen || playerLevelSystem == null) return;

        if (statsUI != null && statsUI.isStatsPanelOpen)
            statsUI.HideStats();

        isOpen = true;
        popupPanel.SetActive(true);
        UpdateText();
        ResetBackgroundGrid();

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
            coinsText.text = $"{totalCoins}";
    }

    private void ResetBackgroundGrid()
    {
        currentPage = 0;
        PopulateBackgroundGrid();
    }

    private void PopulateBackgroundGrid()
    {
        foreach (var item in activeBackgroundItems)
            Destroy(item);
        activeBackgroundItems.Clear();

        int startIndex = currentPage * itemsPerPage;
        int endIndex = Mathf.Min(startIndex + itemsPerPage, backgroundDataList.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            BackgroundData data = backgroundDataList[i];
            GameObject item = Instantiate(backgroundItemPrefab, backgroundsGrid.transform);
            SetupBackgroundItem(item, data);
            activeBackgroundItems.Add(item);
        }

        UpdateNavigationButtons();
    }

    private void SetupBackgroundItem(GameObject item, BackgroundData data)
    {
        Image previewImage = item.GetComponentInChildren<Image>();
        TMP_Text requirementText = item.GetComponentInChildren<TMP_Text>();
        Button actionButton = item.GetComponentInChildren<Button>();
        Image checkmark = item.transform.Find("Checkmark")?.GetComponent<Image>();

        if (previewImage != null && data.previewSprite != null)
            previewImage.sprite = data.previewSprite;

        string reqText = "";
        if (data.requiredLevel > 1) reqText += $"{data.requiredLevel} lvl";
        if (data.requiredCoins > 0)
        {
            if (!string.IsNullOrEmpty(reqText)) reqText += ", ";
            reqText += $"{data.requiredCoins} coin";
        }
        requirementText.text = reqText;

        bool isCurrent = GetCurrentBackgroundId() == data.id;
        bool isPurchased = data.isPurchased;

        if (checkmark != null)
            checkmark.gameObject.SetActive(isCurrent);

        // Анимация наведения на кнопку
        if (actionButton != null)
        {
            var buttonRectTransform = actionButton.GetComponent<RectTransform>();
            actionButton.onClick.RemoveAllListeners();

            if (!isPurchased)
            {
                actionButton.interactable = CanPurchase(data);
                actionButton.onClick.AddListener(() => PurchaseBackground(data));

                // Наведение на кнопку "Купить"
                EventTrigger trigger = actionButton.GetComponent<EventTrigger>();
                if (trigger == null) trigger = actionButton.gameObject.AddComponent<EventTrigger>();

                EventTrigger.Entry entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                entryEnter.callback.AddListener((data) => {
                    buttonRectTransform.DOScale(itemScaleAmount, itemScaleDuration);
                });

                EventTrigger.Entry entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                entryExit.callback.AddListener((data) => {
                    buttonRectTransform.DOScale(1f, itemReturnDuration);
                });

                trigger.triggers.Add(entryEnter);
                trigger.triggers.Add(entryExit);

                actionButton.GetComponentInChildren<TMP_Text>().text = "Купить";
            }
            else
            {
                actionButton.interactable = !isCurrent;
                actionButton.onClick.AddListener(() => SetActiveBackground(data));
                actionButton.GetComponentInChildren<TMP_Text>().text = isCurrent ? "Текущий" : "Выбрать";
            }
        }
    }

    private bool CanPurchase(BackgroundData data)
    {
        int currentLevel = playerLevelSystem.GetCurrentLevel();
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        return currentLevel >= data.requiredLevel && totalCoins >= data.requiredCoins;
    }

    private void PurchaseBackground(BackgroundData data)
    {
        if (!CanPurchase(data))
        {
            AudioManager.Instance?.PlayBackgroundError(); // Звук ошибки
            return;
        }

        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        currentCoins -= data.requiredCoins;
        PlayerPrefs.SetInt("TotalCoins", currentCoins);
        PlayerPrefs.Save();

        data.isPurchased = true;
        PlayerPrefs.SetInt($"BG_Purchased_{data.id}", 1);
        PlayerPrefs.Save();

        // Анимация покупки
        var item = activeBackgroundItems.Find(go => go.GetComponent<RectTransform>().GetComponentInChildren<Button>().onClick.GetPersistentEventCount() > 0);
        if (item != null)
        {
            var button = item.GetComponentInChildren<Button>();
            button.GetComponent<RectTransform>().DOPunchScale(Vector3.one * 0.2f, 0.3f, 8, 0.5f);
        }

        AudioManager.Instance?.PlayBackgroundPurchase(); // Звук покупки
        UpdateText();
        PopulateBackgroundGrid();
    }

    private void SetActiveBackground(BackgroundData data)
    {
        if (!data.isPurchased) return;

        int oldId = GetCurrentBackgroundId();
        PlayerPrefs.SetInt(CURRENT_BACKGROUND_KEY, data.id);
        PlayerPrefs.Save();

        UpdateSceneBackground(data.id);

        // Анимация смены фона
        var item = activeBackgroundItems.Find(go => go.GetComponent<RectTransform>().GetComponentInChildren<Button>().onClick.GetPersistentEventCount() > 0);
        if (item != null)
        {
            var button = item.GetComponentInChildren<Button>();
            button.GetComponent<RectTransform>().DOPunchScale(Vector3.one * 0.1f, 0.2f, 4, 0.3f);
        }

        AudioManager.Instance?.PlayBackgroundSelect(); // Звук выбора
        PopulateBackgroundGrid();
    }

    private void UpdateSceneBackground(int backgroundId)
    {
        GameScene gameScene = Object.FindFirstObjectByType<GameScene>();
        if (gameScene != null)
        {
            gameScene.SetBackground(backgroundId);
        }
        else
        {
            Debug.LogWarning("GameScene не найден — фон не обновлён!");
        }
    }

    private int GetCurrentBackgroundId()
    {
        return PlayerPrefs.GetInt(CURRENT_BACKGROUND_KEY, 0);
    }

    private void NextPage()
    {
        if ((currentPage + 1) * itemsPerPage < backgroundDataList.Count)
        {
            AudioManager.Instance?.PlayButtonClick();
            currentPage++;
            PopulateBackgroundGrid();
        }
    }

    private void PrevPage()
    {
        if (currentPage > 0)
        {
            AudioManager.Instance?.PlayButtonClick();
            currentPage--;
            PopulateBackgroundGrid();
        }
    }

    private void UpdateNavigationButtons()
    {
        if (nextPageButton != null)
            nextPageButton.interactable = (currentPage + 1) * itemsPerPage < backgroundDataList.Count;

        if (prevPageButton != null)
            prevPageButton.interactable = currentPage > 0;

        if (currentPageText != null)
            currentPageText.text = $"Страница {currentPage + 1} из {(backgroundDataList.Count - 1) / itemsPerPage + 1}";
    }

    [System.Serializable]
    public class BackgroundData
    {
        public int id;
        public int requiredLevel;
        public int requiredCoins;
        public Sprite previewSprite;
        public bool isPurchased;
        public string backgroundName;
        public string description;
    }
}