using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameSceneDisplayController : MonoBehaviour
{
    public Button backButton;

    void Start()
    {
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