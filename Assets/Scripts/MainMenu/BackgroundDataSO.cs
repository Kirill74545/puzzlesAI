using UnityEngine;

[CreateAssetMenu(fileName = "Background_", menuName = "Custom/Background Data")]
public class BackgroundDataSO : ScriptableObject
{
    [Header("Основная информация")]
    public int id;                    // Уникальный ID (должен соответствовать индексу в списке GameScene)
    public Sprite previewSprite;      // Спрайт для предпросмотра в магазине
    [Header("Требования")]
    public int requiredLevel;         // Минимальный уровень для покупки
    public int requiredCoins;         // Стоимость в монетах
    [Header("Дополнительно")]
    public string backgroundName;     // Опционально: имя фона
    public string description;        // Опционально: описание
}