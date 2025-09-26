using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

public class InputPanelController : MonoBehaviour
{
    [Header("�������� UI ��������")]
    public Button startButton;               // ��������� ������
    public GameObject inputPanel;            // �������� ������ �����
    public TMP_InputField inputField;        // ���� ����� TextMeshPro
    public Button confirmButton;             // ������ �������������
    public Button randomButton; // ������ ��������� ���������

    [Header("�������������� �����������")]
    public GameObject additionalImage;       

    [Header("��������� ��������")]
    public float appearDuration = 0.5f;      // ������������ ���������
    public Vector2 targetScale = Vector2.one; // �������� ������� ������

    private CanvasGroup panelCanvasGroup;
    private RectTransform panelRectTransform;
    private CanvasGroup imageCanvasGroup;
    private RectTransform imageRectTransform;

    void Start()
    {
        // �������� ���������� ������
        panelRectTransform = inputPanel.GetComponent<RectTransform>();
        panelCanvasGroup = inputPanel.GetComponent<CanvasGroup>()
            ?? inputPanel.AddComponent<CanvasGroup>();

        // �������� ���������� ��������������� ����������� (���� ������)
        if (additionalImage != null)
        {
            imageRectTransform = additionalImage.GetComponent<RectTransform>();
            imageCanvasGroup = additionalImage.GetComponent<CanvasGroup>()
                ?? additionalImage.AddComponent<CanvasGroup>();
        }

        // ����������, ��� �� ������ ����������
        inputPanel.SetActive(false);
        if (additionalImage != null)
            additionalImage.SetActive(false);

        if (confirmButton != null)
            confirmButton.gameObject.SetActive(false);

        // �������� �� ������� ������
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

        // ���������� ������ �������������, ���� ����� �� ������
        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(!string.IsNullOrWhiteSpace(newText));
        }

        // ������ ��������� ��������� �����, ���� ����� ������
        if (randomButton != null)
            randomButton.gameObject.SetActive(isEmpty);
    }

    void OnStartButtonClicked()
    {
        // �������� ��������� ������
        startButton.gameObject.SetActive(false);

        // ���������� ������ � �����������
        inputPanel.SetActive(true);
        if (additionalImage != null)
            additionalImage.SetActive(true);

        // ���������� ��������� ���������
        panelRectTransform.localScale = Vector3.zero;
        panelCanvasGroup.alpha = 0f;

        if (additionalImage != null)
        {
            imageRectTransform.localScale = Vector3.zero;
            imageCanvasGroup.alpha = 0f;
        }

        // ��������� �������� ���������
        StartCoroutine(AppearUI());
    }

    void OnRandomButtonClicked()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("RandomValues");
        if (jsonFile == null)
        {
            Debug.LogError("���� RandomValues.json �� ������ � ����� Resources!");
            return;
        }

        string[] values = null;
        try
        {
            values = JsonHelper.FromJson<string>(jsonFile.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError("������ ��� �������� JSON: " + e.Message);
            return;
        }

        if (values == null || values.Length == 0)
        {
            Debug.LogWarning("JSON �� �������� ��������.");
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

            // �������� ������
            panelRectTransform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            // �������� ��������������� �����������
            if (additionalImage != null)
            {
                imageRectTransform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
                imageCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ��������� ���������
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
        Debug.Log("������� (TMP): " + userInput);

        // ����� ����� �������������� �����

        // ��������� ������� =>
        inputField.text = "";
        inputPanel.SetActive(false);
        if (additionalImage != null)
            additionalImage.SetActive(false);

        startButton.gameObject.SetActive(true);
    }
}