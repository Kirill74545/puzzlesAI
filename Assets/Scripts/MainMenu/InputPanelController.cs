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
    public Button randomButton; // Кнопка случайной генерации

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

    private CanvasGroup panelCanvasGroup;
    private RectTransform panelRectTransform;
    private CanvasGroup imageCanvasGroup;
    private RectTransform imageRectTransform;
    private CanvasGroup imageCanvasGroup_;
    private RectTransform imageRectTransform_;

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

        if (inputField != null)
            inputField.gameObject.SetActive(false);

        inputField.text = "";
        if (additionalImage_ != null) additionalImage_.SetActive(false);
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);
        if (randomButton != null) randomButton.gameObject.SetActive(false);

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
            StartCoroutine(RotateLoadingIndicator()); 
        }

        StartCoroutine(ProcessSubmission());
    }

    IEnumerator ProcessSubmission()
    {
        yield return new WaitForSeconds(7f);

        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);

        if (promptImage != null)
            promptImage.SetActive(true);

        if (confirmButton2 != null)
            confirmButton2.gameObject.SetActive(true);
            confirmButton2.onClick.AddListener(OnConfirmButton2Clicked);
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
        confirmButton2.gameObject.SetActive(false);
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

    IEnumerator ProcessSecondLoading()
    {
        yield return new WaitForSeconds(5f);

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
            isRotating = false; 
        }


        PlayerPrefs.SetString("SavedInput", savedInput);
        PlayerPrefs.Save();

        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
}