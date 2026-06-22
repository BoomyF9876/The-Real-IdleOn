using TMPro;
using UnityEngine;

/// <summary>
/// Live readout of player stats. Subscribes to PlayerStats events so every value
/// updates the moment a talent/shop purchase or level-up changes it.
/// </summary>
public class StatPanelUI : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] HealthBarUI healthBar;

    [Header("Exp Bar")]
    [SerializeField] HealthBarUI expBar;

    [Header("Detail Stats")]
    [SerializeField] TMP_Text level;
    [SerializeField] TMP_Text attackDmg;
    [SerializeField] TMP_Text attackSpeed;
    [SerializeField] TMP_Text moveSpeed;
    [SerializeField] TMP_Text coinDrop;
    [SerializeField] TMP_Text expGain;

    PlayerStats playerStats;

    void Start()
    {
        playerStats = PlayerController.Instance.Stats;

        healthBar.RefreshHealthBar(playerStats.CurrentHealth, playerStats.MaxHealth);
        expBar.RefreshHealthBar(playerStats.CurrentExp, playerStats.MaxExp);
        UpdateLevel(playerStats.Level);
        RefreshDetailStats();

        playerStats.OnHealthChanged += healthBar.RefreshHealthBar;
        playerStats.OnExpChanged += expBar.RefreshHealthBar;
        playerStats.OnLevelChanged += UpdateLevel;
        playerStats.OnStatsRecalculated += RefreshDetailStats; // <-- updates dmg/atk/move/coin/exp on every purchase
    }

    void OnDisable()
    {
        playerStats.OnHealthChanged -= healthBar.RefreshHealthBar;
        playerStats.OnExpChanged -= expBar.RefreshHealthBar;
        playerStats.OnLevelChanged -= UpdateLevel;
        playerStats.OnStatsRecalculated -= RefreshDetailStats;
    }

    private void RefreshDetailStats()
    {
        attackDmg.text = playerStats.AttackDamage.ToString("0.#");
        attackSpeed.text = playerStats.AttacksPerSecond.ToString("0.##");
        moveSpeed.text = playerStats.MoveSpeed.ToString("0.##");
        coinDrop.text = $"+{playerStats.CoinMultiplier * 100f:0.#}%";
        expGain.text = $"+{playerStats.ExpGain * 100f:0.#}%";
    }

    private void UpdateLevel(int newLvl)
    {
        level.text = newLvl.ToString();
    }
}
