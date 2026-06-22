using TMPro;
using UnityEngine;

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
        UpdateAtkDmg(playerStats.AttackDamage);
        UpdateAtkSpeed(playerStats.AttacksPerSecond);
        UpdateMoveSpeed(playerStats.MoveSpeed);
        UpdateCoinDrop(playerStats.CoinMultiplier);
        UpdateExp(playerStats.ExpGain);

        playerStats.OnHealthChanged += healthBar.RefreshHealthBar;
        playerStats.OnExpChanged += expBar.RefreshHealthBar;
        playerStats.OnLevelChanged += UpdateLevel;
    }

    void OnDisable()
    {
        playerStats.OnHealthChanged -= healthBar.RefreshHealthBar;
        playerStats.OnExpChanged -= expBar.RefreshHealthBar;
        playerStats.OnLevelChanged -= UpdateLevel;
    }

    private void UpdateLevel(int newLvl)
    {
        level.text = newLvl.ToString();
    }

    private void UpdateAtkDmg(float amount)
    {
        attackDmg.text = amount.ToString();
    }

    private void UpdateAtkSpeed(float amount)
    {
        attackSpeed.text = amount.ToString();
    }

    private void UpdateMoveSpeed(float amount)
    {
        moveSpeed.text = amount.ToString();
    }

    private void UpdateCoinDrop(float amount)
    {
        coinDrop.text = amount.ToString();
    }

    private void UpdateExp(float amount)
    {
        expGain.text = amount.ToString();
    }
}
