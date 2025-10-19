using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    public int targetRow;
    public int targetCol;
    public bool isCorrectlyPlaced = false;
    private bool wasPlacedInDropZone = false;
    private Vector2 dropZonePositionWhenPlaced;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            uiCamera = canvas.worldCamera;

        // Гарантируем, что начальная Z-координата равна 0
        ResetZPosition();
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

        // Сбрасываем Z-координату при начале перетаскивания
        ResetZPosition();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
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
        bool isInsideDropZone = false;
        bool isCorrectPlacement = false;
        Vector2 dropPosition = Vector2.zero;

        if (dropZone != null)
        {
            // Получаем позицию отпускания в локальных координатах DropZone
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    dropZone.rectTransform,
                    eventData.position,
                    uiCamera,
                    out dropPosition))
            {
                if (dropZone.rectTransform.rect.Contains(dropPosition))
                {
                    isInsideDropZone = true;

                    // Определяем, какая ячейка под этой позицией
                    Vector3 worldPoint = uiCamera.ScreenToWorldPoint(eventData.position);
                    Vector2Int currentCell = dropZone.GetCellAtWorldPosition(worldPoint, dropZone.gridSize);

                    if (currentCell.x == targetCol && currentCell.y == targetRow)
                    {
                        isCorrectPlacement = true;
                    }
                }
            }
        }

        if (isInsideDropZone && dropZone != null)
        {
            // Всегда остаёмся в DropZone
            rectTransform.SetParent(dropZone.transform, false);
            rectTransform.localScale = Vector3.one;

            // Устанавливаем размер
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSizeInDropZone.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSizeInDropZone.y);

            if (isCorrectPlacement)
            {
                // Приклеиваем точно по центру своей ячейки 
                float cellWidth = dropZone.rectTransform.rect.width / dropZone.gridSize;
                float cellHeight = dropZone.rectTransform.rect.height / dropZone.gridSize;

                Vector2 perfectPosition = new Vector2(
                    dropZone.rectTransform.rect.xMin + cellWidth * (targetCol + 0.5f),
                    dropZone.rectTransform.rect.yMax - cellHeight * (targetRow + 0.5f)
                );

                rectTransform.anchoredPosition = perfectPosition;
                isCorrectlyPlaced = true;
                this.enabled = false; // больше нельзя тащить
                Debug.Log($"Пазл [{targetRow},{targetCol}] приклеен!");
            }
            else
            {
                rectTransform.anchoredPosition = dropPosition;
                isCorrectlyPlaced = false;
                // Можно снова тащить
            }

            wasPlacedInDropZone = true;
            dropZonePositionWhenPlaced = rectTransform.anchoredPosition;
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