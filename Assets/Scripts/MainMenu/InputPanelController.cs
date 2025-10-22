using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

        aiGenerator = FindObjectOfType<Gen_image_AI>();
        if (aiGenerator == null)
        {
            Debug.LogWarning("Gen_image_AI не найден на сцене. Генерация изображений недоступна.");
        }

        // Убеждаемся, что всё скрыто изначально
        inputPanel.SetActive(false);
        if (additionalImage != null)
            additionalImage.SetActive(false);

        if (additionalImage_ != null)
            additionalImage_.SetActive(false);

        if (promptImage != null)
            promptImage.SetActive(false);

        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);

        if (confirmButton != null)
            confirmButton.gameObject.SetActive(false);

        if (confirmButton2 != null)
            confirmButton2.gameObject.SetActive(false);

        if (classikButton != null)
            classikButton.gameObject.SetActive(false);

        if (randomChoiceButton != null)
            randomChoiceButton.gameObject.SetActive(false);

        if (level1Button != null)
            level1Button.gameObject.SetActive(false);
        if (level2Button != null)
            level2Button.gameObject.SetActive(false);
        if (level3Button != null)
            level3Button.gameObject.SetActive(false);
        if (level4Button != null)
            level4Button.gameObject.SetActive(false);

        if (userImageButton != null)
            userImageButton.gameObject.SetActive(false);

        // Подписка на события кнопок
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (randomButton != null)
            randomButton.onClick.AddListener(OnRandomButtonClicked);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);

        if (classikButton != null)
            classikButton.onClick.AddListener(() => OnChoiceSelected("classik"));

        if (randomChoiceButton != null)
            randomChoiceButton.onClick.AddListener(() => OnChoiceSelected("random"));

        if (level1Button != null)
            level1Button.onClick.AddListener(() => OnLevelSelected("level1"));
        if (level2Button != null)
            level2Button.onClick.AddListener(() => OnLevelSelected("level2"));
        if (level3Button != null)
            level3Button.onClick.AddListener(() => OnLevelSelected("level3"));
        if (level4Button != null)
            level4Button.onClick.AddListener(() => OnLevelSelected("level4"));

        if (userImageButton != null)
            userImageButton.onClick.AddListener(OnUserImageButtonClicked);

        if (inputField != null)
            inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
    }

    void OnInputFieldValueChanged(string newText)
    {
        bool isEmpty = string.IsNullOrWhiteSpace(newText);

        // Показываем кнопку подтверждения, если текст НЕ пустой
        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(!string.IsNullOrWhiteSpace(newText));
        }

        // Кнопка случайной генерации видна, если текст ПУСТОЙ
        if (randomButton != null)
            randomButton.gameObject.SetActive(isEmpty);

        // Кнопка загрузки изображения видна, если текст ПУСТОЙ
        if (userImageButton != null)
            userImageButton.gameObject.SetActive(isEmpty);
    }

    void OnStartButtonClicked()
    {
        // Скрываем стартовую кнопку
        startButton.gameObject.SetActive(false);

        // Активируем панель и изображение
        inputPanel.SetActive(true);
        if (additionalImage != null)
            additionalImage.SetActive(true);
        if (additionalImage_ != null)
            additionalImage_.SetActive(true);

        // Сбрасываем начальные состояния
        panelRectTransform.localScale = Vector3.zero;
        panelCanvasGroup.alpha = 0f;

        if (additionalImage != null)
        {
            imageRectTransform.localScale = Vector3.zero;
            imageCanvasGroup.alpha = 0f;
        }

        if (additionalImage_ != null)
        {
            imageRectTransform_.localScale = Vector3.zero;
            imageCanvasGroup_.alpha = 0f;
        }

        if (inputField != null)
            OnInputFieldValueChanged(inputField.text);

        // Запускаем анимацию появления
        StartCoroutine(AppearUI());
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

    IEnumerator AppearUI()
    {
        float elapsedTime = 0f;

        while (elapsedTime < appearDuration)
        {
            float t = elapsedTime / appearDuration;

            // Анимация панели
            panelRectTransform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            // Анимация дополнительного изображения
            if (additionalImage != null)
            {
                imageRectTransform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
                imageCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            }

            if (additionalImage_ != null)
            {
                imageRectTransform_.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
                imageCanvasGroup_.alpha = Mathf.Lerp(0f, 1f, t);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Финальное состояние
        panelRectTransform.localScale = targetScale;
        panelCanvasGroup.alpha = 1f;

        if (additionalImage != null)
        {
            imageRectTransform.localScale = targetScale;
            imageCanvasGroup.alpha = 1f;
        }

        if (additionalImage_ != null)
        {
            imageRectTransform_.localScale = targetScale;
            imageCanvasGroup_.alpha = 1f;
        }
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
        // Скрываем стандартные элементы ввода
        HideInputElements();

        // Запрашиваем изображение из галереи
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
        // Эмуляция в редакторе Unity
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
        if (inputField != null) inputField.gameObject.SetActive(false);
        if (randomButton != null) randomButton.gameObject.SetActive(false);
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);
        if (userImageButton != null) userImageButton.gameObject.SetActive(false);
        if (additionalImage_ != null) additionalImage_.SetActive(false);
    }

    void ShowInputElements()
    {
        if (inputField != null) inputField.gameObject.SetActive(true);
        if (additionalImage_ != null) additionalImage_.SetActive(true);

        // Обновляем видимость кнопок в зависимости от содержимого поля
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

        // 🔍 ШАГ 1: Проверяем, есть ли изображение в Resources
        Sprite spriteFromResources = Resources.Load<Sprite>(savedInput);
        if (spriteFromResources != null && spriteFromResources.texture != null)
        {
            loadedTexture = spriteFromResources.texture;
            fromResources = true;
            Debug.Log($"✅ Изображение '{savedInput}' найдено в Resources. Пропускаем генерацию ИИ.");
        }
        else
        {
            // 🔁 ШАГ 2: Если нет — запускаем генерацию через ИИ
            if (aiGenerator != null)
            {
                Debug.Log($"🔄 Изображение '{savedInput}' не найдено в Resources. Запускаем генерацию через ИИ...");
                yield return StartCoroutine(aiGenerator.GenerateImage(savedInput, (tex) => loadedTexture = tex));
            }
            else
            {
                // Fallback: ждём 2 сек и используем "banana"
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

        // Устанавливаем изображение
        if (loadedTexture != null)
        {
            if (fromResources)
            {
                // Из Resources → устанавливаем как обычное изображение
                SetPromptImage(savedInput); // Это уже загружает спрайт из Resources
                GameData.InputMode = savedInput; // Оставляем как текст
                GameData.UserImage = null;      // Не пользовательское
            }
            else
            {
                // От ИИ → устанавливаем как "пользовательское"
                GameData.InputMode = "user image";
                GameData.UserImage = loadedTexture;
                SetAIPromptImage(loadedTexture);
            }
        }
        else
        {
            // Fallback: если ничего не получилось — используем banana
            Debug.LogError("Не удалось получить изображение ни из Resources, ни через ИИ.");
            SetPromptImage("banana");
            GameData.InputMode = "banana";
            GameData.UserImage = null;
        }

        if (promptImage != null)
            promptImage.SetActive(true);

        if (confirmButton2 != null)
        {
            confirmButton2.gameObject.SetActive(true);
            confirmButton2.onClick.RemoveAllListeners();
            confirmButton2.onClick.AddListener(OnConfirmButton2Clicked);
        }
    }

    IEnumerator ProcessUserImageSubmission()
    {
        yield return new WaitForSeconds(2f);

        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);

        SetUserPromptImage();

        if (promptImage != null)
            promptImage.SetActive(true);

        if (confirmButton2 != null)
        {
            confirmButton2.gameObject.SetActive(true);
            confirmButton2.onClick.RemoveAllListeners();
            confirmButton2.onClick.AddListener(OnConfirmButton2Clicked);
        }
    }

    IEnumerator RotateLoadingIndicator()
    {
        isRotating = true;
        RectTransform rect = loadingIndicator.GetComponent<RectTransform>();

        while (isRotating)
        {
            rect.Rotate(0, 0, -180 * Time.deltaTime);
            yield return null;
        }
    }

    void OnConfirmButton2Clicked()
    {
        if (confirmButton2 != null)
            confirmButton2.gameObject.SetActive(false);

        if (promptImage != null)
            promptImage.SetActive(false);

        if (classikButton != null)
            classikButton.gameObject.SetActive(true);
        if (randomChoiceButton != null)
            randomChoiceButton.gameObject.SetActive(true);
    }

    void OnChoiceSelected(string choice)
    {
        selectedChoice = choice;
        Debug.Log("Выбрано: " + selectedChoice);

        if (classikButton != null)
            classikButton.gameObject.SetActive(false);
        if (randomChoiceButton != null)
            randomChoiceButton.gameObject.SetActive(false);

        if (level1Button != null)
            level1Button.gameObject.SetActive(true);
        if (level2Button != null)
            level2Button.gameObject.SetActive(true);
        if (level3Button != null)
            level3Button.gameObject.SetActive(true);
        if (level4Button != null)
            level4Button.gameObject.SetActive(true);
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

        if (level1Button != null)
            level1Button.gameObject.SetActive(false);
        if (level2Button != null)
            level2Button.gameObject.SetActive(false);
        if (level3Button != null)
            level3Button.gameObject.SetActive(false);
        if (level4Button != null)
            level4Button.gameObject.SetActive(false);

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
            // Создаем спрайт из пользовательской текстуры
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

    IEnumerator ProcessSecondLoading()
    {
        yield return new WaitForSeconds(2f);

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
            isRotating = false;
        }

        GameData.SelectedLevel = selectedLevel;


        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
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
                100); // pixels per unit

            imageComponent.sprite = aiSprite;
            Debug.Log("Установлено сгенерированное AI изображение");
        }
        else
        {
            Debug.LogError("promptImage не содержит компонент Image!");
        }
    }
}

