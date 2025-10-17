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
        imageName = PlayerPrefs.GetString("SavedInput", "default_puzzle");
        selectedLevel = PlayerPrefs.GetString("SelectedLevel", "level1");

        // ���������� ������ �����
        switch (selectedLevel)
        {
            case "level1": gridSize = 5; break;   // 25
            case "level2": gridSize = 11; break;  // 121
            case "level3": gridSize = 14; break;  // 196
            case "level4": gridSize = 16; break;  // 256
            default: gridSize = 5; break;
        }

        LoadTextureFromResources();
        if (fullTexture != null)
        {
            GeneratePuzzle();
        }
        else
        {
            Debug.LogError($"����������� '{imageName}' �� ������� � Resources!");
        }
    }

    void LoadTextureFromResources()
    {
        // �������� ��������� ��� Texture2D
        fullTexture = Resources.Load<Texture2D>(imageName);

        // ���� �� ���������� � ������� ��� Sprite (������ ��� ������)
        if (fullTexture == null)
        {
            Sprite sprite = Resources.Load<Sprite>(imageName);
            if (sprite != null)
            {
                fullTexture = sprite.texture;
            }
        }

        if (fullTexture == null)
        {
            Debug.LogError($"�� ������� ��������� ����������� '{imageName}' �� ����� Resources.");
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
                }

                // ������� ������ Viewport'�
                rt.anchoredPosition = Vector2.zero;

                // ��� �������
                Debug.Log($"���� {row},{col} ������ � �������: {rt.anchoredPosition}");
            }
        }
    }

}