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

    [Header("Цены в магазине")]
    public int imageHintCost = 15;    // Стоимость одной подсказки-изображения
    public int autoPlaceHintCost = 5; // Стоимость одной авто-подсказки

    private int availableHints = 3;           // Количество доступных подсказок изображения
    private int availableAutoPlaceHints = 5;  // Количество доступных подсказок автоматического размещения


    void Start()
    {
        // Инициализация UI подсказок
        if (imageHintCountText != null) imageHintCountText.text = availableHints.ToString();
        if (autoPlaceHintCountText != null) autoPlaceHintCountText.text = availableAutoPlaceHints.ToString();

        // Скрытие панели магазина при старте
        if (shopPanel != null) shopPanel.SetActive(false);

        // Настройка кнопок
        if (hintButton != null) hintButton.onClick.AddListener(ShowHint);
        if (autoPlaceButton != null) autoPlaceButton.onClick.AddListener(UseAutoPlaceHint);
        if (shopButton != null) shopButton.onClick.AddListener(ToggleShopPanel);

        // Загрузка сохранённых значений
        availableHints = PlayerPrefs.GetInt("AvailableHints", 3);
        availableAutoPlaceHints = PlayerPrefs.GetInt("AvailableAutoPlaceHints", maxAutoPlaceHints);

        // Обновление состояния кнопок и UI
        UpdateHintButtonState();
        UpdateAutoPlaceButtonState();
        UpdateHintCountTexts();
    }

    // Переключение видимости панели магазина
    public void ToggleShopPanel()
    {
        if (shopPanel != null)
        {
            bool isActive = shopPanel.activeSelf;
            shopPanel.SetActive(!isActive);

            // Обновляем UI магазина при открытии (например, актуальное количество монет)
            if (!isActive) UpdateShopUI();
        }
    }

    // Обновление текстовых полей с количеством подсказок
    void UpdateHintCountTexts()
    {
        if (imageHintCountText != null) imageHintCountText.text = availableHints.ToString();
        if (autoPlaceHintCountText != null) autoPlaceHintCountText.text = availableAutoPlaceHints.ToString();
    }

    // Метод для обновления UI магазина (вызывать при открытии панели)
    void UpdateShopUI()
    {
        // Здесь можно обновить отображение цен или количество монет в магазине
        // Например: shopCoinText.text = PlayerPrefs.GetInt("TotalCoins", 0).ToString();
    }

    // Покупка подсказок-изображений
    public void BuyImageHints(int count)
    {
        int totalCost = count * imageHintCost;
        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (currentCoins >= totalCost)
        {
            // Списание монет
            currentCoins -= totalCost;
            PlayerPrefs.SetInt("TotalCoins", currentCoins);

            // Добавление подсказок
            availableHints += count;
            PlayerPrefs.SetInt("AvailableHints", availableHints);

            PlayerPrefs.Save();

            // Обновление UI
            UpdateHintCountTexts();
            UpdateHintButtonState();

            // Обновление отображения монет (через CoinManager)
            CoinManager coinManager = Object.FindFirstObjectByType<CoinManager>();
            if (coinManager != null) coinManager.UpdateDisplay(currentCoins);

            Debug.Log($"Куплено {count} подсказок-изображений за {totalCost} монет");
        }
        else
        {
            Debug.Log("Недостаточно монет!");
            // Здесь можно показать предупреждение в UI
        }
    }

    // Покупка авто-подсказок
    public void BuyAutoPlaceHints(int count)
    {
        int totalCost = count * autoPlaceHintCost;
        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (currentCoins >= totalCost)
        {
            // Списание монет
            currentCoins -= totalCost;
            PlayerPrefs.SetInt("TotalCoins", currentCoins);

            // Добавление подсказок
            availableAutoPlaceHints += count;
            PlayerPrefs.SetInt("AvailableAutoPlaceHints", availableAutoPlaceHints);

            PlayerPrefs.Save();

            // Обновление UI
            UpdateHintCountTexts();
            UpdateAutoPlaceButtonState();

            // Обновление отображения монет
            CoinManager coinManager = Object.FindFirstObjectByType<CoinManager>();
            if (coinManager != null) coinManager.UpdateDisplay(currentCoins);

            Debug.Log($"Куплено {count} авто-подсказок за {totalCost} монет");
        }
        else
        {
            Debug.Log("Недостаточно монет!");
            // Здесь можно показать предупреждение в UI
        }
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

    void MovePieceToCorrectPosition(PuzzlePieceDragHandler piece)
    {
        if (dropZone == null)
        {
            Debug.LogError("DropZone не назначен в HintManager!");
            return;
        }

        RectTransform dropZoneRT = dropZone.GetComponent<RectTransform>();

        if (dropZoneRT == null)
        {
            Debug.LogError("DropZone RectTransform не найден!");
            return;
        }

        // Убедимся, что дропзона имеет правильные размеры
        LayoutRebuilder.ForceRebuildLayoutImmediate(dropZoneRT);
        Canvas.ForceUpdateCanvases();

        // Получаем размеры дропзоны
        float dropZoneWidth = dropZoneRT.rect.width;
        float dropZoneHeight = dropZoneRT.rect.height;

        // Проверка, чтобы избежать деления на ноль
        if (dropZone.gridSize <= 0)
        {
            Debug.LogError("DropZone gridSize <= 0!");
            return;
        }

        // Вычисляем размер ячейки
        float cellWidth = dropZoneWidth / dropZone.gridSize;
        float cellHeight = dropZoneHeight / dropZone.gridSize;

        Debug.Log($"DropZone размеры: {dropZoneWidth}x{dropZoneHeight}");
        Debug.Log($"Размер ячейки: {cellWidth}x{cellHeight}");
        Debug.Log($"gridSize: {dropZone.gridSize}, targetRow: {piece.targetRow}, targetCol: {piece.targetCol}");

        float x = -(dropZoneWidth / 2f) + (piece.targetCol * cellWidth) + (cellWidth / 2f);
        float y = (dropZoneHeight / 2f) - (piece.targetRow * cellHeight) - (cellHeight / 2f);

        Vector2 targetPosition = dropZone.GetCorrectCellPosition(piece.targetRow, piece.targetCol);

        Debug.Log($"Рассчитанная позиция: ({x}, {y}) для ячейки [{piece.targetRow}, {piece.targetCol}]");

        RectTransform pieceRT = piece.GetComponent<RectTransform>();

        // Сохраняем оригинальные настройки
        Vector2 originalAnchorMin = pieceRT.anchorMin;
        Vector2 originalAnchorMax = pieceRT.anchorMax;
        Vector2 originalPivot = pieceRT.pivot;

        // Устанавливаем anchor и pivot в центр для точного позиционирования
        pieceRT.anchorMin = new Vector2(0.5f, 0.5f);
        pieceRT.anchorMax = new Vector2(0.5f, 0.5f);
        pieceRT.pivot = new Vector2(0.5f, 0.5f);

        // Если деталь находится в ScrollRect, извлекаем её оттуда
        if (piece.scrollRectContent != null && pieceRT.parent == piece.scrollRectContent)
        {
            Debug.Log("Деталь находится в ScrollRect, перемещаем в дропзону");
            pieceRT.SetParent(dropZoneRT, false);
        }
        else
        {
            Debug.Log("Деталь уже не в ScrollRect, устанавливаем позицию");
            // Убедимся, что деталь находится в дропзоне
            if (pieceRT.parent != dropZoneRT)
            {
                pieceRT.SetParent(dropZoneRT, false);
            }
        }

        // Устанавливаем правильный размер для ячейки в дропзоне
        pieceRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellWidth);
        pieceRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellHeight);

        // Устанавливаем позицию
        pieceRT.anchoredPosition = targetPosition;

        // Сбрасываем локальную позицию Z
        Vector3 localPos = pieceRT.localPosition;
        localPos.z = 0;
        pieceRT.localPosition = localPos;

        // Визуальная проверка положения
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, pieceRT.position);
        Debug.Log($"Позиция на экране: {screenPos}");

        // Обновляем состояние пазла
        piece.isCorrectlyPlaced = true;
        piece.enabled = false; // отключаем перетаскивание

        // Обновляем CanvasGroup
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

        // Регистрируем правильное размещение
        PuzzleGenerator puzzleGen = FindObjectOfType<PuzzleGenerator>();
        if (puzzleGen != null)
        {
            puzzleGen.RegisterCorrectPlacement();
        }

        Debug.Log($"Автоматически размещена деталь [{piece.targetRow},{piece.targetCol}] на позицию: {targetPosition}");
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

    // Метод для покупки подсказок изображения за монеты
    public void PurchaseHintImage(int count, int costPerHint)
    {
        int totalCost = count * costPerHint;
        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (currentCoins >= totalCost)
        {
            // Снимаем монеты
            currentCoins -= totalCost;
            PlayerPrefs.SetInt("TotalCoins", currentCoins);
            PlayerPrefs.Save();

            // Добавляем подсказки
            availableHints += count;
            PlayerPrefs.SetInt("AvailableHints", availableHints);
            PlayerPrefs.Save();

            UpdateHintButtonState();

            Debug.Log($"Куплено {count} подсказок изображения за {totalCost} монет. Осталось монет: {currentCoins}, подсказок: {availableHints}");
        }
        else
        {
            Debug.Log("Недостаточно монет для покупки подсказок изображения.");
        }
    }

    // Метод для покупки подсказок автоматического размещения за монеты
    public void PurchaseAutoPlaceHint(int count)
    {
        int totalCost = count * autoPlaceHintCost;
        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (currentCoins >= totalCost)
        {
            // Снимаем монеты
            currentCoins -= totalCost;
            PlayerPrefs.SetInt("TotalCoins", currentCoins);
            PlayerPrefs.Save();

            // Добавляем подсказки
            availableAutoPlaceHints += count;
            PlayerPrefs.SetInt("AvailableAutoPlaceHints", availableAutoPlaceHints);
            PlayerPrefs.Save();

            UpdateAutoPlaceButtonState();

            Debug.Log($"Куплено {count} подсказок автоматического размещения за {totalCost} монет. Осталось монет: {currentCoins}, подсказок: {availableAutoPlaceHints}");
        }
        else
        {
            Debug.Log("Недостаточно монет для покупки подсказок автоматического размещения.");
        }
    }

    // Методы для получения текущего количества подсказок (для UI)
    public int GetAvailableHintImageCount()
    {
        return availableHints;
    }

    public int GetAvailableAutoPlaceHintCount()
    {
        return availableAutoPlaceHints;
    }
}