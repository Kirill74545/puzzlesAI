using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using UnityEngine.Networking;
using UnityEngine.InputSystem;
using System.Threading;

public class InputPanelController : MonoBehaviour
{
    [Header("Основные UI элементы")]
    public Button startButton;               // Стартовая кнопка
    public GameObject inputPanel;            // Основная панель ввода
    public TMP_InputField inputField;        // Поле ввода TextMeshPro
    public Button randomButton;              // Кнопка случайной генерации — остаётся до выбора метода
    public Button userImageButton;           // Кнопка загрузки из галереи — исчезает при вводе/рандоме
    public Button searchWebImageButton;      // Кнопка поиска в Openverse
    public Button aiGenerateButton;          // Кнопка генерации через ИИ
    public Button backToInputButton;         // Кнопка возврата
    public Button backDuringGenerationButton;

    [Header("Дополнительное изображение")]
    public GameObject additionalImage;
    public GameObject additionalImage_;
    public GameObject promptImage;

    [Header("Загрузочный индикатор")]
    public GameObject loadingIndicator;

    [Header("Кнопка после загрузки")]
    public Button confirmButton2;

    [Header("Кнопки выбора")]
    public Button classikButton;
    public Button randomChoiceButton;

    [Header("Кнопки уровней сложности")]
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    public Button level4Button;

    [Header("Предупреждение и кнопка аэрохоккея при генерации ИИ")]
    public GameObject aiWarningText;
    public Button aeroHockeyButton;

    [Header("Параметры анимации")]
    public float appearDuration = 0.5f;

    private string savedInput;
    private string selectedChoice;
    private string selectedLevel;
    private Texture2D userTexture;

    private CanvasGroup panelCanvasGroup;
    private RectTransform panelRectTransform;
    private CanvasGroup imageCanvasGroup;
    private RectTransform imageRectTransform;
    private CanvasGroup imageCanvasGroup_;
    private RectTransform imageRectTransform_;

    private Gen_image_AI aiGenerator;
    private bool isRotating = false;

    private bool _isRandomButtonVisible = false;
    private bool _isUserImageButtonVisible = false;
    private bool _isSearchWebImageButtonVisible = false;
    private bool _isAIGenerateButtonVisible = false;

    public AeroHockeyMiniGame miniGame;

    // Состояния приложения
    private enum AppState
    {
        Start,
        InputSelection,
        GallerySelection,
        AIGeneration,
        ImageConfirmed,
        ChoiceSelection,
        LevelSelection
    }

    private AppState currentState = AppState.Start;

    private CancellationTokenSource aiGenerationCts;
    private CancellationTokenSource webImageCts;
    private CancellationTokenSource userImageCts;

    void Start()
    {
        panelRectTransform = inputPanel.GetComponent<RectTransform>();
        panelCanvasGroup = inputPanel.GetComponent<CanvasGroup>() ?? inputPanel.AddComponent<CanvasGroup>();
        if (additionalImage != null)
        {
            imageRectTransform = additionalImage.GetComponent<RectTransform>();
            imageCanvasGroup = additionalImage.GetComponent<CanvasGroup>() ?? additionalImage.AddComponent<CanvasGroup>();
        }
        if (additionalImage_ != null)
        {
            imageRectTransform_ = additionalImage_.GetComponent<RectTransform>();
            imageCanvasGroup_ = additionalImage_.GetComponent<CanvasGroup>() ?? additionalImage_.AddComponent<CanvasGroup>();
        }
        aiGenerator = Object.FindFirstObjectByType<Gen_image_AI>();
        if (aiGenerator == null)
        {
            Debug.LogWarning("Gen_image_AI не найден на сцене. Генерация изображений недоступна.");
        }


        inputPanel.SetActive(false);
        if (additionalImage != null) additionalImage.SetActive(false);
        if (additionalImage_ != null) additionalImage_.SetActive(false);
        if (promptImage != null) promptImage.SetActive(false);
        if (loadingIndicator != null) loadingIndicator.SetActive(false);
        if (confirmButton2 != null) confirmButton2.gameObject.SetActive(false);
        if (classikButton != null) classikButton.gameObject.SetActive(false);
        if (randomChoiceButton != null) randomChoiceButton.gameObject.SetActive(false);
        if (level1Button != null) level1Button.gameObject.SetActive(false);
        if (level2Button != null) level2Button.gameObject.SetActive(false);
        if (level3Button != null) level3Button.gameObject.SetActive(false);
        if (level4Button != null) level4Button.gameObject.SetActive(false);

        // Кнопки ввода изначально скрыты — появятся после startButton
        if (randomButton != null) randomButton.gameObject.SetActive(false);
        if (userImageButton != null) userImageButton.gameObject.SetActive(false);
        if (searchWebImageButton != null) searchWebImageButton.gameObject.SetActive(false);
        if (aiGenerateButton != null) aiGenerateButton.gameObject.SetActive(false);

        // Скрыть предупреждение и кнопку аэрохоккея изначально
        if (aiWarningText != null) aiWarningText.SetActive(false);
        if (aeroHockeyButton != null) aeroHockeyButton.gameObject.SetActive(false);
        if (backToInputButton != null) backToInputButton.gameObject.SetActive(false);
        if (backDuringGenerationButton != null) backDuringGenerationButton.gameObject.SetActive(false);

        // Подписка на события
        if (startButton != null)
            startButton.onClick.AddListener(() => { AnimateButtonPress(startButton); OnStartButtonClicked(); });
        if (randomButton != null)
            randomButton.onClick.AddListener(() => { AnimateButtonPress(randomButton); OnRandomButtonClicked(); });
        if (aiGenerateButton != null)
            aiGenerateButton.onClick.AddListener(() => { AnimateButtonPress(aiGenerateButton); OnAIGenerateClicked(); });
        if (searchWebImageButton != null)
            searchWebImageButton.onClick.AddListener(() => { AnimateButtonPress(searchWebImageButton); OnSearchWebImageButtonClicked(); });
        if (userImageButton != null)
            userImageButton.onClick.AddListener(() => { AnimateButtonPress(userImageButton); OnUserImageButtonClicked(); });
        if (confirmButton2 != null)
            confirmButton2.onClick.AddListener(() => { AnimateButtonPress(confirmButton2); OnConfirmButton2Clicked(); });
        if (classikButton != null)
            classikButton.onClick.AddListener(() => { AnimateButtonPress(classikButton); OnChoiceSelected("classik"); });
        if (randomChoiceButton != null)
            randomChoiceButton.onClick.AddListener(() => { AnimateButtonPress(randomChoiceButton); OnChoiceSelected("random"); });
        if (backToInputButton != null)
            backToInputButton.onClick.AddListener(OnBackToInputButtonClicked);
        if (backDuringGenerationButton != null)
            backDuringGenerationButton.onClick.AddListener(OnBackDuringGenerationButtonClicked);
        if (level1Button != null)
            level1Button.onClick.AddListener(() => { AnimateButtonPress(level1Button); OnLevelSelected("level1"); });
        if (level2Button != null)
            level2Button.onClick.AddListener(() => { AnimateButtonPress(level2Button); OnLevelSelected("level2"); });
        if (level3Button != null)
            level3Button.onClick.AddListener(() => { AnimateButtonPress(level3Button); OnLevelSelected("level3"); });
        if (level4Button != null)
            level4Button.onClick.AddListener(() => { AnimateButtonPress(level4Button); OnLevelSelected("level4"); });
        if (aeroHockeyButton != null)
            aeroHockeyButton.onClick.AddListener(() => { AnimateButtonPress(aeroHockeyButton); OnAeroHockeyButtonClicked(); });
        if (inputField != null)
            inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
        currentState = AppState.Start;
    }

    void Update()
    {
        bool backPressed = false;
    #if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            backPressed = true;
        }
    #else
        // Старая система ввода
        if (Input.GetKeyDown(KeyCode.Escape))
        {
        backPressed = true;
        }
    #endif
        if (backPressed)
        {
            HandleBackButton();
        }
    }

    private void HandleBackButton()
    {
        switch (currentState)
        {
            case AppState.GallerySelection:
                Debug.Log("Системная кнопка 'Назад' нажата во время выбора галереи");
                ReturnToInputSelection();
                break;
            case AppState.AIGeneration:
                Debug.Log("Системная кнопка 'Назад' нажата во время генерации ИИ");
                OnBackDuringGenerationButtonClicked();
                break;
            case AppState.ImageConfirmed:
                Debug.Log("Системная кнопка 'Назад' нажата при подтверждении изображения");
                OnBackToInputButtonClicked();
                break;
            case AppState.ChoiceSelection:
                Debug.Log("Системная кнопка 'Назад' нажата при выборе метода");
                ReturnFromChoiceToImage();
                break;
            case AppState.LevelSelection:
                Debug.Log("Системная кнопка 'Назад' нажата при выборе уровня");
                ReturnFromLevelToChoice();
                break;
            case AppState.InputSelection:
                Debug.Log("Системная кнопка 'Назад' нажата - возврат к стартовому экрану");
                ReturnToStart();
                break;
        }
    }

    void OnInputFieldValueChanged(string newText)
    {
        bool hasText = !string.IsNullOrWhiteSpace(newText);

        // Галерея видна ТОЛЬКО когда поле пустое
        if (userImageButton != null)
        {
            if (!hasText && !_isUserImageButtonVisible)
            {
                ShowUIElement(userImageButton.gameObject, 0.2f);
                _isUserImageButtonVisible = true;
            }
            else if (hasText && _isUserImageButtonVisible)
            {
                HideUIElement(userImageButton.gameObject, 0.2f);
                _isUserImageButtonVisible = false;
            }
        }

        // Кнопки ИИ и Web — видны при любом непустом тексте
        if (aiGenerateButton != null)
        {
            if (hasText && !_isAIGenerateButtonVisible)
            {
                ShowUIElement(aiGenerateButton.gameObject, 0.2f);
                _isAIGenerateButtonVisible = true;
            }
            else if (!hasText && _isAIGenerateButtonVisible)
            {
                HideUIElement(aiGenerateButton.gameObject, 0.2f);
                _isAIGenerateButtonVisible = false;
            }
        }

        if (searchWebImageButton != null)
        {
            if (hasText && !_isSearchWebImageButtonVisible)
            {
                ShowUIElement(searchWebImageButton.gameObject, 0.2f);
                _isSearchWebImageButtonVisible = true;
            }
            else if (!hasText && _isSearchWebImageButtonVisible)
            {
                HideUIElement(searchWebImageButton.gameObject, 0.2f);
                _isSearchWebImageButtonVisible = false;
            }
        }
    }

    void OnStartButtonClicked()
    {
        startButton.gameObject.SetActive(false);
        currentState = AppState.InputSelection;

        // Используем тот же подход, что и в ReturnToInputSelection
        ResetAllUIStates();

        // Показываем основные элементы
        if (inputPanel != null)
        {
            inputPanel.SetActive(true);
            ShowUIElement(inputPanel, appearDuration);
        }
        if (additionalImage != null)
        {
            additionalImage.SetActive(true);
            ShowUIElement(additionalImage, appearDuration);
        }
        if (additionalImage_ != null)
        {
            additionalImage_.SetActive(true);
            ShowUIElement(additionalImage_, appearDuration);
        }

        // Показываем кнопки ввода
        if (inputField != null)
        {
            inputField.gameObject.SetActive(true);
            ShowUIElement(inputField.gameObject, 0.2f);
        }
        if (randomButton != null)
        {
            randomButton.gameObject.SetActive(true);
            ShowUIElement(randomButton.gameObject, 0.2f);
            _isRandomButtonVisible = true;
        }
        if (userImageButton != null)
        {
            userImageButton.gameObject.SetActive(true);
            ShowUIElement(userImageButton.gameObject, 0.2f);
            _isUserImageButtonVisible = true;
        }

        inputField.text = "";
    }

    void OnRandomButtonClicked()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("RandomValues");
        if (jsonFile == null)
        {
            Debug.LogError("Файл RandomValues.json не найден в папке Resources!");
            return;
        }

        string[] values = null;
        try
        {
            values = JsonHelper.FromJson<string>(jsonFile.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка при парсинге JSON: " + e.Message);
            return;
        }

        if (values == null || values.Length == 0)
        {
            Debug.LogWarning("JSON не содержит значений.");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, values.Length);
        inputField.text = values[randomIndex];
    }

    void OnAIGenerateClicked()
    {
        if (string.IsNullOrWhiteSpace(inputField.text))
        {
            Debug.LogWarning("Нет текста для генерации.");
            return;
        }

        // Отмена предыдущ генерации, если была
        if (aiGenerationCts != null)
        {
            aiGenerationCts.Cancel();
            aiGenerationCts.Dispose();
        }

        // Создаём новый токен для текущей генерации
        aiGenerationCts = new CancellationTokenSource();

        savedInput = inputField.text.Trim();
        HideInputElements();
        currentState = AppState.AIGeneration;

        // Показать предупреждение и кнопку аэрохоккея при ИИ-генерации
        if (aiWarningText != null) ShowUIElement(aiWarningText);
        if (aeroHockeyButton != null) ShowUIElement(aeroHockeyButton.gameObject);
        if (backDuringGenerationButton != null) ShowUIElement(backDuringGenerationButton.gameObject);
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
            StartCoroutine(RotateLoadingIndicator());
        }

        StartCoroutine(ProcessSubmission(aiGenerationCts.Token));
    }

    void OnAeroHockeyButtonClicked()
    {
        // Скрыть предупреждение и кнопку аэрохоккея
        if (aiWarningText != null) HideUIElement(aiWarningText);
        if (aeroHockeyButton != null) HideUIElement(aeroHockeyButton.gameObject);
        if (backDuringGenerationButton != null) HideUIElement(backDuringGenerationButton.gameObject);

        // Запуск мини-игры аэрохоккея
        if (miniGame != null)
        {
            miniGame.StartMiniGame();
        }
        else
        {
            Debug.LogWarning("AeroHockeyMiniGame не назначен.");
        }
    }

    void OnUserImageButtonClicked()
    {
        // Отмена генерации
        if (userImageCts != null)
        {
            userImageCts.Cancel();
            userImageCts.Dispose();
        }
        // Создаём новый токен для текущей загрузки
        userImageCts = new CancellationTokenSource();

        HideInputElements();
        currentState = AppState.GallerySelection;

        #if UNITY_ANDROID && !UNITY_EDITOR
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize: 1024);
                if (texture != null)
                {
                    userTexture = texture;
                    savedInput = "user image";
                    Debug.Log("Выбрано пользовательское изображение");
                    if (loadingIndicator != null)
                    {
                        loadingIndicator.SetActive(true);
                        StartCoroutine(RotateLoadingIndicator());
                    }
                    // --- 🔥 ПЕРЕДАЁМ ТОКЕН В КОРУТИНУ ---
                    StartCoroutine(ProcessUserImageSubmission(userImageCts.Token));
                    // --- /ПЕРЕДАЁМ ТОКЕН ---
                }
                else
                {
                    // --- 🔥 ОЧИСТКА ТОКЕНА ПРИ ОШИБКЕ ---
                    userImageCts?.Cancel();
                    userImageCts?.Dispose();
                    userImageCts = null;
                    // --- /ОЧИСТКА ---
                    Debug.LogError("Не удалось загрузить изображение");
                    ReturnToInputSelection();
                }
            }
            else
            {
                // --- 🔥 ОЧИСТКА ТОКЕНА ПРИ ОТМЕНЕ ---
                userImageCts?.Cancel();
                userImageCts?.Dispose();
                userImageCts = null;
                // --- /ОЧИСТКА ---
                Debug.Log("Выбор отменён");
                ReturnToInputSelection();
            }
        }, "Выберите изображение", "image/*");
        #else
        Debug.Log("Галерея недоступна в редакторе");
        userTexture = new Texture2D(256, 256);
        savedInput = "user image";


        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
            StartCoroutine(RotateLoadingIndicator());
        }

        StartCoroutine(ProcessUserImageSubmission(userImageCts.Token));
        #endif
    }

    void OnBackToInputButtonClicked()
    {
        Debug.Log("Кнопка 'Вернуться' нажата. Сбрасываем изображение и возвращаемся к выбору ввода.");

        aiGenerationCts?.Cancel();
        webImageCts?.Cancel();
        userImageCts?.Cancel();

        // Сбрасываем GameData, связанные с изображением
        GameData.InputMode = null;
        GameData.UserImage = null;

        // Скрываем изображение и кнопку подтверждения
        if (promptImage != null) HideUIElement(promptImage);
        if (confirmButton2 != null) HideUIElement(confirmButton2.gameObject);

        // Скрываем кнопку 'Вернуться'
        if (backToInputButton != null) HideUIElement(backToInputButton.gameObject);

        if (inputField != null) inputField.text = "";

        // Возвращаем элементы ввода
        AudioManager.Instance?.PlayButtonClick();
        ReturnToInputSelection();
    }

    void OnBackDuringGenerationButtonClicked()
    {
        Debug.Log("Кнопка 'Вернуться во время генерации' нажата. Отменяем генерацию и возвращаемся к выбору ввода.");

        aiGenerationCts?.Cancel();
        webImageCts?.Cancel();
        userImageCts?.Cancel();

        // Остановить аэрохоккей, если он был запущен
        if (miniGame != null && miniGame.isActive)
        {
            miniGame.StopMiniGame();
        }

        // Остановить вращение индикатора
        isRotating = false;
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
            DOTween.Kill("loadingRotation");
        }

        // Сбрасываем GameData, связанные с изображением
        GameData.InputMode = null;
        GameData.UserImage = null;

        // Скрываем все элементы, связанные с генерацией
        if (aiWarningText != null) HideUIElement(aiWarningText);
        if (aeroHockeyButton != null) HideUIElement(aeroHockeyButton.gameObject);
        if (backDuringGenerationButton != null) HideUIElement(backDuringGenerationButton.gameObject);
        if (promptImage != null) HideUIElement(promptImage);
        if (confirmButton2 != null) HideUIElement(confirmButton2.gameObject);
        if (backToInputButton != null) HideUIElement(backToInputButton.gameObject);

        // Возвращаем элементы ввода
        AudioManager.Instance?.PlayButtonClick();
        ReturnToInputSelection();
    }

    void OnSearchWebImageButtonClicked()
    {
        string query = inputField.text.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            Debug.LogWarning("Запрос пустой.");
            return;
        }

        if (webImageCts != null)
        {
            webImageCts.Cancel();
            webImageCts.Dispose();
        }
        // Создаём новый токен для текущей загрузки
        webImageCts = new CancellationTokenSource();

        HideInputElements();
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
            isRotating = true;
            StartCoroutine(RotateLoadingIndicator());
        }
        // Сначала ищем на Wikimedia, если не найдено - ищем на Openverse
        StartCoroutine(FetchImageFromWikimedia(query, webImageCts.Token));
    }

    void HideInputElements()
    {
        if (inputField != null) HideUIElement(inputField.gameObject);
        if (randomButton != null) HideUIElement(randomButton.gameObject);
        if (userImageButton != null) HideUIElement(userImageButton.gameObject);
        if (aiGenerateButton != null) HideUIElement(aiGenerateButton.gameObject);
        if (searchWebImageButton != null) HideUIElement(searchWebImageButton.gameObject);
        if (additionalImage_ != null) HideUIElement(additionalImage_.gameObject);
        if (backToInputButton != null) HideUIElement(backToInputButton.gameObject);
    }

    IEnumerator ProcessSubmission(CancellationToken token)
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
            StartCoroutine(RotateLoadingIndicator());
        }

        Texture2D loadedTexture = null;

        // ВСЕГДА используем ИИ (без Resources)
        if (aiGenerator != null)
        {
            Debug.Log($"Генерация ИИ для: {savedInput}");

            if (token.IsCancellationRequested)
            {
                Debug.Log("ProcessSubmission: Отменено перед вызовом GenerateImage.");
                yield break; 
            }
            yield return StartCoroutine(aiGenerator.GenerateImage(savedInput, (tex) => loadedTexture = tex));
        }
        else
        {
            yield return new WaitForSeconds(2f);
            Debug.LogWarning("Gen_image_AI не найден. Fallback на 'banana'.");
            Sprite fallback = Resources.Load<Sprite>("banana");
            if (fallback != null) loadedTexture = fallback.texture;
        }

        if (token.IsCancellationRequested)
        {
            Debug.Log("ProcessSubmission: Отменено пользователем после завершения генерации.");
            yield break; 
        }

        // Останавливаем аэрохоккей, если он был запущен 
        if (miniGame != null && miniGame.isActive)
        {
            miniGame.StopMiniGame();
        }

        // Проверяем, была ли нажата кнопка аэрохоккея
        bool wasAeroHockeyStarted = (miniGame != null && miniGame.isActive);

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
            isRotating = false;
        }

        if (!token.IsCancellationRequested)
        {
            if (loadedTexture != null)
            {
                GameData.InputMode = "user image";
                GameData.UserImage = loadedTexture;
                SetAIPromptImage(loadedTexture);

                currentState = AppState.ImageConfirmed; 
                if (promptImage != null) ShowUIElement(promptImage);
                if (confirmButton2 != null) ShowUIElement(confirmButton2.gameObject);
               if (backToInputButton != null) ShowUIElement(backToInputButton.gameObject);
            }
            else
            {
                Debug.LogError("Не удалось получить изображение.");
                SetPromptImage("banana");
                GameData.InputMode = "banana";
                GameData.UserImage = null;
                if (promptImage != null) ShowUIElement(promptImage);
                if (backToInputButton != null) ShowUIElement(backToInputButton.gameObject);
            }
        }
        else
        {
            Debug.Log("ProcessSubmission: Установка изображения отменена.");
        }

        if (!wasAeroHockeyStarted)
        {
            if (aiWarningText != null) HideUIElement(aiWarningText);
            if (aeroHockeyButton != null) HideUIElement(aeroHockeyButton.gameObject);
            if (backDuringGenerationButton != null) HideUIElement(backDuringGenerationButton.gameObject);
        }
    }

    IEnumerator ProcessUserImageSubmission(CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            Debug.Log("ProcessUserImageSubmission: Отменено пользователем.");
            userImageCts?.Dispose();
            userImageCts = null;
            yield break;
        }

        yield return new WaitForSeconds(2f);

        if (token.IsCancellationRequested)
        {
            Debug.Log("ProcessUserImageSubmission: Отменено пользователем после ожидания.");
            userImageCts?.Dispose();
            userImageCts = null;
            yield break;
        }

        if (miniGame != null && miniGame.isActive)
        {
            miniGame.StopMiniGame();
        }

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
            isRotating = false;
        }

        if (!token.IsCancellationRequested)
        {
            GameData.InputMode = "user image";

            Texture2D safeTexture = MakeReadableAndResize(userTexture, 1024);

            GameData.UserImage = safeTexture;

            byte[] bytes = safeTexture.EncodeToPNG();
            PlayerPrefs.SetString("UserImageData", System.Convert.ToBase64String(bytes));
            PlayerPrefs.Save();

            if (userTexture != null && userTexture != safeTexture)
            {
                Destroy(userTexture);
            }
            userTexture = safeTexture;

            SetUserPromptImage();
            currentState = AppState.ImageConfirmed;

            if (promptImage != null) ShowUIElement(promptImage);
            if (confirmButton2 != null) ShowUIElement(confirmButton2.gameObject);
            if (backToInputButton != null) ShowUIElement(backToInputButton.gameObject);
        }
        else
        {
            Debug.Log("ProcessUserImageSubmission: Установка изображения отменена.");
            userImageCts?.Dispose();
            userImageCts = null;
        }
    }

    Texture2D MakeReadableAndResize(Texture2D source, int maxSize)
    {
        // Шаг 1: Делаем текстуру читаемой
        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height);
        Graphics.Blit(source, rt);

        RenderTexture prevRT = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D readableTex = new Texture2D(source.width, source.height);
        readableTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readableTex.Apply();

        RenderTexture.active = prevRT;
        RenderTexture.ReleaseTemporary(rt);

        // Шаг 2: Уменьшаем размер для скорости
        int width = readableTex.width;
        int height = readableTex.height;

        float ratio = (float)width / height;
        if (width > height && width > maxSize)
        {
            width = maxSize;
            height = (int)(maxSize / ratio);
        }
        else if (height > maxSize)
        {
            height = maxSize;
            width = (int)(maxSize * ratio);
        }

        Texture2D resized = new Texture2D(width, height);
        resized.SetPixels(readableTex.GetPixels(0, 0, width, height)); 
        resized.Apply();

        Destroy(readableTex); 
        return resized;
    }

    IEnumerator FetchImageFromWikimedia(string query, CancellationToken token)
    {
        string userAgent = "Unity3D/2025.1 (non-commercial use)";
        // Кодируем только сам запрос
        string encodedQuery = UnityWebRequest.EscapeURL(query);
        string searchUrl = $"https://commons.wikimedia.org/w/api.php?action=query&format=json&list=search&srsearch={encodedQuery}&srnamespace=6&srlimit=1&origin=*";
        Debug.Log($"[Wikimedia] Поиск: {searchUrl}");

        using (UnityWebRequest www = UnityWebRequest.Get(searchUrl))
        {
            www.SetRequestHeader("User-Agent", userAgent);
            yield return www.SendWebRequest();

            if (token.IsCancellationRequested)
            {
                Debug.Log("[Wikimedia] Поиск отменён пользователем.");
                // Очистка токена в этом случае
                webImageCts?.Dispose();
                webImageCts = null;
                yield break;
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Wikimedia] Ошибка поиска: {www.error}");
                if (!token.IsCancellationRequested)
                {
                    StartCoroutine(FetchImageFromOpenverse(query, token)); 
                }
                yield break;
            }
            WikimediaSearchResponse searchResponse = null;
            try
            {
                searchResponse = JsonUtility.FromJson<WikimediaSearchResponse>(www.downloadHandler.text);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Wikimedia] Ошибка парсинга: {e.Message}");
                if (!token.IsCancellationRequested)
                {
                    StartCoroutine(FetchImageFromOpenverse(query, token)); 
                }
                yield break;
            }

            if (searchResponse?.query?.search == null || searchResponse.query.search.Length == 0)
            {
                Debug.LogWarning($"[Wikimedia] Нет результатов для '{query}'");

                if (!token.IsCancellationRequested)
                {
                    StartCoroutine(FetchImageFromOpenverse(query, token)); // Передаём токен
                }
                yield break;
            }
            string fileName = searchResponse.query.search[0].title;
            Debug.Log($"[Wikimedia] Найден файл: {fileName}");
            // Удаляем префикс "File:" если он есть
            if (fileName.StartsWith("File:"))
            {
                fileName = fileName.Substring(5);
            }
            string encodedFileName = UnityWebRequest.EscapeURL(fileName);
            // Добавляем префикс "File:" в параметр titles, но уже в URL
            string imageInfoUrl = $"https://commons.wikimedia.org/w/api.php?action=query&format=json&prop=imageinfo&iiprop=url|size&titles=File:{encodedFileName}&origin=*";

            Debug.Log($"[Wikimedia] Запрос информации: {imageInfoUrl}");

            using (UnityWebRequest imgInfoReq = UnityWebRequest.Get(imageInfoUrl))
            {
                imgInfoReq.SetRequestHeader("User-Agent", userAgent);
                yield return imgInfoReq.SendWebRequest();

                if (token.IsCancellationRequested)
                {
                    Debug.Log("[Wikimedia] Запрос информации отменён пользователем.");
                    // Очистка токена в этом случае
                    webImageCts?.Dispose();
                    webImageCts = null;
                    yield break;
                }

                if (imgInfoReq.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[Wikimedia] Ошибка получения информации: {imgInfoReq.error}");
                    Debug.Log($"[Wikimedia] Ответ сервера: {imgInfoReq.downloadHandler.text}");

                    if (!token.IsCancellationRequested)
                    {
                        StartCoroutine(FetchImageFromOpenverse(query, token)); // Передаём токен
                    }
                    yield break;
                }

                // Вместо использования предопределенных классов, проверим структуру ответа
                string responseText = imgInfoReq.downloadHandler.text;
                Debug.Log($"[Wikimedia] Полный ответ API: {responseText}");
                // Ищем URL изображения в ответе с помощью простого поиска по JSON
                int urlIndex = responseText.IndexOf("\"url\":\"");
                if (urlIndex == -1)
                {
                    Debug.LogWarning($"[Wikimedia] Не найдено поле 'url' в ответе для файла {fileName}");

                    if (!token.IsCancellationRequested)
                    {
                        StartCoroutine(FetchImageFromOpenverse(query, token)); // Передаём токен
                    }
                    yield break;
                }
                int urlStart = urlIndex + 7; // длина "\"url\":\""
                int urlEnd = responseText.IndexOf("\"", urlStart);
                if (urlEnd == -1)
                {
                    Debug.LogWarning($"[Wikimedia] Некорректный формат URL в ответе для файла {fileName}");

                    if (!token.IsCancellationRequested)
                    {
                        StartCoroutine(FetchImageFromOpenverse(query, token)); // Передаём токен
                    }
                    yield break;
                }
                string escapedUrl = responseText.Substring(urlStart, urlEnd - urlStart);
                string imageUrl = escapedUrl.Replace("\\/", "/");
                Debug.Log($"[Wikimedia] Извлечен URL: {imageUrl}");

                if (token.IsCancellationRequested)
                {
                    Debug.Log("[Wikimedia] Загрузка изображения отменена перед загрузкой.");
                    // Очистка токена в этом случае
                    webImageCts?.Dispose();
                    webImageCts = null;
                    yield break;
                }

                using (UnityWebRequest imgReq = UnityWebRequestTexture.GetTexture(imageUrl))
                {
                    imgReq.SetRequestHeader("User-Agent", userAgent);
                    yield return imgReq.SendWebRequest();

                    if (token.IsCancellationRequested)
                    {
                        Debug.Log("[Wikimedia] Загрузка изображения отменена во время загрузки.");
                        // Очистка токена в этом случае
                        webImageCts?.Dispose();
                        webImageCts = null;
                        yield break;
                    }

                    if (imgReq.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"[Wikimedia] Ошибка загрузки изображения: {imgReq.error}");

                        if (!token.IsCancellationRequested)
                        {
                            StartCoroutine(FetchImageFromOpenverse(query, token)); // Передаём токен
                        }
                        yield break;
                    }
                    Texture2D tex = ((DownloadHandlerTexture)imgReq.downloadHandler).texture;
                    if (tex != null && tex.width > 10 && tex.height > 10)
                    {
                        if (!token.IsCancellationRequested)
                        {
                            if (loadingIndicator != null)
                            {
                                loadingIndicator.SetActive(false);
                                isRotating = false;
                            }
                            GameData.InputMode = "web image";
                            GameData.UserImage = tex;
                            SetAIPromptImage(tex);
                            currentState = AppState.ImageConfirmed; // Устанавливаем состояние здесь
                            if (promptImage != null) ShowUIElement(promptImage);
                            if (confirmButton2 != null) ShowUIElement(confirmButton2.gameObject);
                            if (backToInputButton != null) ShowUIElement(backToInputButton.gameObject);
                            Debug.Log($"[Wikimedia] Успешно загружено изображение для '{query}'");
                        }
                        else
                        {
                            Debug.Log("[Wikimedia] Установка изображения отменена.");
                        }

                    }
                    else
                    {
                        Debug.LogWarning($"[Wikimedia] Изображение слишком маленькое или повреждено: {tex?.width}x{tex?.height}");

                        if (!token.IsCancellationRequested)
                        {
                            StartCoroutine(FetchImageFromOpenverse(query, token)); // Передаём токен
                        }
                    }
                }
            }
        }
    }

    IEnumerator FetchImageFromOpenverse(string query, CancellationToken token)
    {
        string url = $"https://api.openverse.engineering/v1/images/?q={UnityWebRequest.EscapeURL(query)}&page_size=1&format=json";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (token.IsCancellationRequested)
            {
                Debug.Log("[Openverse] Поиск отменён пользователем.");
                // Очистка токена в этом случае
                webImageCts?.Dispose();
                webImageCts = null;
                yield break;
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Openverse ошибка: " + www.error);
                OnImageFetchFailed(token); // Передаём токен
                yield break;
            }
            var response = JsonUtility.FromJson<OpenverseResponse>(www.downloadHandler.text);
            if (response?.results == null || response.results.Length == 0)
            {
                Debug.LogWarning("Изображения не найдены.");
                OnImageFetchFailed(token); // Передаём токен
                yield break;
            }
            string imageUrl = response.results[0].url;
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                OnImageFetchFailed(token); // Передаём токен
                yield break;
            }

            if (token.IsCancellationRequested)
            {
                Debug.Log("[Openverse] Загрузка изображения отменена перед загрузкой.");
                // Очистка токена в этом случае
                webImageCts?.Dispose();
                webImageCts = null;
                yield break;
            }

            using (UnityWebRequest imgRequest = UnityWebRequestTexture.GetTexture(imageUrl))
            {
                yield return imgRequest.SendWebRequest();

                if (token.IsCancellationRequested)
                {
                    Debug.Log("[Openverse] Загрузка изображения отменена во время загрузки.");
                    // Очистка токена в этом случае
                    webImageCts?.Dispose();
                    webImageCts = null;
                    yield break;
                }

                if (imgRequest.result != UnityWebRequest.Result.Success)
                {
                    OnImageFetchFailed(token); // Передаём токен
                    yield break;
                }
                Texture2D tex = ((DownloadHandlerTexture)imgRequest.downloadHandler).texture;
                if (tex != null && tex.width > 10 && tex.height > 10)
                {
                    if (!token.IsCancellationRequested)
                    {
                        if (loadingIndicator != null)
                        {
                            loadingIndicator.SetActive(false);
                            isRotating = false;
                        }
                        GameData.InputMode = "web image";
                        GameData.UserImage = tex;
                        SetAIPromptImage(tex);
                        currentState = AppState.ImageConfirmed; // Устанавливаем состояние здесь
                        if (promptImage != null) ShowUIElement(promptImage);
                        if (confirmButton2 != null) ShowUIElement(confirmButton2.gameObject);
                        if (backToInputButton != null) ShowUIElement(backToInputButton.gameObject);
                    }
                    else
                    {
                        Debug.Log("[Openverse] Установка изображения отменена.");
                    }
                }
                else
                {
                    OnImageFetchFailed(token); // Передаём токен
                }
            }
        }
    }

    void OnImageFetchFailed(CancellationToken token)
    {

        if (token.IsCancellationRequested)
        {
            Debug.Log("OnImageFetchFailed: Вызов отменён, игнорируем установку fallback.");
            // Очистка токена в этом случае, если он относился к веб-загрузке
            if (token == webImageCts?.Token)
            {
                webImageCts?.Dispose();
                webImageCts = null;
            }
            return; 
        }

        if (miniGame != null && miniGame.isActive)
        {
            miniGame.StopMiniGame();
        }

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
            isRotating = false;
        }

        SetPromptImage("banana");
        GameData.InputMode = "banana";
        GameData.UserImage = null;

        //currentState = AppState.ImageConfirmed;
        if (promptImage != null) ShowUIElement(promptImage);
        //if (confirmButton2 != null) ShowUIElement(confirmButton2.gameObject);
        if (backToInputButton != null) ShowUIElement(backToInputButton.gameObject);
    }

    void OnConfirmButton2Clicked()
    {
        HideUIElement(confirmButton2.gameObject);
        if (promptImage != null) HideUIElement(promptImage);
        if (backToInputButton != null) HideUIElement(backToInputButton.gameObject);

        currentState = AppState.ChoiceSelection;

        if (classikButton != null) ShowUIElement(classikButton.gameObject, 0.3f, 0.1f);
        if (randomChoiceButton != null) ShowUIElement(randomChoiceButton.gameObject, 0.3f, 0.2f);
    }

    void OnChoiceSelected(string choice)
    {
        selectedChoice = choice;
        GameData.SelectedMode = choice;
        currentState = AppState.LevelSelection;

        HideUIElement(classikButton.gameObject);
        HideUIElement(randomChoiceButton.gameObject);

        float delay = 0.1f;
        if (level1Button != null) ShowUIElement(level1Button.gameObject, 0.25f, delay);
        if (level2Button != null) ShowUIElement(level2Button.gameObject, 0.25f, delay + 0.05f);
        if (level3Button != null) ShowUIElement(level3Button.gameObject, 0.25f, delay + 0.1f);
        if (level4Button != null) ShowUIElement(level4Button.gameObject, 0.25f, delay + 0.15f);
    }

    void OnLevelSelected(string level)
    {
        selectedLevel = level;
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
            StartCoroutine(RotateLoadingIndicator());
        }

        if (level1Button != null) HideUIElement(level1Button.gameObject);
        if (level2Button != null) HideUIElement(level2Button.gameObject);
        if (level3Button != null) HideUIElement(level3Button.gameObject);
        if (level4Button != null) HideUIElement(level4Button.gameObject);

        if (backToInputButton != null) HideUIElement(backToInputButton.gameObject);

        StartCoroutine(ProcessSecondLoading());
    }

    IEnumerator ProcessSecondLoading()
    {
        yield return new WaitForSeconds(2f);
        if (loadingIndicator != null)
        {
            isRotating = false;
            loadingIndicator.SetActive(false);
        }

        GameData.SelectedLevel = selectedLevel;
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    void SetPromptImage(string imageName)
    {
        if (promptImage == null) return;
        Sprite sprite = Resources.Load<Sprite>(imageName) ?? Resources.Load<Sprite>("banana");
        Image img = promptImage.GetComponent<Image>();
        if (img != null) img.sprite = sprite;
    }

    void SetUserPromptImage()
    {
        if (promptImage == null || userTexture == null) return;
        Image img = promptImage.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = Sprite.Create(userTexture, new Rect(0, 0, userTexture.width, userTexture.height), Vector2.one * 0.5f);
        }
    }

    void SetAIPromptImage(Texture2D tex)
    {
        if (promptImage == null || tex == null) return;
        Image img = promptImage.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);
        }
    }

    IEnumerator RotateLoadingIndicator()
    {
        isRotating = true;
        RectTransform rect = loadingIndicator.GetComponent<RectTransform>();
        rect.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.LocalAxisAdd)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear)
            .SetId("loadingRotation");

        while (isRotating) yield return null;
        DOTween.Kill("loadingRotation");
        loadingIndicator.SetActive(false);
    }

    private void ReturnToInputSelection()
    {
        aiGenerationCts?.Cancel();
        webImageCts?.Cancel();
        userImageCts?.Cancel();

        aiGenerationCts?.Dispose();
        webImageCts?.Dispose();
        userImageCts?.Dispose();
        aiGenerationCts = null;
        webImageCts = null;
        userImageCts = null;

        currentState = AppState.InputSelection;

        // Останавливаем вращение индикатора
        isRotating = false;
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
            DOTween.Kill("loadingRotation");
        }

        // Сбрасываем все возможные состояния, которые могут скрывать поле ввода
        ResetAllUIStates();

        // Гарантированно показываем основные элементы ввода
        if (inputPanel != null)
        {
            inputPanel.SetActive(true);
            ShowUIElement(inputPanel, 0.2f);
        }

        if (inputField != null)
        {
            inputField.gameObject.SetActive(true);
            ShowUIElement(inputField.gameObject, 0.2f);
            inputField.text = ""; // Сбрасываем текст
        }

        if (additionalImage_ != null)
        {
            additionalImage_.SetActive(true);
            ShowUIElement(additionalImage_, 0.2f);
        }

        // ВСЕГДА показываем кнопку случайной генерации
        if (randomButton != null)
        {
            randomButton.gameObject.SetActive(true);
            ShowUIElement(randomButton.gameObject, 0.2f);
            _isRandomButtonVisible = true;
        }

        // ВСЕГДА показываем кнопку галереи
        if (userImageButton != null)
        {
            userImageButton.gameObject.SetActive(true);
            ShowUIElement(userImageButton.gameObject, 0.2f);
            _isUserImageButtonVisible = true;
        }

        // Сбрасываем флаги видимости для кнопок, зависящих от текста
        _isAIGenerateButtonVisible = false;
        _isSearchWebImageButtonVisible = false;

        // Принудительно обновляем состояние кнопок на основе пустого поля ввода
        if (inputField != null)
        {
            OnInputFieldValueChanged("");
        }
    }

    private void ResetAllUIStates()
    {
        // Скрываем все элементы, которые не должны быть видны в состоянии выбора ввода
        if (aiWarningText != null)
        {
            aiWarningText.SetActive(false);
            HideUIElement(aiWarningText);
        }
        if (aeroHockeyButton != null)
        {
            aeroHockeyButton.gameObject.SetActive(false);
            HideUIElement(aeroHockeyButton.gameObject);
        }
        if (backDuringGenerationButton != null)
        {
            backDuringGenerationButton.gameObject.SetActive(false);
            HideUIElement(backDuringGenerationButton.gameObject);
        }
        if (promptImage != null)
        {
            promptImage.SetActive(false);
            HideUIElement(promptImage);
        }
        if (confirmButton2 != null)
        {
            confirmButton2.gameObject.SetActive(false);
            HideUIElement(confirmButton2.gameObject);
        }
        if (backToInputButton != null)
        {
            backToInputButton.gameObject.SetActive(false);
            HideUIElement(backToInputButton.gameObject);
        }
        if (classikButton != null)
        {
            classikButton.gameObject.SetActive(false);
            HideUIElement(classikButton.gameObject);
        }
        if (randomChoiceButton != null)
        {
            randomChoiceButton.gameObject.SetActive(false);
            HideUIElement(randomChoiceButton.gameObject);
        }
        if (level1Button != null)
        {
            level1Button.gameObject.SetActive(false);
            HideUIElement(level1Button.gameObject);
        }
        if (level2Button != null)
        {
            level2Button.gameObject.SetActive(false);
            HideUIElement(level2Button.gameObject);
        }
        if (level3Button != null)
        {
            level3Button.gameObject.SetActive(false);
            HideUIElement(level3Button.gameObject);
        }
        if (level4Button != null)
        {
            level4Button.gameObject.SetActive(false);
            HideUIElement(level4Button.gameObject);
        }
    }

    private void ReturnFromChoiceToImage()
    {
        currentState = AppState.ImageConfirmed;

        // Скрываем кнопки выбора метода
        if (classikButton != null) HideUIElement(classikButton.gameObject);
        if (randomChoiceButton != null) HideUIElement(randomChoiceButton.gameObject);

        // Показываем подтверждение изображения
        if (promptImage != null) ShowUIElement(promptImage);
        if (confirmButton2 != null) ShowUIElement(confirmButton2.gameObject);
        if (backToInputButton != null) ShowUIElement(backToInputButton.gameObject);
    }

    private void ReturnFromLevelToChoice()
    {
        currentState = AppState.ChoiceSelection;

        // Скрываем кнопки уровней
        if (level1Button != null) HideUIElement(level1Button.gameObject);
        if (level2Button != null) HideUIElement(level2Button.gameObject);
        if (level3Button != null) HideUIElement(level3Button.gameObject);
        if (level4Button != null) HideUIElement(level4Button.gameObject);

        // Показываем кнопки выбора метода
        if (classikButton != null) ShowUIElement(classikButton.gameObject);
        if (randomChoiceButton != null) ShowUIElement(randomChoiceButton.gameObject);
    }

    private void ReturnToStart()
    {
        aiGenerationCts?.Cancel();
        webImageCts?.Cancel();
        userImageCts?.Cancel();

        aiGenerationCts?.Dispose();
        webImageCts?.Dispose();
        userImageCts?.Dispose();
        aiGenerationCts = null;
        webImageCts = null;
        userImageCts = null;

        currentState = AppState.Start;
        // Скрываем все элементы
        if (inputPanel != null) HideUIElement(inputPanel);
        if (additionalImage != null) HideUIElement(additionalImage);
        if (additionalImage_ != null) HideUIElement(additionalImage_);
        // Показываем стартовую кнопку
        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
            ShowUIElement(startButton.gameObject);
        }
    }

    void ShowUIElement(GameObject go, float duration = 0.3f, float delay = 0f)
    {
        if (go == null) return;
        // Гарантируем, что объект активен перед анимацией
        go.SetActive(true);
        var rect = go.GetComponent<RectTransform>();
        var cg = go.GetComponent<CanvasGroup>();
        // Если CanvasGroup нет - создаем
        if (cg == null)
        {
            cg = go.AddComponent<CanvasGroup>();
        }
        // Сбрасываем состояние перед анимацией
        rect.localScale = Vector3.one;
        cg.alpha = 1f;
        // Если длительность 0 - просто показываем без анимации
        if (duration <= 0f)
        {
            return;
        }
        // Анимация появления
        rect.localScale = Vector3.zero;
        cg.alpha = 0f;
        rect.DOScale(Vector3.one, duration).SetEase(Ease.OutBack).SetDelay(delay);
        cg.DOFade(1f, duration).SetDelay(delay);
    }

    void HideUIElement(GameObject go, float duration = 0.3f)
    {
        if (go == null) return;
        var rect = go.GetComponent<RectTransform>();
        var cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
        rect.DOScale(Vector3.zero, duration).SetEase(Ease.InBack).OnComplete(() => go.SetActive(false));
        cg.DOFade(0f, duration);
    }

    public void AnimateButtonPress(Button button)
    {
        if (button == null) return;
        AudioManager.Instance?.PlayButtonClick();
        var rect = button.GetComponent<RectTransform>();
        rect.DOScale(0.9f, 0.1f).OnComplete(() => rect.DOScale(1f, 0.1f).SetEase(Ease.OutBack));
    }
}

[System.Serializable]
public class WikimediaSearchResponse
{
    public WikimediaSearchQuery query;
}

[System.Serializable]
public class WikimediaSearchQuery
{
    public WikimediaSearchResult[] search;
}

[System.Serializable]
public class WikimediaSearchResult
{
    public int ns;
    public string title;
    public string snippet;
}

[System.Serializable]
public class WikimediaImageInfoResponse
{
    public WikimediaImageInfoPages query;
}

[System.Serializable]
public class WikimediaImageInfoPages
{
    public WikimediaImageInfoPage[] pages;
}

[System.Serializable]
public class WikimediaImageInfoPage
{
    public int pageid;
    public string title;
    public WikimediaImageInfoDetail[] imageinfo;
}

[System.Serializable]
public class WikimediaImageInfoDetail
{
    public string url;
    public int width;
    public int height;
}