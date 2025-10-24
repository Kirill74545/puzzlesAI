using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class LevelCompleteUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI completionTimeText;
    public Button returnToMenuButton;

    [Header("Effects")]
    public ParticleSystem fireworksEffect;

    [Header("Animation Settings")]
    [SerializeField] private float appearDuration = 0.6f;
    [SerializeField] private float textDelay = 0.3f;
    [SerializeField] private Ease panelEase = Ease.OutBack;
    [SerializeField] private Ease textEase = Ease.OutSine;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.AddListener(() =>
            {
                // Анимация нажатия кнопки
                AnimateButtonPress(returnToMenuButton);
                // Звук
                AudioManager.Instance?.PlayButtonClick();
                // Возврат
                ReturnToMainMenu();
            });
        }

        gameObject.SetActive(false);
    }

    public void Show(float completionTime)
    {
        // Форматируем время
        int minutes = Mathf.FloorToInt(completionTime / 60);
        int seconds = Mathf.FloorToInt(completionTime % 60);
        string timeStr = $"{minutes:00}:{seconds:00}";
        completionTimeText.text = $"Пазл собран за\n{timeStr}!";

        // Включаем панель
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Сбрасываем трансформ
        rectTransform.localScale = Vector3.zero;
        completionTimeText.alpha = 0f;

        AudioManager.Instance?.PlayOneShotSFX(AudioManager.Instance.levelCompleteSFX);

        // Фейерверк 
        if (fireworksEffect != null)
        {
            fireworksEffect.Stop();
            DOVirtual.DelayedCall(0.4f, () => fireworksEffect.Play());
        }

        // Анимация появления панели
        rectTransform.DOScale(1f, appearDuration).SetEase(panelEase);
        canvasGroup.DOFade(1f, appearDuration).OnComplete(() =>
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        });

        // Анимация текста с задержкой
        DOVirtual.DelayedCall(textDelay, () =>
        {
            completionTimeText.DOFade(1f, appearDuration * 0.7f).SetEase(textEase);
        });
    }

    public void Hide()
    {
        if (fireworksEffect != null && fireworksEffect.isPlaying)
            fireworksEffect.Stop();

        DOTween.Kill(gameObject);

        gameObject.SetActive(false);
    }

    private void ReturnToMainMenu()
    {
        Hide();
        SceneManager.LoadScene("MainScene");
    }

    private void AnimateButtonPress(Button button)
    {
        if (button == null) return;
        var rect = button.GetComponent<RectTransform>();
        rect.DOScale(0.9f, 0.1f)
            .OnComplete(() => rect.DOScale(1f, 0.1f).SetEase(Ease.OutBack));
    }
}