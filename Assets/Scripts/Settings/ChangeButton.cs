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
    [SerializeField] private Sprite[] sprites;

    private Image buttonImage;
    private AudioManager audioManager;

    void Start()
    {
        buttonImage = GetComponent<Image>();

        // Проверяем, доступен ли Instance
        if (AudioManager.Instance != null)
        {
            audioManager = AudioManager.Instance;
            UpdateButtonSprite();
        }
    }

    public void OnButtonClick()
    {
        if (audioManager == null)
        {
            Debug.LogWarning("AudioManager недоступен!");
            return;
        }

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
        if (audioManager == null) return;

        bool isOn = (toggleType == ToggleType.Music) ? audioManager.musicOn : audioManager.soundOn;
        int index = isOn ? 1 : 0;

        if (index < sprites.Length)
        {
            buttonImage.sprite = sprites[index];
        }
    }
}