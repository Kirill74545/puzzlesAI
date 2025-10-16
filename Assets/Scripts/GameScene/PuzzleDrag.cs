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

        bool shouldAttach = false;
        Vector2 targetPosition = Vector2.zero;

        if (dropZone != null)
        {
            // Проверяем пересечение: хотя бы 1 угол внутри ИЛИ центр внутри
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            int insideCount = 0;
            foreach (var corner in corners)
            {
                Vector2 local;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        dropZone.rectTransform,
                        uiCamera.WorldToScreenPoint(corner),
                        uiCamera,
                        out local) && dropZone.rectTransform.rect.Contains(local))
                {
                    insideCount++;
                }
            }

            // Также проверим центр
            Vector2 center = (corners[0] + corners[2]) / 2f;
            Vector2 localCenter;
            bool centerInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    dropZone.rectTransform,
                    uiCamera.WorldToScreenPoint(center),
                    uiCamera,
                    out localCenter) && dropZone.rectTransform.rect.Contains(localCenter);

            // Если хотя бы один угол или центр внутри — считаем, что пересекается
            if (insideCount > 0 || centerInside)
            {
                shouldAttach = true;
                targetPosition = dropZone.GetClampedPositionInside(rectTransform);
            }
        }

        if (shouldAttach)
        {
            // Прикрепляем к DropZone и устанавливаем скорректированную позицию
            rectTransform.SetParent(dropZone.transform, false); // false — не сохранять мировую позицию
            rectTransform.anchoredPosition = targetPosition;

            // Сохраняем размер и масштаб
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSize.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize.y);
            rectTransform.localScale = originalLocalScale;

            isInDropZone = true;
            dropZonePosition = targetPosition;
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

    private void AttachToDropZone(DropZone dropZone, Vector2 targetPosition)
    {
        rectTransform.SetParent(dropZone.transform, false);
        rectTransform.anchoredPosition = targetPosition;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSize.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize.y);
        rectTransform.localScale = Vector3.one;
    }


    private void ReturnToDropZone()
    {
        DropZone dropZone = Object.FindFirstObjectByType<DropZone>();
        if (dropZone != null)
        {
            rectTransform.SetParent(dropZone.transform, false);
            rectTransform.anchoredPosition = dropZonePosition;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSize.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize.y);
            rectTransform.localScale = Vector3.one; // 🔥
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