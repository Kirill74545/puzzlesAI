using UnityEngine;
using UnityEngine.UI;

public class ChangeButtonSprite : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites; // Массив спрайтов для переключения
    private Image buttonImage;
    private int currentIndex = 0;

    void Start()
    {
        // Получаем компонент Image кнопки
        buttonImage = GetComponent<Image>();

        // Устанавливаем начальный спрайт
        buttonImage.sprite = sprites[currentIndex];
    }

    // Этот метод вызывается при нажатии кнопки (назначается в инспекторе)
    public void OnButtonClick()
    {
        if (sprites.Length == 0) return;

        // Переходим к следующему спрайту (с зацикливанием)
        currentIndex = (currentIndex + 1) % sprites.Length;
        buttonImage.sprite = sprites[currentIndex];
    }
}