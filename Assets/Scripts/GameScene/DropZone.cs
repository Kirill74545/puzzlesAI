using UnityEngine;
using UnityEngine.UI;

public class DropZone : MonoBehaviour
{
    public RectTransform rectTransform;
    private Camera uiCamera;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            uiCamera = canvas.worldCamera;
        else
            Debug.LogError("DropZone не находится внутри Canvas!");
    }

    public bool IsFullyInside(RectTransform dragged)
    {
        if (uiCamera == null)
        {
            Debug.LogError("UI Camera не назначена в DropZone!");
            return false;
        }

        // Получаем мировые координаты углов перетаскиваемого объекта
        Vector3[] draggedCorners = new Vector3[4];
        dragged.GetWorldCorners(draggedCorners);

        // Проверяем, что все углы внутри дропзоны
        for (int i = 0; i < 4; i++)
        {
            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    uiCamera.WorldToScreenPoint(draggedCorners[i]),
                    uiCamera,
                    out localPoint))
                return false;

            if (!rectTransform.rect.Contains(localPoint))
                return false;
        }
        return true;
    }

    // Альтернативный метод - более надежный для Screen Space - Camera
    public bool IsOverlapping(RectTransform dragged, float requiredOverlap = 0.7f)
    {
        if (uiCamera == null) return false;

        // Получаем bounds в локальных координатах дропзоны
        Vector3[] draggedCorners = new Vector3[4];
        dragged.GetWorldCorners(draggedCorners);

        Vector3[] dropZoneCorners = new Vector3[4];
        rectTransform.GetWorldCorners(dropZoneCorners);

        // Считаем сколько углов пазла внутри дропзоны
        int cornersInside = 0;
        foreach (Vector3 worldCorner in draggedCorners)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    uiCamera.WorldToScreenPoint(worldCorner),
                    uiCamera,
                    out localPoint))
            {
                if (rectTransform.rect.Contains(localPoint))
                {
                    cornersInside++;
                }
            }
        }

        // Если достаточно углов внутри дропзоны
        return cornersInside >= 4 * requiredOverlap;
    }

    // Возвращает позицию (в локальных координатах DropZone), куда можно поместить dragged,
    // чтобы он полностью влез и был как можно ближе к его текущей позиции.
    public Vector2 GetClampedPositionInside(RectTransform dragged)
    {
        // Размеры детали
        Vector2 pieceSize = dragged.rect.size;

        // Текущая позиция детали в локальных координатах DropZone
        Vector3[] corners = new Vector3[4];
        dragged.GetWorldCorners(corners);
        Vector2 worldCenter = (corners[0] + corners[2]) / 2f; // центр детали в мировых координатах

        Vector2 localCenter;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                uiCamera.WorldToScreenPoint(worldCenter),
                uiCamera,
                out localCenter))
        {
            // Если не можем преобразовать — используем центр DropZone как fallback
            localCenter = Vector2.zero;
        }

        // Определяем допустимые границы для центра детали
        float halfWidth = pieceSize.x / 2f;
        float halfHeight = pieceSize.y / 2f;

        Rect safeArea = new Rect(
            rectTransform.rect.xMin + halfWidth,
            rectTransform.rect.yMin + halfHeight,
            rectTransform.rect.width - pieceSize.x,
            rectTransform.rect.height - pieceSize.y
        );

        // Кладём центр детали в безопасную зону
        Vector2 clampedCenter = new Vector2(
            Mathf.Clamp(localCenter.x, safeArea.xMin, safeArea.xMax),
            Mathf.Clamp(localCenter.y, safeArea.yMin, safeArea.yMax)
        );

        return clampedCenter;
    }
}