using UnityEngine;
using UnityEngine.UI;

public class ChangeButtonSprite : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites; // ������ �������� ��� ������������
    private Image buttonImage;
    private int currentIndex = 0;

    void Start()
    {
        // �������� ��������� Image ������
        buttonImage = GetComponent<Image>();

        // ������������� ��������� ������
        buttonImage.sprite = sprites[currentIndex];
    }

    // ���� ����� ���������� ��� ������� ������ (����������� � ����������)
    public void OnButtonClick()
    {
        if (sprites.Length == 0) return;

        // ��������� � ���������� ������� (� �������������)
        currentIndex = (currentIndex + 1) % sprites.Length;
        buttonImage.sprite = sprites[currentIndex];
    }
}