using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class PuzzlePieceDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Camera uiCamera;

    private Vector2 originalAnchoredPosition;
    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector3 originalLocalScale;
    private Vector2 originalSize;

    private Vector2 dropZonePosition;

    public Vector2 targetSizeInDropZone;

    public Image pieceImage; // ссылка на изображение пазла
    public Color shineColor = new Color(1f, 1f, 0.8f, 1f); // тёплый "сияющий" цвет (почти белый с жёлтым оттенком)
    public float bounceIntensity = 1.2f; // на сколько увеличивать при прыжке
    public float effectDuration = 0.4f; // общая длительность эффекта 

    public int targetRow;
    public int targetCol;
    public bool isCorrectlyPlaced = false;
    private bool wasPlacedInDropZone = false;
    private Vector2 dropZonePositionWhenPlaced;
    private DropZone dropZone;

    public Transform scrollRectContent;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            uiCamera = canvas.worldCamera;

        pieceImage = GetComponent<Image>();

        scrollRectContent = transform.parent;

        // Гарантируем, что начальная Z-координата равна 0
        ResetZPosition();
    }

    public bool IsCorrectlyPlaced()
    {
        Vector2Int currentCell = dropZone.GetCellAtWorldPosition(transform.position, dropZone.gridSize);
        return currentCell.x == targetCol && currentCell.y == targetRow;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2 && wasPlacedInDropZone && !isCorrectlyPlaced)
        {
            ReturnToScrollRectIfIncorrect();
            PlayReturnSound();
        }
    }

    public void ReturnToScrollRectIfIncorrect()
    {
        if (!wasPlacedInDropZone || isCorrectlyPlaced)
        {
            return;
        }

        if (scrollRectContent == null)
        {
            Debug.LogWarning("ScrollRect content не найден! Невозможно вернуть пазл.");
            return;
        }

        // Включаем возможность перетаскивания
        this.enabled = true;

        // Восстанавливаем исходный размер (например, 20x20)
        float displaySize = 20f;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, displaySize);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, displaySize);

        // Анимация возврата
        rectTransform.DOScale(0f, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            // Перемещаем в ScrollRect
            rectTransform.SetParent(scrollRectContent, false);
            rectTransform.localScale = Vector3.one;
            rectTransform.SetAsLastSibling();

            // Позиция — временно (0,0), но ScrollRect сам прокрутит
            rectTransform.anchoredPosition = Vector2.zero;

            // Появление
            rectTransform.localScale = Vector3.zero;
            rectTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }

            ResetZPosition();

            Debug.Log($"Неправильный пазл [{targetRow},{targetCol}] возвращён в ScrollRect.");
        });
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isCorrectlyPlaced)
            return;

        originalAnchoredPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        originalLocalScale = rectTransform.localScale;
        originalSize = rectTransform.rect.size;

        wasPlacedInDropZone = (originalParent.GetComponent<DropZone>() != null);

        if (wasPlacedInDropZone)
        {
            dropZonePositionWhenPlaced = rectTransform.anchoredPosition;
        }

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        rectTransform.SetParent(canvas.transform, true);

        transform.SetAsLastSibling();

        // Сбрасываем Z-координату при начале перетаскивания
        ResetZPosition();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;

        PlayPickupSound();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                rectTransform,
                eventData.position,
                uiCamera,
                out Vector3 worldPos))
        {
            // Фиксируем Z-координату на 0
            worldPos.z = 0;
            rectTransform.position = worldPos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        DropZone dropZone = Object.FindFirstObjectByType<DropZone>();
        ScrollRect scrollRect = scrollRectContent?.GetComponentInParent<ScrollRect>();

        bool isInsideDropZone = false;
        bool isInsideScrollRect = false;
        bool isCorrectPlacement = false;
        Vector2 dropPosition = Vector2.zero;

        if (dropZone != null)
        {
            // Получаем позицию отпускания в локальных координатах DropZone
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dropZone.rectTransform,
                eventData.position,
                uiCamera,
                out Vector2 dropPosInDropZone))
            {
                if (dropZone.rectTransform.rect.Contains(dropPosInDropZone))
                {
                    isInsideDropZone = true;
                    dropPosition = dropPosInDropZone;

                    // Проверяем, правильная ли ячейка
                    Vector3 worldPoint = uiCamera.ScreenToWorldPoint(eventData.position);
                    Vector2Int currentCell = dropZone.GetCellAtWorldPosition(worldPoint, dropZone.gridSize);
                    isCorrectPlacement = (currentCell.x == targetCol && currentCell.y == targetRow);
                }
            }
        }

        if (!isInsideDropZone && scrollRect != null && scrollRect.viewport != null)
        {
            // Проверяем попадание в Viewport (видимую область ScrollRect)
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    scrollRect.viewport,
                    eventData.position,
                    uiCamera,
                    out Vector2 localPos))
            {
                if (scrollRect.viewport.rect.Contains(localPos))
                {
                    isInsideScrollRect = true;
                }
            }
        }

        if (isCorrectlyPlaced)
        {
            // Уже правильно размещен — ничего не делаем (на всякий случай)
            ResetZPosition();
            return;
        }

        if (isInsideDropZone && dropZone != null)
        {
            // Остаёмся в DropZone
            rectTransform.SetParent(dropZone.transform, false);
            rectTransform.localScale = Vector3.one;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSizeInDropZone.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSizeInDropZone.y);

            if (isCorrectPlacement)
            {
                // Приклеиваем точно
                float cellWidth = dropZone.rectTransform.rect.width / dropZone.gridSize;
                float cellHeight = dropZone.rectTransform.rect.height / dropZone.gridSize;
                Vector2 perfectPosition = new Vector2(
                    dropZone.rectTransform.rect.xMin + cellWidth * (targetCol + 0.5f),
                    dropZone.rectTransform.rect.yMax - cellHeight * (targetRow + 0.5f)
                );
                rectTransform.anchoredPosition = perfectPosition;
                rectTransform.SetAsFirstSibling();

                isCorrectlyPlaced = true;
                this.enabled = false; // больше нельзя тащить

                PlayBounceAndShineEffect();
                PlayCorrectSound();

                // Отключаем raycast
                if (canvasGroup != null)
                    canvasGroup.blocksRaycasts = false;
                else
                    GetComponent<Graphic>().raycastTarget = false;

                Debug.Log($"Пазл [{targetRow},{targetCol}] приклеен!");

                PuzzleGenerator puzzleGen = Object.FindFirstObjectByType<PuzzleGenerator>();
                puzzleGen?.RegisterCorrectPlacement();
            }
            else
            {

                rectTransform.anchoredPosition = dropPosition;
                isCorrectlyPlaced = false;
                rectTransform.SetAsLastSibling();
            }

            wasPlacedInDropZone = true;
            dropZonePositionWhenPlaced = rectTransform.anchoredPosition;
        }
        else if (isInsideScrollRect && scrollRectContent != null)
        {
            ReturnToScrollRect();
            PlayReturnSound();
        }
        else
        {
            if (wasPlacedInDropZone)
            {
                ReturnToDropZonePosition();
            }
            else
            {
                ReturnToOriginalPosition();
            }
        }

        ResetZPosition();
    }

    private void ReturnToScrollRect()
    {
        if (scrollRectContent == null) return;

        // Восстанавливаем исходный размер (20x20)
        float displaySize = 20f; 
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, displaySize);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, displaySize);

        // Возвращаем в ScrollRect
        rectTransform.SetParent(scrollRectContent, false);
        rectTransform.localScale = Vector3.one;
        rectTransform.SetAsLastSibling();
        rectTransform.anchoredPosition = Vector2.zero; // ScrollRect сам разместит

        // Сбрасываем флаги
        wasPlacedInDropZone = false;
        isCorrectlyPlaced = false;
        this.enabled = true; // можно снова тащить

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        ResetZPosition();

        Debug.Log($"Пазл [{targetRow},{targetCol}] возвращён в ScrollRect через перетаскивание.");
    }

    private void PlayPickupSound()
    {
        AudioManager.Instance?.PlayOneShotSFX(AudioManager.Instance.puzzlePickupSFX);
    }

    public void PlayCorrectSound()
    {
        AudioManager.Instance?.PlayOneShotSFX(AudioManager.Instance.puzzleCorrectSFX);
    }

    private void PlayReturnSound()
    {
        AudioManager.Instance?.PlayOneShotSFX(AudioManager.Instance.puzzleReturnSFX);
    }

    private void ReturnToDropZonePosition()
    {
        DropZone dropZone = Object.FindFirstObjectByType<DropZone>();
        if (dropZone != null)
        {
            rectTransform.SetParent(dropZone.transform, false);
            rectTransform.anchoredPosition = dropZonePositionWhenPlaced;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSizeInDropZone.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSizeInDropZone.y);
            rectTransform.localScale = Vector3.one;
        }
        else
        {
            ReturnToOriginalPosition();
        }
    }

    private void ReturnToOriginalPosition()
    {
        rectTransform.SetParent(originalParent, false);
        rectTransform.SetSiblingIndex(originalSiblingIndex);
        rectTransform.anchoredPosition = originalAnchoredPosition;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSize.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize.y);
        rectTransform.localScale = originalLocalScale;
    }

    public void PlayBounceAndShineEffect()
    {
        if (pieceImage == null || rectTransform == null) return;

        Vector3 originalScale = rectTransform.localScale;
        Color originalColor = pieceImage.color;

        // 1. Плавное "приземление" на место (если нужно — можно пропустить, если уже на месте)
        // В вашем случае пазл уже на месте, поэтому начнём с паузы

        // 2. Короткая задержка — создаёт ощущение "ожидания"
        float delay = 0.15f;

        // 3. Импульс: лёгкое увеличение → возврат с отскоком
        rectTransform.DOScale(originalScale * 1.08f, 0.25f)
            .SetDelay(delay)
            .SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                rectTransform.DOScale(originalScale, 0.3f).SetEase(Ease.OutBack);
            });

        // 4. Сияние: не просто цвет, а кратковременное осветление + лёгкое мерцание
        Color highlightColor = new Color(
            Mathf.Min(originalColor.r * 1.4f, 1f),
            Mathf.Min(originalColor.g * 1.4f, 1f),
            Mathf.Min(originalColor.b * 1.4f, 1f),
            originalColor.a
        );

        pieceImage.DOColor(highlightColor, 0.2f)
            .SetDelay(delay)
            .OnComplete(() =>
            {
                pieceImage.DOColor(originalColor, 0.4f).SetEase(Ease.OutSine);
            });

        // 5. Очень лёгкая пульсация (1 цикл) — как "эхо" успеха
        DOVirtual.DelayedCall(delay + 0.6f, () =>
        {
            rectTransform.DOPunchScale(originalScale * 0.02f, 0.6f, 1, 0.5f);
        });

    }

    // Новый метод для сброса Z-координаты
    private void ResetZPosition()
    {
        // Сбрасываем позицию в мировых координатах
        Vector3 position = rectTransform.position;
        position.z = 0;
        rectTransform.position = position;

        // Сбрасываем локальную позицию
        Vector3 localPosition = rectTransform.localPosition;
        localPosition.z = 0;
        rectTransform.localPosition = localPosition;
    }

}