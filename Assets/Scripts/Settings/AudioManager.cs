using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Ссылки")]
    public AudioSource musicSource;

    [Header("Настройки")]
    public bool musicOn = true;
    public bool soundOn = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }

        if (musicSource == null)
        {
            musicSource = Object.FindFirstObjectByType<AudioSource>();
        }

        ApplyMusicVolume();
    }

    public void ToggleMusic()
    {
        musicOn = !musicOn;
        SaveSettings();
        ApplyMusicVolume();
    }

    public void ToggleSound()
    {
        soundOn = !soundOn;
        SaveSettings();
        ApplySoundVolume();
    }

    public void ApplyMusicVolume()
    {
        if (musicSource != null)
        {
            musicSource.mute = !musicOn; 
        }
    }

    public void ApplySoundVolume()
    {
        
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("MusicOn", musicOn ? 1 : 0);
        PlayerPrefs.SetInt("SoundOn", soundOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        musicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        soundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        ApplyMusicVolume();
        ApplySoundVolume();
    }
}