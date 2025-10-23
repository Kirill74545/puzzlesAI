using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelCompleteUI : MonoBehaviour
{
    public TextMeshProUGUI completionTimeText;
    public Button returnToMenuButton;
    public ParticleSystem fireworksEffect;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(ReturnToMainMenu);

        // Скрываем при старте
        gameObject.SetActive(false);
    }

    public void Show(float completionTime)
    {
        // Форматируем время
        int minutes = Mathf.FloorToInt(completionTime / 60);
        int seconds = Mathf.FloorToInt(completionTime % 60);
        completionTimeText.text = $"Пазл собран за\n{minutes:00}:{seconds:00}!";

        // Включаем панель
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Запускаем плавное появление
        StartCoroutine(FadeIn());

        // Фейерверк
        if (fireworksEffect != null)
            fireworksEffect.Play();
    }

    private IEnumerator FadeIn()
    {
        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            canvasGroup.alpha = elapsed / duration;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        if (fireworksEffect != null && fireworksEffect.isPlaying)
            fireworksEffect.Stop();
        gameObject.SetActive(false);
    }

    private void ReturnToMainMenu()
    {
        Hide();
        SceneManager.LoadScene("MainScene");
    }
}