using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PuzzleGenerator : MonoBehaviour
{
    [Header("������")]
    public DropZone dropZone;               // ���� ������ �����
    public Transform puzzlePiecesParent;    // �������� ��� ������� (��� DropZone)
    public GameObject puzzlePiecePrefab;    // ������ � Image + PuzzlePieceDragHandler

    [Header("���������")]
    public float spacing = 5f;              // ������ ����� �������� ��� ��������

    private string imageName;
    private string selectedLevel;
    private int gridSize;
    private Texture2D fullTexture;


    void Start()
    {
        Debug.Log("PuzzleGenerator: ������ ��������� �����");

        string inputMode = GameData.InputMode;
        string selectedLevel = GameData.SelectedLevel;

        Debug.Log($"InputMode: '{inputMode}', Level: '{selectedLevel}'");

        // ���������� ������ �����
        switch (selectedLevel)
        {
            case "level1": gridSize = 5; break;
            case "level2": gridSize = 11; break;
            case "level3": gridSize = 14; break;
            case "level4": gridSize = 16; break;
            default: gridSize = 5; break;
        }

        // ��������� �����������
        if (inputMode == "user image")
        {
            fullTexture = GameData.UserImage;
            Debug.Log(fullTexture != null
                ? $"�������� ���������������� �����������: {fullTexture.width}x{fullTexture.height}"
                : "UserImage is NULL!");
        }
        else
        {
            LoadTextureFromResources(inputMode);
        }

        if (fullTexture != null)
        {
            GeneratePuzzle();
        }
        else
        {
            Debug.LogError("�� ������� �������� ����������� ��� �����!");
        }
    }

    void LoadTextureFromResources(string imageName)
    {
        fullTexture = Resources.Load<Texture2D>(imageName);
        if (fullTexture == null)
        {
            Sprite sprite = Resources.Load<Sprite>(imageName);
            if (sprite != null) fullTexture = sprite.texture;
        }
    }

    void GeneratePuzzle()
    {
        if (fullTexture == null || dropZone == null || puzzlePiecePrefab == null)
        {
            Debug.LogError("������������ ������ ��� ��������� �����.");
            return;
        }

        float cellWidth = dropZone.rectTransform.rect.width / gridSize;
        float cellHeight = dropZone.rectTransform.rect.height / gridSize;

        float displaySize = 20f;

        // ������� Viewport
        ScrollRect scrollRect = puzzlePiecesParent.GetComponentInParent<ScrollRect>();
        RectTransform viewportRT = null;

        if (scrollRect != null && scrollRect.viewport != null)
        {
            viewportRT = scrollRect.viewport;
            Debug.Log($"Viewport ������: {viewportRT.rect.width} x {viewportRT.rect.height}");
        }
        else
        {
            Debug.LogError("�� ������� ����� ScrollRect ��� ��� viewport! ����� ����� ���� ���������� ��� ��������������.");
            // � ������� ������ � ���������� ������ Canvas ��� ������
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                RectTransform canvasRT = canvas.GetComponent<RectTransform>();
                float w = canvasRT.rect.width;
                float h = canvasRT.rect.height;
                viewportRT = new GameObject("FakeViewport").AddComponent<RectTransform>();
                viewportRT.sizeDelta = new Vector2(w, h);
            }
            else
            {
                viewportRT = dropZone.rectTransform;
            }
        }

        float viewportWidth = viewportRT.rect.width;
        float viewportHeight = viewportRT.rect.height;

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                Rect spriteRect = new Rect(
                    col * (fullTexture.width / gridSize),
                    (gridSize - 1 - row) * (fullTexture.height / gridSize),
                    fullTexture.width / gridSize,
                    fullTexture.height / gridSize
                );

                Sprite pieceSprite = Sprite.Create(
                    fullTexture,
                    spriteRect,
                    new Vector2(0.5f, 0.5f),
                    100
                );

                // ������������
                GameObject pieceGO = Instantiate(puzzlePiecePrefab, puzzlePiecesParent);
                Image image = pieceGO.GetComponent<Image>();
                image.sprite = pieceSprite;
                image.preserveAspect = false;

                RectTransform rt = pieceGO.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(displaySize, displaySize);

                var dragHandler = pieceGO.GetComponent<PuzzlePieceDragHandler>();
                if (dragHandler != null)
                {
                    dragHandler.targetSizeInDropZone = new Vector2(cellWidth, cellHeight);
                    dragHandler.targetRow = row;
                    dragHandler.targetCol = col; 
                }
                // ������� ������ Viewport'�
                rt.anchoredPosition = Vector2.zero;

                // ��� �������
                Debug.Log($"���� {row},{col} ������ � �������: {rt.anchoredPosition}");
            }
        }
    }

    Texture2D LoadUserImageFromPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey("UserImageData"))
        {
            Debug.LogWarning("��� ����������� ����������������� �����������.");
            return null;
        }

        string base64Data = PlayerPrefs.GetString("UserImageData");
        if (string.IsNullOrEmpty(base64Data))
        {
            Debug.LogWarning("������ ������ ����������������� �����������.");
            return null;
        }

        try
        {
            byte[] bytes = System.Convert.FromBase64String(base64Data);
            Texture2D texture = new Texture2D(2, 2); // dummy size
            if (texture.LoadImage(bytes))
            {
                Debug.Log($"���������������� ����������� ���������: {texture.width}x{texture.height}");
                return texture;
            }
            else
            {
                Debug.LogError("�� ������� ������������ ����������� �� base64.");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("������ ��� �������� ����������������� �����������: " + e.Message);
            return null;
        }
    }

}