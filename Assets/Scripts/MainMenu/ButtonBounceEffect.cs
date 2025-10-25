// ButtonBounceEffect.cs
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonBounceEffect : MonoBehaviour
{
    [Header("Анимация")]
    [SerializeField] private float animationDuration = 0.25f;
    [SerializeField] private Ease easeIn = Ease.InBack;
    [SerializeField] private Ease easeOut = Ease.OutBack;
    [SerializeField] private float scaleDown = 0.8f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private bool isAnimating = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonPressed);
        }
    }

    private void OnButtonPressed()
    {
        // Сначала проигрываем звук
        AudioManager.Instance?.PlayButtonClick();

        // Затем запускаем анимацию (если не анимируется)
        if (!isAnimating)
        {
            StartCoroutine(AnimateBounce());
        }
    }

    private System.Collections.IEnumerator AnimateBounce()
    {
        isAnimating = true;

        // 1. Сжатие
        rectTransform.DOScale(scaleDown, animationDuration * 0.6f).SetEase(easeIn);
        canvasGroup.DOFade(0.6f, animationDuration * 0.6f).SetEase(Ease.Linear);

        yield return new WaitForSeconds(animationDuration * 0.6f);

        // 2. Раскрытие с отскоком
        rectTransform.DOScale(1f, animationDuration * 0.8f).SetEase(easeOut);
        canvasGroup.DOFade(1f, animationDuration * 0.8f).SetEase(Ease.Linear);

        yield return new WaitForSeconds(animationDuration * 0.8f);

        isAnimating = false;
    }
}