using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelCompleteUI : MonoBehaviour
{
    public TextMeshProUGUI completionTimeText;
    public Button returnToMenuButton;
    public ParticleSystem fireworksEffect;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(ReturnToMainMenu);

        // �������� ��� ������
        gameObject.SetActive(false);
    }

    public void Show(float completionTime)
    {
        // ����������� �����
        int minutes = Mathf.FloorToInt(completionTime / 60);
        int seconds = Mathf.FloorToInt(completionTime % 60);
        completionTimeText.text = $"���� ������ ��\n{minutes:00}:{seconds:00}!";

        // �������� ������
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // ��������� ������� ���������
        StartCoroutine(FadeIn());

        // ���������
        if (fireworksEffect != null)
            fireworksEffect.Play();
    }

    private IEnumerator FadeIn()
    {
        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            canvasGroup.alpha = elapsed / duration;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        if (fireworksEffect != null && fireworksEffect.isPlaying)
            fireworksEffect.Stop();
        gameObject.SetActive(false);
    }

    private void ReturnToMainMenu()
    {
        Hide();
        SceneManager.LoadScene("MainScene");
    }
}