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

    private bool isInDropZone = false;
    private Vector2 dropZonePosition;

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
        originalAnchoredPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        originalLocalScale = rectTransform.localScale;
        originalSize = rectTransform.rect.size;

        isInDropZone = (originalParent.GetComponent<DropZone>() != null);

        if (isInDropZone)
        {
            dropZonePosition = rectTransform.anchoredPosition;
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

        if (dropZone != null && dropZone.IsFullyInside(rectTransform))
        {
            AttachToDropZone(dropZone, eventData);
            isInDropZone = true;
        }
        else
        {
            if (isInDropZone)
            {
                ReturnToDropZone();
            }
            else
            {
                ReturnToOriginalPosition();
            }
        }

        // Гарантируем, что Z-координата сброшена после любого действия
        ResetZPosition();
    }

    private void AttachToDropZone(DropZone dropZone, PointerEventData eventData)
    {
        // Просто меняем родителя и центрируем, сохраняя текущий визуальный размер
        rectTransform.SetParent(dropZone.transform, true); // true - сохраняем мировые координаты

        // Принудительно сохраняем размер
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSize.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize.y);
        ResetZPosition();
    }


    private void ReturnToDropZone()
    {
        DropZone dropZone = Object.FindFirstObjectByType<DropZone>();
        if (dropZone != null)
        {
            rectTransform.SetParent(dropZone.transform, false);
            rectTransform.anchoredPosition = dropZonePosition;

            // Восстанавливаем размер и масштаб
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSize.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize.y);
            rectTransform.localScale = originalLocalScale;

            // Сбрасываем Z-координату
            ResetZPosition();
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
        rectTransform.localScale = originalLocalScale;

        // Восстанавливаем размер
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSize.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize.y);

        // Сбрасываем Z-координату
        ResetZPosition();

        // Обновляем Layout только если возвращаемся в ScrollView
        if (originalParent != null && originalParent.GetComponent<DropZone>() == null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)originalParent);
        }
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