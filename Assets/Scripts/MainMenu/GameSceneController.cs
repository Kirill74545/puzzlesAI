using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneDisplayController : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public Text titleTextLegacy;
    public Button backButton;

    void Start()
    {
        string input = GameData.InputMode;

        string displayText = (input == "user image") ? "" : input;

        if (titleText != null)
            titleText.text = displayText;
        else if (titleTextLegacy != null)
            titleTextLegacy.text = displayText;

        Debug.Log("Отображено название: " + displayText);

        if (backButton != null)
            backButton.onClick.AddListener(BackToMainMenu);
    }

    void BackToMainMenu()
    {
        GameData.UserImage = null;
        GameData.InputMode = "";
        GameData.SelectedLevel = "";

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

}