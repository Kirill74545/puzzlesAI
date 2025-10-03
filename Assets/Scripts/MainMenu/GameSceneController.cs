using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSceneDisplayController : MonoBehaviour
{
    [Header("UI ������� ��� ����������� ��������")]
    public TextMeshProUGUI titleText;       
    public Text titleTextLegacy;            

    [Header("������ ��� ����� �������� �������")]
    public Button backButton;               

    private string savedInput;              

    void Start()
    {
        savedInput = PlayerPrefs.GetString("SavedInput", "�������� �� �������");

        DisplaySavedInput();

        if (backButton != null)
            backButton.onClick.AddListener(BackToMainMenu);
    }

    void DisplaySavedInput()
    {
        if (titleText != null)
        {
            titleText.text = savedInput;
        }
        else if (titleTextLegacy != null)
        {
            titleTextLegacy.text = savedInput;
        }
        else
        {
            Debug.LogWarning("�� TextMeshProUGUI, �� ������� Text �� ������ ��� ����������� ��������!");
        }

        Debug.Log("���������� ��������: " + savedInput);
    }

    public void SetSavedInput(string input)
    {
        savedInput = input;
        PlayerPrefs.SetString("SavedInput", savedInput);
        PlayerPrefs.Save();

        if (titleText != null)
        {
            titleText.text = savedInput;
        }
        else if (titleTextLegacy != null)
        {
            titleTextLegacy.text = savedInput;
        }
    }

    void BackToMainMenu()
    {
        PlayerPrefs.DeleteKey("SavedInput"); 
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); 
    }

    void OnDestroy()
    {
        PlayerPrefs.SetString("SavedInput", savedInput);
        PlayerPrefs.Save();
    }
}