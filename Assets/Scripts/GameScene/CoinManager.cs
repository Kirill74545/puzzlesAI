using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text coinText; 

    [Header("Настройки")]
    public string prefix = "";

    private const string COINS_KEY = "TotalCoins";

    void Start()
    {
        if (coinText == null)
        {
            Debug.LogError("CoinManager: TMP_Text не назначен!");
            return;
        }

        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        int coins = PlayerPrefs.GetInt(COINS_KEY, 0);
        coinText.text = prefix + coins.ToString();
    }
}