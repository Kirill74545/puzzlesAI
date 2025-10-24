using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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
        {
            // Подписываемся на клик с анимацией и звуком
            backButton.onClick.AddListener(() =>
            {
                AnimateButtonPress(backButton);
                PlayButtonClickSound();
                BackToMainMenu();
            });
        }
    }

    void BackToMainMenu()
    {
        GameData.UserImage = null;
        GameData.InputMode = "";
        GameData.SelectedLevel = "";

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private void PlayButtonClickSound()
    {
        AudioManager.Instance?.PlayButtonClick();
    }

    private void AnimateButtonPress(Button button)
    {
        if (button == null) return;
        var rect = button.GetComponent<RectTransform>();
        rect.DOScale(0.9f, 0.1f)
            .OnComplete(() => rect.DOScale(1f, 0.1f).SetEase(Ease.OutBack));
    }
}