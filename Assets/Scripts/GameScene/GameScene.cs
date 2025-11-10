using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class GameScene : MonoBehaviour
{
    [Header("Фон сцены")]
    public Image mainBackground; // Ссылка на Image фона
    public List<Sprite> backgroundSprites = new List<Sprite>(); // Список всех спрайтов

    [Header("DOTween Анимации")]
    public float fadeInDuration = 0.5f;
    public Ease fadeInEase = Ease.InOutQuad;

    private void Start()
    {
        int savedId = PlayerPrefs.GetInt("CurrentBackgroundId", 0);
        SetBackground(savedId);
    }

    public void SetBackground(int backgroundId)
    {
        if (backgroundSprites == null || backgroundId < 0 || backgroundId >= backgroundSprites.Count)
        {
            Debug.LogWarning($"Фон с ID {backgroundId} не найден в списке!");
            return;
        }

        if (mainBackground != null)
        {
            // Сначала скрываем текущий фон
            mainBackground.DOFade(0f, fadeInDuration / 2f).OnComplete(() =>
            {
                // Меняем спрайт
                mainBackground.sprite = backgroundSprites[backgroundId];
                // Плавно показываем
                mainBackground.DOFade(1f, fadeInDuration / 2f).SetEase(fadeInEase);
            });
        }

        PlayerPrefs.SetInt("CurrentBackgroundId", backgroundId);
        PlayerPrefs.Save();
    }
}