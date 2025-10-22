using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

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

    [Header("������")]
    public TextMeshProUGUI timerText; 

    private float elapsedTime = 0f;
    private bool isTimerRunning = false;
    private int totalPieces = 0;
    private int correctlyPlacedCount = 0;

    public LevelCompleteUI levelCompleteUI;


    void Start()
    {
        Debug.Log("PuzzleGenerator: ������ ��������� �����");

        string inputMode = GameData.InputMode;
        string selectedLevel = GameData.SelectedLevel;

        Debug.Log($"InputMode: '{inputMode}', Level: '{selectedLevel}'");

        // ���������� ������ �����
        switch (selectedLevel)
        {
            case "level1": gridSize = 4; break;   // 4x4 = 16
            case "level2": gridSize = 5; break;   // 5x5 = 25
            case "level3": gridSize = 7; break;   // 7x7 = 49
            case "level4": gridSize = 9; break;   // 9x9 = 81
            default: gridSize = 4; break;
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

    void Update()
    {
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void RegisterCorrectPlacement()
    {
        correctlyPlacedCount++;
        if (correctlyPlacedCount >= totalPieces)
        {
            isTimerRunning = false;
            OnPuzzleCompleted();
        }
    }

    void OnPuzzleCompleted()
    {
        isTimerRunning = false;

        PlayerPrefs.SetFloat("LastPuzzleTime", elapsedTime);
        PlayerPrefs.SetString("LastCompletedLevel", GameData.SelectedLevel);
        PlayerPrefs.Save();
        Debug.Log($"���� �������� �� {elapsedTime:F2} ������!");

        if (levelCompleteUI != null)
        {
            levelCompleteUI.Show(elapsedTime);
        }
        else
        {
            Debug.LogWarning("LevelCompleteUI �� ��������! ��������� ��� � ����������.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
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
        if (dropZone != null)
        {
            dropZone.gridSize = gridSize; 
        }

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

        // ��� 1: �������� ��� ������
        List<Vector2Int> cells = new List<Vector2Int>();
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                cells.Add(new Vector2Int(col, row));
            }
        }

        // ��� 2: ������������ ������
        System.Random rng = new System.Random();
        int n = cells.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Vector2Int temp = cells[k];
            cells[k] = cells[n];
            cells[n] = temp;
        }

        // ��� 3: ������������, ������� ������ ���������� � ������
        int piecesPerRow = Mathf.Max(1, Mathf.FloorToInt(viewportWidth / (displaySize + spacing)));

        // ��� 4: ������ ����� � ��������� ������� � ��������� �� � ����� ������ ScrollRect
        for (int i = 0; i < cells.Count; i++)
        {
            Vector2Int cell = cells[i];
            int col = cell.x;
            int row = cell.y;

            // ������ ������ ���������
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

            // ������������ ����
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

            // ������������ ������� � ScrollRect (����� ����� �������, ������ ����)
            int rowIndex = i / piecesPerRow;
            int colIndex = i % piecesPerRow;

            float xPos = colIndex * (displaySize + spacing) - viewportWidth / 2 + displaySize / 2;
            float yPos = -rowIndex * (displaySize + spacing) + viewportHeight / 2 - displaySize / 2;

            rt.anchoredPosition = new Vector2(xPos, yPos);

            Debug.Log($"���� [{row},{col}] ������ � �������: {rt.anchoredPosition}");
        }

        totalPieces = gridSize * gridSize;
        correctlyPlacedCount = 0;
        elapsedTime = 0f;
        isTimerRunning = true;
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