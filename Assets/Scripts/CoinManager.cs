using System;
using UnityEngine;

/// <summary>
/// Single source of truth for the player's coin total.
/// Use CoinManager.Instance from anywhere (spawner death callbacks, upgrade UI, save system).
/// </summary>
public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    public int CurrentCoins { get; private set; }

    public event Action<int> OnCoinsChanged; // new total

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        CurrentCoins += amount;
        OnCoinsChanged?.Invoke(CurrentCoins);
    }

    /// <summary>
    /// Returns true and deducts coins if affordable. Used by the future upgrade UI.
    /// </summary>
    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0 || CurrentCoins < amount) return false;
        CurrentCoins -= amount;
        OnCoinsChanged?.Invoke(CurrentCoins);
        return true;
    }
}
