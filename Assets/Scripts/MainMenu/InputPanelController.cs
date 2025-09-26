using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

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

    [Header("Параметры анимации")]
    public float appearDuration = 0.5f;      // Длительность появления
    public Vector2 targetScale = Vector2.one; // Конечный масштаб панели

    private CanvasGroup panelCanvasGroup;
    private RectTransform panelRectTransform;
    private CanvasGroup imageCanvasGroup;
    private RectTransform imageRectTransform;

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

        // Убеждаемся, что всё скрыто изначально
        inputPanel.SetActive(false);
        if (additionalImage != null)
            additionalImage.SetActive(false);

        if (confirmButton != null)
            confirmButton.gameObject.SetActive(false);

        // Подписка на события кнопок
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (randomButton != null)
            randomButton.onClick.AddListener(OnRandomButtonClicked);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);

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

        // Сбрасываем начальные состояния
        panelRectTransform.localScale = Vector3.zero;
        panelCanvasGroup.alpha = 0f;

        if (additionalImage != null)
        {
            imageRectTransform.localScale = Vector3.zero;
            imageCanvasGroup.alpha = 0f;
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
    }

    void OnConfirmButtonClicked()
    {
        string userInput = inputField.text;
        Debug.Log("Введено (TMP): " + userInput);

        // ЗДЕСЬ БУДЕТ ОБРАБАТЫВАТЬСЯ ВЫВОД

        // ВРЕМЕННОЕ СКРЫТИЕ =>
        inputField.text = "";
        inputPanel.SetActive(false);
        if (additionalImage != null)
            additionalImage.SetActive(false);

        startButton.gameObject.SetActive(true);
    }
}