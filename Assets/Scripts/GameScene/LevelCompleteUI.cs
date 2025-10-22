using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelCompleteUI : MonoBehaviour
{
    [Header("������")]
    public TextMeshProUGUI completionTimeText;
    public Button returnToMenuButton;

    [Header("�������")]
    public ParticleSystem fireworksEffect; // ����� �������� null � ����� �� ����� ����������
    public float fadeInDuration = 0.6f;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
        HideImmediately();
    }

    public void Show(float completionTime)
    {
        // ����������� �����
        int minutes = Mathf.FloorToInt(completionTime / 60);
        int seconds = Mathf.FloorToInt(completionTime % 60);
        completionTimeText.text = $"���� ������ ��\n{minutes:00}:{seconds:00}!";

        // ���������� ������ � ���������
        gameObject.SetActive(true);
        StartCoroutine(FadeIn());

        // ��������� ���������
        if (fireworksEffect != null)
        {
            fireworksEffect.Play();
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (elapsed < fadeInDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            elapsed += Time.unscaledDeltaTime; // ���������� Time.timeScale
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void HideImmediately()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void ReturnToMainMenu()
    {
        // ������������� ������� (���� �����)
        if (fireworksEffect != null && fireworksEffect.isPlaying)
        {
            fireworksEffect.Stop();
        }

        // ������� �� MainScene
        SceneManager.LoadScene("MainScene");
    }
}