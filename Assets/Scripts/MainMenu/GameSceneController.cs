using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSceneDisplayController : MonoBehaviour
{
    [Header("UI элемент для отображения названия")]
    public TextMeshProUGUI titleText;       
    public Text titleTextLegacy;            

    [Header("Кнопка для теста перехода обратно")]
    public Button backButton;               

    private string savedInput;              

    void Start()
    {
        savedInput = PlayerPrefs.GetString("SavedInput", "Название не найдено");

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
            Debug.LogWarning("Ни TextMeshProUGUI, ни обычный Text не заданы для отображения названия!");
        }

        Debug.Log("Отображено название: " + savedInput);
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