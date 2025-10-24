using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("������")]
    public AudioSource musicSource;
    public AudioClip buttonClickSFX;      // ���� ������� ������
    public AudioClip toggleSwitchSFX;     // ���� ������������� 
    public AudioClip puzzlePickupSFX;   // ���� ������ �����
    public AudioClip puzzleCorrectSFX;  // ���� ���������� ���������
    public AudioClip puzzleReturnSFX;   // ���� �������� � ������
    public AudioClip levelCompleteSFX; // ���� ������

    [Header("���������")]
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

        // ��������� ������
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }

        // ������ ��������� �������� ��� SFX (����� �� ������ ������)
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
            PlayButtonClick(); // fallback
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
}