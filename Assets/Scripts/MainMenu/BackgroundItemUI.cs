using UnityEngine;
using UnityEngine.UI;

public class BackgroundItemUI : MonoBehaviour
{
    [Header("Компоненты")]
    public Image previewImage;      // Предпросмотр фона
    public TMPro.TMP_Text requirementsText; // Требования (уровень/монеты)
    public Button actionButton;     // Кнопка действия
    public Image iconImage;         // Иконка внутри кнопки
    public Image checkmarkImage;    // Галочка "текущий фон"

    [Header("Спрайты")]
    public Sprite buyIcon;          // Иконка покупки (монета)
    public Sprite selectIcon;       // Иконка выбора (галочка)
    public Sprite currentIcon;      // Иконка "текущий" (галочка или другая)

    public void Setup(ProgressPopupUI.BackgroundData data, bool isCurrent, bool isPurchased, bool canAfford, System.Action onClick)
    {
        // Установка предпросмотра
        if (previewImage != null && data.previewSprite != null)
            previewImage.sprite = data.previewSprite;

        // Установка требований
        if (requirementsText != null)
        {
            string reqText = "";
            if (data.requiredLevel > 1) reqText += $"{data.requiredLevel} lvl";
            if (data.requiredCoins > 0)
            {
                if (!string.IsNullOrEmpty(reqText)) reqText += ", ";
                reqText += $"{data.requiredCoins} coin";
            }
            requirementsText.text = reqText;
        }

        // Установка галочки "текущий"
        if (checkmarkImage != null)
            checkmarkImage.gameObject.SetActive(isCurrent);

        // Настройка кнопки и иконки
        if (actionButton != null && iconImage != null)
        {
            actionButton.interactable = !isCurrent; // Нельзя нажать, если текущий
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => onClick?.Invoke());

            if (!isPurchased)
            {
                iconImage.sprite = buyIcon;
                // Если не куплен — кнопка интерактивна, если можно купить
                actionButton.interactable = canAfford;
            }
            else
            {
                iconImage.sprite = isCurrent ? currentIcon : selectIcon;
            }
        }
    }
}