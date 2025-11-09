using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using UnityEngine.Networking;

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

        // Скрыть всё изначально
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

        ShowUIElement(inputPanel, appearDuration);
        if (additionalImage != null) ShowUIElement(additionalImage, appearDuration);
        if (additionalImage_ != null) ShowUIElement(additionalImage_, appearDuration);

        // Явно активируем начальные кнопки
        if (randomButton != null && !_isRandomButtonVisible)
        {
            ShowUIElement(randomButton.gameObject, 0.2f);
            _isRandomButtonVisible = true;
        }
        if (userImageButton != null && !_isUserImageButtonVisible)
        {
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

        savedInput = inputField.text.Trim();
        HideInputElements();

        // Показать предупреждение и кнопку аэрохоккея при ИИ-генерации
        if (aiWarningText != null) ShowUIElement(aiWarningText);
        if (aeroHockeyButton != null) ShowUIElement(aeroHockeyButton.gameObject);
        if (backDuringGenerationButton != null) ShowUIElement(backDuringGenerationButton.gameObject);

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
            StartCoroutine(RotateLoadingIndicator());
        }

        StartCoroutine(ProcessSubmission());
    }

    void OnAeroHockeyButtonClicked()
    {
        // Скрыть предупреждение и кнопку аэрохоккея
        if (aiWarningText != null) HideUIElement(aiWarningText);
        if (aeroHockeyButton != null) HideUIElement(aeroHockeyButton.gameObject);
        if (backDuringGenerationButton != null) ShowUIElement(backDuringGenerationButton.gameObject);

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
        HideInputElements();

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

                    StartCoroutine(ProcessUserImageSubmission());
                }
                else
                {
                    Debug.LogError("Не удалось загрузить изображение");
                    ShowInputFieldAndChoiceButtons();
                }
            }
            else
            {
                Debug.Log("Выбор отменён");
                ShowInputFieldAndChoiceButtons();
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

        StartCoroutine(ProcessUserImageSubmission());
        #endif
    }

    void ShowInputFieldAndChoiceButtons()
    {
        if (inputField != null) ShowUIElement(inputField.gameObject);

        // Показываем кнопку случайного выбора 
        if (randomButton != null)
        {
            ShowUIElement(randomButton.gameObject, 0.2f);
            _isRandomButtonVisible = true;
        }

        // Показываем кнопку галереи 
        if (userImageButton != null)
        {
            ShowUIElement(userImageButton.gameObject, 0.2f);
            _isUserImageButtonVisible = true;
        }

        if (additionalImage_ != null) ShowUIElement(additionalImage_.gameObject);

        // Также нужно вызвать OnInputFieldValueChanged, чтобы корректно отобразить
        // кнопки ИИ и Web в зависимости от содержимого поля ввода
        if (inputField != null)
        {
            OnInputFieldValueChanged(inputField.text);
        }
    }

    void OnBackToInputButtonClicked()
    {
        Debug.Log("Кнопка 'Вернуться' нажата. Сбрасываем изображение и возвращаемся к выбору ввода.");

        // Сбрасываем GameData, связанные с изображением
        GameData.InputMode = null; // или ""
        GameData.UserImage = null;

        // Скрываем изображение и кнопку подтверждения
        if (promptImage != null) HideUIElement(promptImage);
        if (confirmButton2 != null) HideUIElement(confirmButton2.gameObject);

        // Скрываем кнопку 'Вернуться'
        if (backToInputButton != null) HideUIElement(backToInputButton.gameObject);

        if (inputField != null) inputField.text = "";

        // Возвращаем элементы ввода
        ShowInputFieldAndChoiceButtons(); 
    }

    void OnBackDuringGenerationButtonClicked()
    {
        Debug.Log("Кнопка 'Вернуться во время генерации' нажата. Отменяем генерацию и возвращаемся к выбору ввода.");

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
        GameData.InputMode = null; // или ""
        GameData.UserImage = null;

        // Скрываем все элементы, связанные с генерацией
        if (aiWarningText != null) HideUIElement(aiWarningText);
        if (aeroHockeyButton != null) HideUIElement(aeroHockeyButton.gameObject);
        if (backDuringGenerationButton != null) HideUIElement(backDuringGenerationButton.gameObject); // НОВОЕ
        if (promptImage != null) HideUIElement(promptImage); // На всякий случай
        if (confirmButton2 != null) HideUIElement(confirmButton2.gameObject); // На всякий случай
        if (backToInputButton != null) HideUIElement(backToInputButton.gameObject); // На всякий случай

        // Очищаем поле ввода
        if (inputField != null) inputField.text = "";

        // Возвращаем элементы ввода
        ShowInputFieldAndChoiceButtons();
    }

    void OnSearchWebImageButtonClicked()
    {
        string query = inputField.text.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            Debug.LogWarning("Запрос пустой.");
            return;
        }

        HideInputElements();
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
            isRotating = true;
            StartCoroutine(RotateLoadingIndicator());
        }

        StartCoroutine(FetchImageFromOpenverse(query));
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

    void ShowInputElements()
    {
        if (inputField != null) ShowUIElement(inputField.gameObject);
        if (additionalImage_ != null) ShowUIElement(additionalImage_.gameObject);
        OnInputFieldValueChanged(inputField.text);
    }

    IEnumerator ProcessSubmission()
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

            yield return StartCoroutine(aiGenerator.GenerateImage(savedInput, (tex) => loadedTexture = tex));
        }
        else
        {
            yield return new WaitForSeconds(2f);
            Debug.LogWarning("Gen_image_AI не найден. Fallback на 'banana'.");
            Sprite fallback = Resources.Load<Sprite>("banana");
            if (fallback != null) loadedTexture = fallback.texture;
        }

        // Остановить аэрохоккей после завершения генерации
        if (miniGame != null && miniGame.isActive)
        {
            miniGame.StopMiniGame();
        }

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
            isRotating = false;
        }

        if (loadedTexture != null)
        {
            GameData.InputMode = "user image";
            GameData.UserImage = loadedTexture;
            SetAIPromptImage(loadedTexture);
        }
        else
        {
            Debug.LogError("Не удалось получить изображение.");
            SetPromptImage("banana");
            GameData.InputMode = "banana";
            GameData.UserImage = null;
        }

        if (promptImage != null) ShowUIElement(promptImage);
        if (confirmButton2 != null) ShowUIElement(confirmButton2.gameObject);
        if (backToInputButton != null) ShowUIElement(backToInputButton.gameObject);
    }

    IEnumerator ProcessUserImageSubmission()
    {
        yield return new WaitForSeconds(2f);

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
            isRotating = false;
        }

        GameData.InputMode = "user image";
        GameData.UserImage = userTexture;
        SetUserPromptImage();

        if (promptImage != null) ShowUIElement(promptImage);
        if (confirmButton2 != null) ShowUIElement(confirmButton2.gameObject);
        if (backToInputButton != null) ShowUIElement(backToInputButton.gameObject);
    }

    IEnumerator FetchImageFromOpenverse(string query)
    {
        string url = $"https://api.openverse.engineering/v1/images/?q={UnityWebRequest.EscapeURL(query)}&page_size=1&format=json";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Openverse ошибка: " + www.error);
                OnImageFetchFailed();
                yield break;
            }

            var response = JsonUtility.FromJson<OpenverseResponse>(www.downloadHandler.text);
            if (response?.results == null || response.results.Length == 0)
            {
                Debug.LogWarning("Изображения не найдены.");
                OnImageFetchFailed();
                yield break;
            }

            string imageUrl = response.results[0].url;
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                OnImageFetchFailed();
                yield break;
            }

            using (UnityWebRequest imgRequest = UnityWebRequestTexture.GetTexture(imageUrl))
            {
                yield return imgRequest.SendWebRequest();

                if (imgRequest.result != UnityWebRequest.Result.Success)
                {
                    OnImageFetchFailed();
                    yield break;
                }

                Texture2D tex = ((DownloadHandlerTexture)imgRequest.downloadHandler).texture;
                if (tex != null && tex.width > 10 && tex.height > 10)
                {
                    if (loadingIndicator != null)
                    {
                        loadingIndicator.SetActive(false);
                        isRotating = false;
                    }

                    GameData.InputMode = "web image";
                    GameData.UserImage = tex;
                    SetAIPromptImage(tex);

                    if (promptImage != null) ShowUIElement(promptImage);
                    if (confirmButton2 != null) ShowUIElement(confirmButton2.gameObject);
                    if (backToInputButton != null) ShowUIElement(backToInputButton.gameObject);
                }
                else
                {
                    OnImageFetchFailed();
                }
            }
        }
    }

    void OnImageFetchFailed()
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
            isRotating = false;
        }

        SetPromptImage("banana");
        GameData.InputMode = "banana";
        GameData.UserImage = null;

        if (promptImage != null) ShowUIElement(promptImage);
        if (confirmButton2 != null) ShowUIElement(confirmButton2.gameObject);
        if (backToInputButton != null) ShowUIElement(backToInputButton.gameObject);
    }

    void OnConfirmButton2Clicked()
    {
        HideUIElement(confirmButton2.gameObject);
        if (promptImage != null) HideUIElement(promptImage);
        if (classikButton != null) ShowUIElement(classikButton.gameObject, 0.3f, 0.1f);
        if (randomChoiceButton != null) ShowUIElement(randomChoiceButton.gameObject, 0.3f, 0.2f);
        if (backToInputButton != null) HideUIElement(backToInputButton.gameObject);
    }

    void OnChoiceSelected(string choice)
    {
        selectedChoice = choice;
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

    void ShowUIElement(GameObject go, float duration = 0.3f, float delay = 0f)
    {
        if (go == null) return;
        go.SetActive(true);
        var rect = go.GetComponent<RectTransform>();
        var cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
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