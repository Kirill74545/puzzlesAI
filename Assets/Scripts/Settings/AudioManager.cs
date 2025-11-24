using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Ссылки")]
    public AudioSource musicSource;
    public AudioClip buttonClickSFX;      // звук обычной кнопки
    public AudioClip toggleSwitchSFX;     // звук переключателя 
    public AudioClip puzzlePickupSFX;     // звук взятия пазла
    public AudioClip puzzleCorrectSFX;    // звук правильной установки
    public AudioClip puzzleReturnSFX;     // звук возврата в список
    public AudioClip levelCompleteSFX;    // звук победы
    public AudioClip coinCollectSFX;      // звук сбора монет

    [Header("Музыка")]
    public AudioClip defaultMusic; // Трек по умолчанию
    public AudioClip specialMusic; // Трек для фона ID 14

    [Header("Звуки магазина фонов")]
    public AudioClip backgroundPurchaseSFX; // Звук покупки фона
    public AudioClip backgroundSelectSFX;   // Звук выбора фона
    public AudioClip backgroundErrorSFX;    // Звук ошибки (не хватает монет/уровня)

    [Header("Звуки подсказок")]
    public AudioClip hintPurchaseSuccessSFX; // Успешная покупка подсказки
    public AudioClip hintPurchaseFailSFX;    // Недостаточно монет

    [Header("Настройки")]
    public bool musicOn = true;
    public bool soundOn = true;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private AudioSource sfxSource;

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
            return;
        }

        // Настройка музыки
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }

        if (defaultMusic != null)
        {
            musicSource.clip = defaultMusic;
            if (musicOn)
            {
                musicSource.Play();
            }
        }

        // Создаём отдельный источник для SFX (чтобы не мешать музыке)
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;

        ApplyMusicVolume();
        ApplySoundVolume();
    }

    public void PlayOneShotSFX(AudioClip clip)
    {
        if (!soundOn || clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayButtonClick()
    {
        if (!soundOn || buttonClickSFX == null) return;
        sfxSource.PlayOneShot(buttonClickSFX, sfxVolume);
    }

    public void PlayToggleSound()
    {
        if (!soundOn) return;
        if (toggleSwitchSFX != null)
            sfxSource.PlayOneShot(toggleSwitchSFX, sfxVolume);
        else
            PlayButtonClick(); 
    }

    public void PlayBackgroundPurchase()
    {
        if (!soundOn || backgroundPurchaseSFX == null) return;
        sfxSource.PlayOneShot(backgroundPurchaseSFX, sfxVolume);
    }

    public void PlayBackgroundSelect()
    {
        if (!soundOn || backgroundSelectSFX == null) return;
        sfxSource.PlayOneShot(backgroundSelectSFX, sfxVolume);
    }

    public void PlayBackgroundError()
    {
        if (!soundOn || backgroundErrorSFX == null) return;
        sfxSource.PlayOneShot(backgroundErrorSFX, sfxVolume);
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
        if (sfxSource != null)
        {
            sfxSource.mute = !soundOn;
        }
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
    }

    public void SetSpecialMusic()
    {
        if (specialMusic != null && musicSource.clip != specialMusic)
        {
            musicSource.clip = specialMusic;
            if (musicOn && !musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
    }

    public void SetDefaultMusic()
    {
        if (defaultMusic != null && musicSource.clip != defaultMusic)
        {
            musicSource.clip = defaultMusic;
            if (musicOn && !musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
    }
}