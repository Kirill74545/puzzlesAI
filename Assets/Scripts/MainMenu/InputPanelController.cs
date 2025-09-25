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

        // Подписка на события кнопок
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
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
        inputPanel.SetActive(false);
        if (additionalImage != null)
            additionalImage.SetActive(false);

        startButton.gameObject.SetActive(true);
    }
}