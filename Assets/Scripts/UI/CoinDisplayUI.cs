using TMPro;
using UnityEngine;

public class CoinDisplayUI : MonoBehaviour
{
    [SerializeField] TMP_Text coinCount;

    void Start()
    {
        coinCount.text = "0";
        CoinManager.Instance.OnCoinsChanged += UpdateUI;
    }

    void OnDisable()
    {
        CoinManager.Instance.OnCoinsChanged -= UpdateUI;
    }

    void UpdateUI(int coin)
    {
        coinCount.text = coin.ToString();
    }
}
