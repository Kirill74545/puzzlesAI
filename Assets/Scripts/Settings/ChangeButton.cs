using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ChangeButtonSprite : MonoBehaviour
{
    public enum ToggleType
    {
        Music,
        Sound
    }

    [SerializeField] private ToggleType toggleType;
    [SerializeField] private Sprite[] sprites; // [0] = выкл, [1] = вкл

    [Header("Анимация")]
    [SerializeField] private float animationDuration = 0.25f;
    [SerializeField] private Ease easeIn = Ease.InBack;
    [SerializeField] private Ease easeOut = Ease.OutBack;

    private Image buttonImage;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private AudioManager audioManager;
    private bool isAnimating = false;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        if (AudioManager.Instance != null)
        {
            audioManager = AudioManager.Instance;
            UpdateButtonSprite();
        }
    }

    public void OnButtonClick()
    {
        if (isAnimating || audioManager == null)
        {
            if (audioManager == null)
                Debug.LogWarning("AudioManager недоступен!");
            return;
        }

        // Выполняем переключение звука/музыки
        if (toggleType == ToggleType.Music)
        {
            audioManager.ToggleMusic();
        }
        else
        {
            audioManager.ToggleSound();
        }
        AudioManager.Instance?.PlayToggleSound();
        // Запускаем анимацию
        StartCoroutine(AnimateToggle());
    }

    private System.Collections.IEnumerator AnimateToggle()
    {
        isAnimating = true;

        // 1. Анимация "схлопывания"
        rectTransform.DOScale(0.8f, animationDuration * 0.6f).SetEase(easeIn);
        canvasGroup.DOFade(0.6f, animationDuration * 0.6f).SetEase(Ease.Linear);

        yield return new WaitForSeconds(animationDuration * 0.6f);

        // 2. Меняем спрайт мгновенно (в "нижней точке" анимации)
        UpdateButtonSprite();

        // 3. Анимация "раскрытия"
        rectTransform.DOScale(1f, animationDuration * 0.8f).SetEase(easeOut);
        canvasGroup.DOFade(1f, animationDuration * 0.8f).SetEase(Ease.Linear);

        yield return new WaitForSeconds(animationDuration * 0.8f);

        isAnimating = false;
    }

    private void UpdateButtonSprite()
    {
        if (audioManager == null) return;

        bool isOn = (toggleType == ToggleType.Music) ? audioManager.musicOn : audioManager.soundOn;
        int index = isOn ? 1 : 0;

        if (index < sprites.Length && sprites[index] != null)
        {
            buttonImage.sprite = sprites[index];
        }
    }
}