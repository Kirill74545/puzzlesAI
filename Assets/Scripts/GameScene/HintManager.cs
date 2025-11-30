using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class HintManager : MonoBehaviour
{
    [Header("Ссылки")]
    public Image hintImage;           // Изображение подсказки (транспарентное)
    public Button hintButton;         // Кнопка подсказки (показать изображение)
    public Button autoPlaceButton;    // Кнопка подсказки (автоматическое размещение)
    public DropZone dropZone;         // Дропзона

    [Header("UI Элементы")]
    public TMP_Text imageHintCountText; // Текст для отображения количества подсказок-изображений
    public TMP_Text autoPlaceHintCountText; // Текст для отображения количества авто-подсказок
    public Button shopButton;         // Кнопка магазина
    public GameObject shopPanel;      // Панель магазина (активируется кнопкой)

    [Header("Настройки подсказки изображения")]
    public float hintDuration = 10f;  // Длительность показа подсказки в секундах
    public float fadeDuration = 0.5f; // Время анимации появления/исчезновения
    public float hintTransparency = 0.4f; // Прозрачность подсказки

    [Header("Настройки подсказки автоматического размещения")]
    public int maxAutoPlaceHints = 5; // Максимальное количество подсказок автоматического размещения

    [Header("Кнопки магазина (изображения)")]
    public Button imageHintX1;
    public Button imageHintX3;
    public Button imageHintX5;

    [Header("Кнопки магазина (авто-подсказки)")]
    public Button autoHintX1;
    public Button autoHintX3;
    public Button autoHintX5;

    [Header("Цены в магазине")]
    public int imageHintPriceX1 = 15;
    public int imageHintPriceX3 = 45;
    public int imageHintPriceX5 = 75;

    public int autoPlaceHintPriceX1 = 5;
    public int autoPlaceHintPriceX3 = 15;
    public int autoPlaceHintPriceX5 = 25;

    private int availableHints = 3;           // Количество доступных подсказок изображения
    private int availableAutoPlaceHints = 5;  // Количество доступных подсказок автоматического размещения

    // Добавленные поля для анимаций
    private CanvasGroup shopCanvasGroup;
    private Vector3 shopOriginalLocalPosition;
    private const float slideDuration = 0.3f; // Длительность анимации появления/исчезновения панели

    private bool usedHintsThisLevel = false;

    void Start()
    {
        // Инициализация UI подсказок
        UpdateHintCountTexts();
        shopPanel.SetActive(true); // Важно: сначала активировать, чтобы получить RectTransform и CanvasGroup
        shopCanvasGroup = shopPanel.GetComponent<CanvasGroup>();
        if (shopCanvasGroup == null) shopCanvasGroup = shopPanel.AddComponent<CanvasGroup>();

        RectTransform shopRT = shopPanel.GetComponent<RectTransform>();
        shopOriginalLocalPosition = shopRT.anchoredPosition3D;

        // Скрыть панель сразу после инициализации
        shopPanel.SetActive(false);

        // Настройка основных кнопок
        if (hintButton != null) hintButton.onClick.AddListener(ShowHint);
        if (autoPlaceButton != null) autoPlaceButton.onClick.AddListener(UseAutoPlaceHint);
        if (shopButton != null) shopButton.onClick.AddListener(ToggleShopPanel);

        // Настройка кнопок магазина
        SetupShopButtons();

        // Загрузка сохранённых значений
        availableHints = PlayerPrefs.GetInt("AvailableHints", 3);
        availableAutoPlaceHints = PlayerPrefs.GetInt("AvailableAutoPlaceHints", 5);

        UpdateHintButtonState();
        UpdateAutoPlaceButtonState();
        UpdateHintCountTexts();

        ResetHintsUsedFlag();
    }

    public void ResetHintsUsedFlag()
    {
        usedHintsThisLevel = false;
        Debug.Log("[HintManager] Флаг использования подсказок сброшен.");
    }

    public bool WereHintsUsedThisLevel()
    {
        return usedHintsThisLevel;
    }

    // Переключение видимости панели магазина
    public void ToggleShopPanel()
    {
        if (shopPanel != null)
        {
            bool isActive = shopPanel.activeSelf;
            if (isActive)
            {
                AnimateShopPanelOut();
            }
            else
            {
                shopPanel.SetActive(true);
                AnimateShopPanelIn();
                UpdateShopUI();
            }
        }
    }

    private void AnimateShopPanelIn()
    {
        RectTransform shopRT = shopPanel.GetComponent<RectTransform>();
        // Смещаем панель за пределы экрана (например, вниз)
        shopRT.anchoredPosition3D = new Vector3(shopOriginalLocalPosition.x, shopOriginalLocalPosition.y - 200, shopOriginalLocalPosition.z);

        // Устанавливаем прозрачность в 0
        shopCanvasGroup.alpha = 0f;
        shopCanvasGroup.interactable = false;
        shopCanvasGroup.blocksRaycasts = false;

        // Анимация перемещения и появления
        shopRT.DOAnchorPos3D(shopOriginalLocalPosition, slideDuration).SetEase(Ease.OutBack);
        shopCanvasGroup.DOFade(1f, slideDuration).OnComplete(() =>
        {
            shopCanvasGroup.interactable = true;
            shopCanvasGroup.blocksRaycasts = true;
        });
    }

    private void AnimateShopPanelOut()
    {
        RectTransform shopRT = shopPanel.GetComponent<RectTransform>();

        // Анимация исчезновения и скрытия
        shopRT.DOAnchorPos3D(new Vector3(shopOriginalLocalPosition.x, shopOriginalLocalPosition.y - 200, shopOriginalLocalPosition.z), slideDuration).SetEase(Ease.InBack);
        shopCanvasGroup.DOFade(0f, slideDuration).OnComplete(() =>
        {
            shopPanel.SetActive(false);
        });
    }

    private void SetupShopButtons()
    {
        // Кнопки для подсказок-изображений
        if (imageHintX1 != null) imageHintX1.onClick.AddListener(() => BuyImageHints(1, imageHintPriceX1));
        if (imageHintX3 != null) imageHintX3.onClick.AddListener(() => BuyImageHints(3, imageHintPriceX3));
        if (imageHintX5 != null) imageHintX5.onClick.AddListener(() => BuyImageHints(5, imageHintPriceX5));

        // Кнопки для авто-подсказок
        if (autoHintX1 != null) autoHintX1.onClick.AddListener(() => BuyAutoPlaceHints(1, autoPlaceHintPriceX1));
        if (autoHintX3 != null) autoHintX3.onClick.AddListener(() => BuyAutoPlaceHints(3, autoPlaceHintPriceX3));
        if (autoHintX5 != null) autoHintX5.onClick.AddListener(() => BuyAutoPlaceHints(5, autoPlaceHintPriceX5));

        // Подписываемся на обновление состояния кнопок при изменении монет
        // Можно вызвать обновление при старте
        UpdateAllButtonStates();
    }

    // Обновление текстовых полей с количеством подсказок
    void UpdateHintCountTexts()
    {
        if (imageHintCountText != null) imageHintCountText.text = availableHints.ToString();
        if (autoPlaceHintCountText != null) autoPlaceHintCountText.text = availableAutoPlaceHints.ToString();
    }

    public void BuyImageHints(int count, int price)
    {
        if (!CheckAndDeductCoins(price))
        {
            AudioManager.Instance?.PlayOneShotSFX(AudioManager.Instance.hintPurchaseFailSFX);
            return;
        }

        AudioManager.Instance?.PlayOneShotSFX(AudioManager.Instance.hintPurchaseSuccessSFX);

        availableHints += count;
        PlayerPrefs.SetInt("AvailableHints", availableHints);
        PlayerPrefs.Save();

        UpdateHintCountTexts();
        UpdateHintButtonState();
        UpdateAllButtonStates(); // Обновить состояние кнопок магазина
        Debug.Log($"Куплено {count} подсказок-изображений за {price} монет");
    }

    // Покупка авто-подсказок
    public void BuyAutoPlaceHints(int count, int price)
    {
        if (!CheckAndDeductCoins(price))
        {
            AudioManager.Instance?.PlayOneShotSFX(AudioManager.Instance.hintPurchaseFailSFX);
            return;
        }
        AudioManager.Instance?.PlayOneShotSFX(AudioManager.Instance.hintPurchaseSuccessSFX);

        availableAutoPlaceHints += count;
        PlayerPrefs.SetInt("AvailableAutoPlaceHints", availableAutoPlaceHints);
        PlayerPrefs.Save();

        UpdateHintCountTexts();
        UpdateAutoPlaceButtonState();
        UpdateAllButtonStates(); // Обновить состояние кнопок магазина
        Debug.Log($"Куплено {count} авто-подсказок за {price} монет");
    }

    private bool CheckAndDeductCoins(int price)
    {
        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (currentCoins < price)
        {
            Debug.Log("Недостаточно монет для покупки!");
            // Можно добавить визуальное уведомление
            return false;
        }

        currentCoins -= price;
        PlayerPrefs.SetInt("TotalCoins", currentCoins);
        PlayerPrefs.Save();

        // Обновляем отображение монет
        CoinManager coinManager = Object.FindFirstObjectByType<CoinManager>();
        if (coinManager != null) coinManager.UpdateDisplay(currentCoins);

        return true;
    }

    // Метод для обновления UI магазина (вызывать при открытии панели)
    void UpdateShopUI()
    {
        UpdateAllButtonStates();
    }

    private void UpdateAllButtonStates()
    {
        UpdateShopButtonPriceAndState(imageHintX1, imageHintPriceX1);
        UpdateShopButtonPriceAndState(imageHintX3, imageHintPriceX3);
        UpdateShopButtonPriceAndState(imageHintX5, imageHintPriceX5);

        UpdateShopButtonPriceAndState(autoHintX1, autoPlaceHintPriceX1);
        UpdateShopButtonPriceAndState(autoHintX3, autoPlaceHintPriceX3);
        UpdateShopButtonPriceAndState(autoHintX5, autoPlaceHintPriceX5);
    }

    private void UpdateShopButtonPriceAndState(Button button, int price)
    {
        if (button == null) return;

        // Поиск TMP_Text внутри кнопки
        TMP_Text priceText = button.GetComponentInChildren<TMP_Text>();
        if (priceText != null)
        {
            // Извлекаем текущий текст (например "x1") и добавляем цену
            string currentText = priceText.text.Split('\n')[0];
            priceText.text = $"{currentText}\n{price}";
        }

        // Обновляем состояние кнопки (серая/активная)
        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        button.interactable = currentCoins >= price;
    }

    void ShowHint()
    {
        // Проверка, есть ли доступные подсказки
        if (availableHints <= 0)
        {
            Debug.Log("Нет доступных подсказок изображения. Купите подсказки за монеты.");
            // Здесь можно показать окно покупки подсказок
            return;
        }

        usedHintsThisLevel = true;
        Debug.Log("[HintManager] Использована подсказка изображения.");

        // Уменьшаем количество доступных подсказок
        availableHints--;
        PlayerPrefs.SetInt("AvailableHints", availableHints);
        PlayerPrefs.Save();
        UpdateHintCountTexts();
        UpdateHintButtonState();

        if (hintImage != null && GameData.UserImage != null)
        {
            // Создаём спрайт из GameData.UserImage
            Sprite hintSprite = Sprite.Create(
                GameData.UserImage,
                new Rect(0, 0, GameData.UserImage.width, GameData.UserImage.height),
                new Vector2(0.5f, 0.5f),
                100
            );

            hintImage.sprite = hintSprite;

            // Устанавливаем прозрачность
            Color color = hintImage.color;
            color.a = hintTransparency;
            hintImage.color = color;

            // Устанавливаем размер и позицию под дропзоной
            RectTransform hintRT = hintImage.GetComponent<RectTransform>();
            RectTransform dropZoneRT = dropZone.GetComponent<RectTransform>();

            hintRT.SetParent(dropZoneRT.parent, false); // Перемещаем в тот же родитель
            hintRT.anchorMin = dropZoneRT.anchorMin;
            hintRT.anchorMax = dropZoneRT.anchorMax;
            hintRT.anchoredPosition = dropZoneRT.anchoredPosition;
            hintRT.sizeDelta = dropZoneRT.sizeDelta;

            // Показываем подсказку с анимацией
            hintImage.gameObject.SetActive(true);
            hintImage.DOFade(hintTransparency, fadeDuration);

            // Запускаем таймер для скрытия подсказки
            StartCoroutine(HideHintAfterDelay());
        }
        else
        {
            Debug.LogWarning("Нет доступного изображения для подсказки (GameData.UserImage == null).");
        }
    }

    System.Collections.IEnumerator HideHintAfterDelay()
    {
        yield return new WaitForSeconds(hintDuration);
        if (hintImage != null)
        {
            hintImage.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                hintImage.gameObject.SetActive(false);
            });
        }
    }

    void UseAutoPlaceHint()
    {
        // Проверка, есть ли доступные подсказки автоматического размещения
        if (availableAutoPlaceHints <= 0)
        {
            Debug.Log("Нет доступных подсказок автоматического размещения. Купите подсказки за монеты.");
            // Здесь можно показать окно покупки подсказок
            return;
        }

        usedHintsThisLevel = true;

        // Уменьшаем количество доступных подсказок автоматического размещения
        availableAutoPlaceHints--;
        PlayerPrefs.SetInt("AvailableAutoPlaceHints", availableAutoPlaceHints);
        PlayerPrefs.Save();

        UpdateHintCountTexts();
        UpdateAutoPlaceButtonState();

        // Находим неправильно размещенные детали
        List<PuzzlePieceDragHandler> misplacedPieces = FindMisplacedPieces();

        if (misplacedPieces.Count > 0)
        {
            // Выбираем случайную деталь
            System.Random rng = new System.Random();
            PuzzlePieceDragHandler selectedPiece = misplacedPieces[rng.Next(misplacedPieces.Count)];

            // Перемещаем деталь в правильное место
            MovePieceToCorrectPosition(selectedPiece);
        }
        else
        {
            Debug.Log("Все детали уже на своих местах!");
            // Восстанавливаем подсказку, если нет неправильно размещённых деталей
            availableAutoPlaceHints++;
            PlayerPrefs.SetInt("AvailableAutoPlaceHints", availableAutoPlaceHints);
            PlayerPrefs.Save();
            UpdateAutoPlaceButtonState();
        }
    }

    List<PuzzlePieceDragHandler> FindMisplacedPieces()
    {
        List<PuzzlePieceDragHandler> misplacedPieces = new List<PuzzlePieceDragHandler>();
        PuzzlePieceDragHandler[] allPieces = FindObjectsOfType<PuzzlePieceDragHandler>();

        foreach (PuzzlePieceDragHandler piece in allPieces)
        {
            if (!piece.isCorrectlyPlaced)
            {
                misplacedPieces.Add(piece);
            }
        }

        return misplacedPieces;
    }

    // В классе HintManager.cs

    void MovePieceToCorrectPosition(PuzzlePieceDragHandler piece)
    {
        if (piece == null || dropZone == null)
        {
            Debug.LogError("MovePieceToCorrectPosition: Деталь или DropZone не найдены!");
            return;
        }

        RectTransform dropZoneRT = dropZone.GetComponent<RectTransform>();
        RectTransform pieceRT = piece.GetComponent<RectTransform>();

        // --- ОБЩАЯ ЧАСТЬ ДЛЯ ВСЕХ ПАЗЛОВ ---

        // 1. Перемещаем деталь в DropZone, если она еще не там
        if (pieceRT.parent != dropZoneRT)
        {
            pieceRT.SetParent(dropZoneRT, false);
        }

        // 2. Устанавливаем правильный размер
        pieceRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, piece.targetSizeInDropZone.x);
        pieceRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, piece.targetSizeInDropZone.y);

        // 3. Устанавливаем anchors и pivot в центр для точного позиционирования
        pieceRT.anchorMin = new Vector2(0.5f, 0.5f);
        pieceRT.anchorMax = new Vector2(0.5f, 0.5f);
        pieceRT.pivot = new Vector2(0.5f, 0.5f);

        // 4. Вычисляем и устанавливаем правильную позицию в зависимости от типа пазла
        Vector2 targetAnchoredPosition = Vector2.zero;

        if (piece.isTriangulationPiece)
        {
            // --- ЛОГИКА ДЛЯ ТРИАНГУЛЯЦИОННЫХ ПАЗЛОВ ---
            // Позиция уже была предрассчитана в TriangulationPuzzleGenerator
            // и сохранена в piece.targetPosition
            targetAnchoredPosition = piece.targetPosition;
            Debug.Log($"Подсказка: Размещаю треугольный пазл {piece.triangleIndex} на позицию {targetAnchoredPosition}");
        }
        else
        {
            // --- ЛОГИКА ДЛЯ КЛАССИЧЕСКИХ ПАЗЛОВ ---
            // Вычисляем позицию по сетке
            targetAnchoredPosition = dropZone.GetCorrectCellPosition(piece.targetRow, piece.targetCol);
            Debug.Log($"Подсказка: Размещаю классический пазл [{piece.targetRow},{piece.targetCol}] на позицию {targetAnchoredPosition}");
        }

        // Устанавливаем итоговую позицию
        pieceRT.anchoredPosition = targetAnchoredPosition;
        pieceRT.localPosition = new Vector3(pieceRT.localPosition.x, pieceRT.localPosition.y, 0); // Сбрасываем Z

        // --- ФИНАЛЬНЫЕ ДЕЙСТВИЯ ДЛЯ ВСЕХ ПАЗЛОВ ---

        // Помечаем как правильно размещенный
        piece.isCorrectlyPlaced = true;
        piece.enabled = false; // Отключаем перетаскивание

        // Отключаем raycast
        CanvasGroup canvasGroup = piece.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 1f;
        }
        else
        {
            Graphic graphic = piece.GetComponent<Graphic>();
            if (graphic != null) graphic.raycastTarget = false;
        }

        // Воспроизводим эффекты
        piece.PlayBounceAndShineEffect();
        piece.PlayCorrectSound();

        // Регистрируем правильное размещение в общем генераторе
        PuzzleGenerator puzzleGen = Object.FindFirstObjectByType<PuzzleGenerator>();
        if (puzzleGen != null)
        {
            puzzleGen.RegisterCorrectPlacement();
        }
    }

    void UpdateHintButtonState()
    {
        if (hintButton != null)
        {
            hintButton.interactable = availableHints > 0;
        }
    }

    void UpdateAutoPlaceButtonState()
    {
        if (autoPlaceButton != null)
        {
            autoPlaceButton.interactable = availableAutoPlaceHints > 0;
        }
    }
}