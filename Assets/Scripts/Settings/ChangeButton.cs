using UnityEngine;
using UnityEngine.UI;

public class ChangeButtonSprite : MonoBehaviour
{
    public enum ToggleType
    {
        Music,
        Sound
    }

    [SerializeField] private ToggleType toggleType;
    [SerializeField] private Sprite[] sprites; // Массив спрайтов для переключения
    private Image buttonImage;
    private AudioManager audioManager;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        audioManager = AudioManager.Instance;

        UpdateButtonSprite();
    }

    // Этот метод вызывается при нажатии кнопки (назначается в инспекторе)
    public void OnButtonClick()
    {
        if (audioManager == null) return;

        if (toggleType == ToggleType.Music)
        {
            audioManager.ToggleMusic();
        }
        else
        {
            audioManager.ToggleSound();
        }

        UpdateButtonSprite();
    }

    private void UpdateButtonSprite()
    {
        bool isOn = (toggleType == ToggleType.Music) ? audioManager.musicOn : audioManager.soundOn;
        int index = isOn ? 1 : 0; 

        if (index < sprites.Length)
        {
            buttonImage.sprite = sprites[index];
        }
    }
}