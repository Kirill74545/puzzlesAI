using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TriangulationPuzzleGenerator : MonoBehaviour
{
    [Header("Ссылки")]
    public DropZone dropZone;
    public Transform puzzlePiecesParent;
    public GameObject puzzlePiecePrefab;
    public PointGeneratorUnity pointGenerator;

    private string currentDifficulty;

    private int correctlyPlacedTriangles = 0;

    private List<TriangleData> triangles;
    private Texture2D fullTexture;

    private Vector2 minUV;
    private Vector2 maxUV;
    private Vector2 contentSizeUV;
    private Vector2 scaleToDropZone;

    [Header("Настройки")]
    public float pieceDisplaySize = 50f;
    public float spacing = 10f;

    [Tooltip("Масштаб всей триангуляционной картинки. Увеличивайте, если картинка мала, уменьшайте, если велика.")]
    public float pictureScaleFactor =0.8f;

    public int TriangleCount => triangles != null ? triangles.Count : 0;

    public void Initialize(Texture2D texture, string difficultyLevel)
    {
        fullTexture = texture;
        currentDifficulty = difficultyLevel;
        GeneratePuzzle();
    }

    void GeneratePuzzle()
    {
        if (fullTexture == null || dropZone == null || puzzlePiecePrefab == null)
        {
            Debug.LogError("Недостаточно данных для генерации триангуляционного пазла.");
            return;
        }

        Rect dropZoneRect = dropZone.rectTransform.rect;
        triangles = TriangulationPipeline.Generate(pointGenerator, currentDifficulty);

        ScrollRect scrollRect = puzzlePiecesParent.GetComponentInParent<ScrollRect>();
        RectTransform viewportRT = scrollRect?.viewport;

        float viewportWidth = viewportRT != null ? viewportRT.rect.width : Screen.width;
        float viewportHeight = viewportRT != null ? viewportRT.rect.height : Screen.height;

        int piecesPerRow = Mathf.Max(1, Mathf.FloorToInt(viewportWidth / (pieceDisplaySize + spacing)));

        for (int i = 0; i < triangles.Count; i++)
        {
            TriangleData triangle = triangles[i];
            GameObject pieceGO = CreateTrianglePiece(triangle, i);

            int rowIndex = i / piecesPerRow;
            int colIndex = i % piecesPerRow;

            float xPos = colIndex * (pieceDisplaySize + spacing) - viewportWidth / 2 + pieceDisplaySize / 2;
            float yPos = -rowIndex * (pieceDisplaySize + spacing) + viewportHeight / 2 - pieceDisplaySize / 2;

            RectTransform rt = pieceGO.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(xPos, yPos);

            rt.localScale = Vector3.zero;
            rt.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutBack)
                .SetDelay(i * 0.03f);
        }

        Debug.Log($"Сгенерирован триангуляционный пазл уровня {currentDifficulty} с {triangles.Count} треугольниками");
    }

    GameObject CreateTrianglePiece(TriangleData triangle, int index)
    {
        GameObject pieceGO = Instantiate(puzzlePiecePrefab, puzzlePiecesParent);

        // --- ВОЗВРАЩАЕМСЯ К ИЗОБРАЖЕНИЮ ---
        Image pieceImage = pieceGO.GetComponent<Image>();
        if (pieceImage == null)
        {
            Debug.LogError("На префабе puzzlePiecePrefab отсутствует компонент Image!");
            Destroy(pieceGO);
            return null;
        }

        // Создаем текстуру для треугольника
        Texture2D triangleTexture = CreateTriangleTexture(triangle);
        if (triangleTexture != null)
        {
            pieceImage.sprite = Sprite.Create(
                triangleTexture,
                new Rect(0, 0, triangleTexture.width, triangleTexture.height),
                new Vector2(0.5f, 0.5f)
            );
        }
        // ------------------------------------

        RectTransform rt = pieceGO.GetComponent<RectTransform>();

        // --- ГЛАВНОЕ ИСПРАВЛЕНИЕ: РАЗМЕР И ПОЗИЦИЯ ПО ГРАНИЦАМ ---
        Rect dropZoneRect = dropZone.rectTransform.rect;

        // Размер пазла - это размер его ограничивающего прямоугольника
        Vector2 targetSize = new Vector2(
            triangle.boundingBoxSizeUV.x * dropZoneRect.width,
            triangle.boundingBoxSizeUV.y * dropZoneRect.height
        );
        rt.sizeDelta = targetSize;

        // Позиция пазла - это центр его ограничивающего прямоугольника
        Vector2 targetPosition = new Vector2(
            (triangle.boundingBoxCenterUV.x - 0.5f) * dropZoneRect.width,
            (triangle.boundingBoxCenterUV.y - 0.5f) * dropZoneRect.height
        );
        // ---------------------------------------------------------

        var dragHandler = pieceGO.GetComponent<PuzzlePieceDragHandler>();
        if (dragHandler != null)
        {
            dragHandler.targetSizeInDropZone = targetSize;
            dragHandler.targetPosition = targetPosition;

            dragHandler.isTriangulationPiece = true;
            dragHandler.triangleIndex = index;
            dragHandler.scrollRectContent = puzzlePiecesParent;
            dragHandler.placementTolerance = GetPlacementTolerance(currentDifficulty);
        }

        return pieceGO;
    }

    // --- ВОЗВРАЩАЕМ МЕТОД СОЗДАНИЯ ТЕКСТУРЫ, НО ОН УМНЕЕ ---
    Texture2D CreateTriangleTexture(TriangleData triangle)
    {
        int textureSize = 256;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);

        Color[] transparentColors = new Color[textureSize * textureSize];
        for (int i = 0; i < transparentColors.Length; i++)
            transparentColors[i] = Color.clear;
        texture.SetPixels(transparentColors);

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                // UV-координата пикселя внутри текстуры пазла (от 0 до 1)
                Vector2 pieceUV = new Vector2((float)x / textureSize, (float)y / textureSize);

                // Конвертируем эту UV в UV-координату полной картинки
                Vector2 fullImageUV = new Vector2(
                    triangle.boundingBoxMinUV.x + pieceUV.x * triangle.boundingBoxSizeUV.x,
                    triangle.boundingBoxMinUV.y + pieceUV.y * triangle.boundingBoxSizeUV.y
                );

                if (IsPointInTriangle(fullImageUV, triangle.uvA, triangle.uvB, triangle.uvC))
                {
                    int texX = Mathf.RoundToInt(fullImageUV.x * fullTexture.width);
                    int texY = Mathf.RoundToInt(fullImageUV.y * fullTexture.height);
                    texX = Mathf.Clamp(texX, 0, fullTexture.width - 1);
                    texY = Mathf.Clamp(texY, 0, fullTexture.height - 1);

                    Color pixelColor = fullTexture.GetPixel(texX, texY);
                    texture.SetPixel(x, y, pixelColor);
                }
            }
        }

        texture.Apply();
        return texture;
    }

    bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 v0 = c - a;
        Vector2 v1 = b - a;
        Vector2 v2 = p - a;
        float dot00 = Vector2.Dot(v0, v0);
        float dot01 = Vector2.Dot(v0, v1);
        float dot02 = Vector2.Dot(v0, v2);
        float dot11 = Vector2.Dot(v1, v1);
        float dot12 = Vector2.Dot(v1, v2);
        float invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
        return (u >= 0) && (v >= 0) && (u + v <= 1);
    }

    private float GetTargetSizeInDropZone(string difficulty)
    {
        switch (difficulty)
        {
            case "level1": return 80f;
            case "level2": return 70f;
            case "level3": return 60f;
            case "level4": return 50f;
            default: return 80f;
        }
    }

    private float GetPlacementTolerance(string difficulty)
    {
        switch (difficulty)
        {
            case "level1": return 10f;
            case "level2": return 10f;
            case "level3": return 10f;
            case "level4": return 15f;
            default: return 10f;
        }
    }
}
