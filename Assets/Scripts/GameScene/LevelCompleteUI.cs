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

    [Header("Coin Reward Animation")]
    public GameObject coinPrefab;               // Префаб монеты 
    public Transform coinTarget;                // Цель: иконка или текст счёта монет
    public int maxCoinsToShow = 10;             // Макс. кол-во монет в анимации
    public float coinFlyDuration = 0.7f;
    public Ease coinFlyEase = Ease.OutCubic;
    public float coinDelayStep = 0.02f;
    public Vector2 coinSpawnOffset = Vector2.zero; // Смещение от центра панели

    [Header("Effects")]
    public ParticleSystem fireworksEffect;

    [Header("Animation Settings")]
    [SerializeField] private float appearDuration = 0.6f;
    [SerializeField] private float textDelay = 0.3f;
    [SerializeField] private Ease panelEase = Ease.OutBack;
    [SerializeField] private Ease textEase = Ease.OutSine;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas cachedCanvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        cachedCanvas = GetComponentInParent<Canvas>();

        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.AddListener(() =>
            {
                AnimateButtonPress(returnToMenuButton);
                AudioManager.Instance?.PlayButtonClick();
                ReturnToMainMenu();
            });
        }

        gameObject.SetActive(false);
    }

    public void Show(float completionTime, int coinsEarned)
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
        if (completionTimeText != null)
            completionTimeText.alpha = 0f;

        // Звук победы
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

        // Анимация текста
        if (completionTimeText != null)
        {
            DOVirtual.DelayedCall(textDelay, () =>
            {
                completionTimeText.DOFade(1f, appearDuration * 0.7f).SetEase(textEase);
            });
        }

        DOVirtual.DelayedCall(0.5f, () => PlayCoinRewardAnimation(coinsEarned));
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

    private void PlayCoinRewardAnimation(int rewardAmount)
    {
        if (coinPrefab == null || coinTarget == null || cachedCanvas == null)
        {
            Debug.LogWarning("Coin animation skipped: missing reference.");
            return;
        }

        // ПолучаемRectTransform панели и цели
        RectTransform panelRect = rectTransform;
        RectTransform targetRect = coinTarget.GetComponent<RectTransform>();
        if (targetRect == null)
        {
            Debug.LogError("Coin target has no RectTransform!");
            return;
        }

        Vector2 panelCenterInCanvas = panelRect.anchoredPosition;
        Vector2 targetPositionInCanvas = targetRect.anchoredPosition;

        int coinsToAnimate = Mathf.Min(rewardAmount, maxCoinsToShow);

        for (int i = 0; i < coinsToAnimate; i++)
        {
            GameObject coinGO = Instantiate(coinPrefab, cachedCanvas.transform);
            coinGO.SetActive(true);

            RectTransform coinRT = coinGO.GetComponent<RectTransform>();
            if (coinRT == null)
            {
                Destroy(coinGO);
                continue;
            }

            coinRT.anchoredPosition = panelCenterInCanvas;

            // Отключаем Raycast
            var image = coinGO.GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = false;
            }

            // Анимация с задержкой
            float delay = i * coinDelayStep;
            DOVirtual.DelayedCall(delay, () =>
            {
                // Летим к цели
                coinRT.DOAnchorPos(targetPositionInCanvas, coinFlyDuration)
                    .SetEase(coinFlyEase)
                    .OnComplete(() =>
                    {
                        if (image != null)
                        {
                            image.DOFade(0f, 0.15f).OnComplete(() => Destroy(coinGO));
                        }
                        else
                        {
                            Destroy(coinGO);
                        }
                    });

                // Вращение
                coinRT.DORotate(Vector3.forward * 360, coinFlyDuration, RotateMode.LocalAxisAdd);
            });
        }

        AudioManager.Instance?.PlayOneShotSFX(AudioManager.Instance.coinCollectSFX);
    }
}