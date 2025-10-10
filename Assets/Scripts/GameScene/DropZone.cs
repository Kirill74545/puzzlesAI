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
}