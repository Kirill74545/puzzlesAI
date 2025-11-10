using UnityEngine;
using TMPro;
using DG.Tweening;

public class CoinManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text coinText;

    [Header("Настройки")]
    public string prefix = "";
    public float animationDuration = 0.5f; // Длительность анимации

    private const string COINS_KEY = "TotalCoins";
    private int currentCoins;
    private int targetCoins;
    private Tween currentTween;

    void Start()
    {
        if (coinText == null)
        {
            Debug.LogError("CoinManager: TMP_Text не назначен!");
            return;
        }

        currentCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
        targetCoins = currentCoins;
        UpdateDisplay(currentCoins);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        int previousCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
        int newTotal = previousCoins + amount;
        PlayerPrefs.SetInt(COINS_KEY, newTotal);
        PlayerPrefs.Save();

        AnimateCoins(previousCoins, newTotal);
    }

    private void AnimateCoins(int startValue, int endValue)
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }

        targetCoins = endValue;
        currentTween = DOVirtual.Float(startValue, endValue, animationDuration, (value) =>
        {
            UpdateDisplay(Mathf.FloorToInt(value));
        }).OnComplete(() =>
        {
            currentCoins = targetCoins;
            UpdateDisplay(currentCoins);
        });
    }

    public void UpdateDisplay(int value)
    {
        if (coinText != null)
        {
            coinText.text = prefix + value.ToString();
        }
    }
}