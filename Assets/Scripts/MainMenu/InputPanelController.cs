using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class InputPanelController : MonoBehaviour
{
    [Header("Основные UI элементы")]
    public Button startButton;               // Стартовая кнопка
    public GameObject inputPanel;            // Основная панель ввода
    public TMP_InputField inputField;        // Поле ввода TextMeshPro
    public Button confirmButton;             // Кнопка подтверждения
    public Button randomButton;              // Кнопка случайной генерации
    public Button userImageButton;           // Кнопка загрузки своего изображения

    [Header("Дополнительное изображение")]
    public GameObject additionalImage;
    public GameObject additionalImage_;
    public GameObject promptImage;

    [Header("Загрузочный индикатор")]
    public GameObject loadingIndicator;

    [Header("Кнопка после загрузки")]
    public Button confirmButton2;           // Новая кнопка подтверждения после загрузки

    [Header("Кнопки выбора")]
    public Button classikButton;
    public Button randomChoiceButton;

    [Header("Кнопки уровней сложности")]
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    public Button level4Button;

    [Header("Параметры анимации")]
    public float appearDuration = 0.5f;      // Длительность появления
    public Vector2 targetScale = Vector2.one; // Конечный масштаб панели

    private string savedInput;
    private string selectedChoice;
    private string selectedLevel;
    private Texture2D userTexture;          // Текстура пользовательского изображения

    private CanvasGroup panelCanvasGroup;
    private RectTransform panelRectTransform;
    private CanvasGroup imageCanvasGroup;
    private RectTransform imageRectTransform;
    private CanvasGroup imageCanvasGroup_;
    private RectTransform imageRectTransform_;

    private Gen_image_AI aiGenerator;

    private bool isRotating = false;

    private bool _isConfirmButtonVisible = false;
    private bool _isRandomButtonVisible = false;
    private bool _isUserImageButtonVisible = false;

    void Start()
    {
        // Получаем компоненты панели
        panelRectTransform = inputPanel.GetComponent<RectTransform>();
        panelCanvasGroup = inputPanel.GetComponent<CanvasGroup>()
            ?? inputPanel.AddComponent<CanvasGroup>();

        // Получаем компоненты дополнительного изображения (если задано)
        if (additionalImage != null)
        {
            imageRectTransform = additionalImage.GetComponent<RectTransform>();
            imageCanvasGroup = additionalImage.GetComponent<CanvasGroup>()
                ?? additionalImage.AddComponent<CanvasGroup>();
        }

        if (additionalImage_ != null)
        {
            imageRectTransform_ = additionalImage_.GetComponent<RectTransform>();
            imageCanvasGroup_ = additionalImage_.GetComponent<CanvasGroup>()
                ?? additionalImage_.AddComponent<CanvasGroup>();
        }

        aiGenerator = Object.FindFirstObjectByType<Gen_image_AI>();
        if (aiGenerator == null)
        {
            Debug.LogWarning("Gen_image_AI не найден на сцене. Генерация изображений недоступна.");
        }

        // Убеждаемся, что всё скрыто изначально
        inputPanel.SetActive(false);
        if (additionalImage != null) additionalImage.SetActive(false);
        if (additionalImage_ != null) additionalImage_.SetActive(false);
        if (promptImage != null) promptImage.SetActive(false);
        if (loadingIndicator != null) loadingIndicator.SetActive(false);
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);
        if (confirmButton2 != null) confirmButton2.gameObject.SetActive(false);
        if (classikButton != null) classikButton.gameObject.SetActive(false);
        if (randomChoiceButton != null) randomChoiceButton.gameObject.SetActive(false);
        if (level1Button != null) level1Button.gameObject.SetActive(false);
        if (level2Button != null) level2Button.gameObject.SetActive(false);
        if (level3Button != null) level3Button.gameObject.SetActive(false);
        if (level4Button != null) level4Button.gameObject.SetActive(false);
        if (userImageButton != null) userImageButton.gameObject.SetActive(false);

        // Подписка на события кнопок с анимацией нажатия
        if (startButton != null)
            startButton.onClick.AddListener(() => { AnimateButtonPress(startButton); OnStartButtonClicked(); });

        if (randomButton != null)
            randomButton.onClick.AddListener(() => { AnimateButtonPress(randomButton); OnRandomButtonClicked(); });

        if (confirmButton != null)
            confirmButton.onClick.AddListener(() => { AnimateButtonPress(confirmButton); OnConfirmButtonClicked(); });

        if (classikButton != null)
            classikButton.onClick.AddListener(() => { AnimateButtonPress(classikButton); OnChoiceSelected("classik"); });

        if (randomChoiceButton != null)
            randomChoiceButton.onClick.AddListener(() => { AnimateButtonPress(randomChoiceButton); OnChoiceSelected("random"); });

        if (level1Button != null)
            level1Button.onClick.AddListener(() => { AnimateButtonPress(level1Button); OnLevelSelected("level1"); });
        if (level2Button != null)
            level2Button.onClick.AddListener(() => { AnimateButtonPress(level2Button); OnLevelSelected("level2"); });
        if (level3Button != null)
            level3Button.onClick.AddListener(() => { AnimateButtonPress(level3Button); OnLevelSelected("level3"); });
        if (level4Button != null)
            level4Button.onClick.AddListener(() => { AnimateButtonPress(level4Button); OnLevelSelected("level4"); });

        if (userImageButton != null)
            userImageButton.onClick.AddListener(() => { AnimateButtonPress(userImageButton); OnUserImageButtonClicked(); });

        if (confirmButton2 != null)
            confirmButton2.onClick.AddListener(() => { AnimateButtonPress(confirmButton2); OnConfirmButton2Clicked(); });

        if (inputField != null)
            inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
    }

    void OnInputFieldValueChanged(string newText)
    {
        bool isEmpty = string.IsNullOrWhiteSpace(newText);
        bool shouldShowConfirm = !isEmpty;
        bool shouldShowRandom = isEmpty;
        bool shouldShowUserImage = isEmpty;

        if (confirmButton != null)
        {
            if (shouldShowConfirm && !_isConfirmButtonVisible)
            {
                ShowUIElement(confirmButton.gameObject, 0.2f);
                _isConfirmButtonVisible = true;
            }
            else if (!shouldShowConfirm && _isConfirmButtonVisible)
            {
                HideUIElement(confirmButton.gameObject, 0.2f);
                _isConfirmButtonVisible = false;
            }
        }

        if (randomButton != null)
        {
            if (shouldShowRandom && !_isRandomButtonVisible)
            {
                ShowUIElement(randomButton.gameObject, 0.2f);
                _isRandomButtonVisible = true;
            }
            else if (!shouldShowRandom && _isRandomButtonVisible)
            {
                HideUIElement(randomButton.gameObject, 0.2f);
                _isRandomButtonVisible = false;
            }
        }

        if (userImageButton != null)
        {
            if (shouldShowUserImage && !_isUserImageButtonVisible)
            {
                ShowUIElement(userImageButton.gameObject, 0.2f);
                _isUserImageButtonVisible = true;
            }
            else if (!shouldShowUserImage && _isUserImageButtonVisible)
            {
                HideUIElement(userImageButton.gameObject, 0.2f);
                _isUserImageButtonVisible = false;
            }
        }
    }

    void OnStartButtonClicked()
    {
        startButton.gameObject.SetActive(false); // мгновенно скрываем стартовую кнопку

        ShowUIElement(inputPanel, appearDuration);
        if (additionalImage != null) ShowUIElement(additionalImage, appearDuration);
        if (additionalImage_ != null) ShowUIElement(additionalImage_, appearDuration);

        if (inputField != null)
            OnInputFieldValueChanged(inputField.text);
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

    void OnConfirmButtonClicked()
    {
        savedInput = inputField.text.Trim();
        Debug.Log("Сохранено: " + savedInput);

        HideInputElements();

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
            StartCoroutine(RotateLoadingIndicator());
        }

        StartCoroutine(ProcessSubmission());
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
                    Debug.LogError("Не удалось загрузить изображение из галереи");
                    ShowInputElements();
                }
            }
            else
            {
                Debug.Log("Пользователь отменил выбор изображения");
                ShowInputElements();
            }
        }, "Выберите изображение", "image/*");
#else
        Debug.Log("В редакторе Unity функционал галереи недоступен");
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

    void HideInputElements()
    {
        if (inputField != null) HideUIElement(inputField.gameObject);
        if (randomButton != null) HideUIElement(randomButton.gameObject);
        if (confirmButton != null) HideUIElement(confirmButton.gameObject);
        if (userImageButton != null) HideUIElement(userImageButton.gameObject);
        if (additionalImage_ != null) HideUIElement(additionalImage_.gameObject);
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
        bool fromResources = false;

        Sprite spriteFromResources = Resources.Load<Sprite>(savedInput);
        if (spriteFromResources != null && spriteFromResources.texture != null)
        {
            loadedTexture = spriteFromResources.texture;
            fromResources = true;
            Debug.Log($"✅ Изображение '{savedInput}' найдено в Resources. Пропускаем генерацию ИИ.");
        }
        else
        {
            if (aiGenerator != null)
            {
                Debug.Log($"🔄 Изображение '{savedInput}' не найдено в Resources. Запускаем генерацию через ИИ...");
                yield return StartCoroutine(aiGenerator.GenerateImage(savedInput, (tex) => loadedTexture = tex));
            }
            else
            {
                yield return new WaitForSeconds(2f);
                Debug.LogWarning("Gen_image_AI не найден. Используется fallback 'banana'.");
                spriteFromResources = Resources.Load<Sprite>("banana");
                if (spriteFromResources != null)
                    loadedTexture = spriteFromResources.texture;
            }
        }

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
            isRotating = false;
        }

        if (loadedTexture != null)
        {
            if (fromResources)
            {
                SetPromptImage(savedInput);
                GameData.InputMode = savedInput;
                GameData.UserImage = null;
            }
            else
            {
                GameData.InputMode = "user image";
                GameData.UserImage = loadedTexture;
                SetAIPromptImage(loadedTexture);
            }
        }
        else
        {
            Debug.LogError("Не удалось получить изображение ни из Resources, ни через ИИ.");
            SetPromptImage("banana");
            GameData.InputMode = "banana";
            GameData.UserImage = null;
        }

        if (promptImage != null)
            ShowUIElement(promptImage);

        if (confirmButton2 != null)
        {
            ShowUIElement(confirmButton2.gameObject);
        }
    }

    IEnumerator ProcessUserImageSubmission()
    {
        yield return new WaitForSeconds(2f);

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
            isRotating = false;
        }

        SetUserPromptImage();

        if (promptImage != null)
            ShowUIElement(promptImage);

        if (confirmButton2 != null)
        {
            ShowUIElement(confirmButton2.gameObject);
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

        while (isRotating)
            yield return null;

        DOTween.Kill("loadingRotation");
        loadingIndicator.SetActive(false);
    }

    void OnConfirmButton2Clicked()
    {
        HideUIElement(confirmButton2.gameObject);
        if (promptImage != null) HideUIElement(promptImage);

        if (classikButton != null) ShowUIElement(classikButton.gameObject, 0.3f, 0.1f);
        if (randomChoiceButton != null) ShowUIElement(randomChoiceButton.gameObject, 0.3f, 0.2f);
    }

    void OnChoiceSelected(string choice)
    {
        selectedChoice = choice;
        Debug.Log("Выбрано: " + selectedChoice);

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
        Debug.Log("Выбран уровень: " + selectedLevel);

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
            StartCoroutine(RotateLoadingIndicator());
        }

        if (level1Button != null) HideUIElement(level1Button.gameObject);
        if (level2Button != null) HideUIElement(level2Button.gameObject);
        if (level3Button != null) HideUIElement(level3Button.gameObject);
        if (level4Button != null) HideUIElement(level4Button.gameObject);

        StartCoroutine(ProcessSecondLoading());
    }

    void SetPromptImage(string imageName)
    {
        if (promptImage == null)
        {
            Debug.LogWarning("promptImage не назначен в инспекторе!");
            return;
        }

        Sprite loadedSprite = Resources.Load<Sprite>(imageName);
        if (loadedSprite == null)
        {
            Debug.Log($"Изображение '{imageName}' не найдено. Используется 'banana'.");
            loadedSprite = Resources.Load<Sprite>("banana");
        }

        Image imageComponent = promptImage.GetComponent<Image>();
        if (imageComponent != null)
        {
            imageComponent.sprite = loadedSprite;
        }
        else
        {
            Debug.LogError("promptImage не содержит компонент Image!");
        }
    }

    void SetUserPromptImage()
    {
        if (promptImage == null || userTexture == null)
        {
            Debug.LogWarning("promptImage или userTexture не назначены!");
            return;
        }

        Image imageComponent = promptImage.GetComponent<Image>();
        if (imageComponent != null)
        {
            Sprite userSprite = Sprite.Create(userTexture,
                new Rect(0, 0, userTexture.width, userTexture.height),
                Vector2.one * 0.5f);
            imageComponent.sprite = userSprite;
            Debug.Log("Установлено пользовательское изображение");
        }
        else
        {
            Debug.LogError("promptImage не содержит компонент Image!");
        }
    }

    void SetAIPromptImage(Texture2D aiTexture)
    {
        if (promptImage == null || aiTexture == null)
        {
            Debug.LogWarning("promptImage или aiTexture не назначены!");
            return;
        }

        Image imageComponent = promptImage.GetComponent<Image>();
        if (imageComponent != null)
        {
            Sprite aiSprite = Sprite.Create(aiTexture,
                new Rect(0, 0, aiTexture.width, aiTexture.height),
                new Vector2(0.5f, 0.5f),
                100);
            imageComponent.sprite = aiSprite;
            Debug.Log("Установлено сгенерированное AI изображение");
        }
        else
        {
            Debug.LogError("promptImage не содержит компонент Image!");
        }
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

    // ─── ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ АНИМАЦИИ ───────────────────────────────────────

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

        rect.DOScale(Vector3.zero, duration).SetEase(Ease.InBack)
            .OnComplete(() => go.SetActive(false));
        cg.DOFade(0f, duration);
    }

    public void AnimateButtonPress(Button button)
    {
        if (button == null) return;
        var rect = button.GetComponent<RectTransform>();
        rect.DOScale(0.9f, 0.1f)
            .OnComplete(() => rect.DOScale(1f, 0.1f).SetEase(Ease.OutBack));
    }
}