using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The talent (stat upgrade) panel. Each of the six buttons buys one level of a stat.
/// Cost rises per purchase: cost = baseCost * costMultiplier ^ (levels already bought).
/// Buttons grey out when unaffordable and update live as coins change.
///
/// Effects live in PlayerStats (growth-per-level); this panel only handles pricing,
/// charging the player, and refreshing labels.
/// </summary>
public class TalentPanelUI : MonoBehaviour
{
    [Header("Open / Close")]
    [SerializeField] Button talentBtn;
    [SerializeField] Button backBtn;
    [SerializeField] GameObject talentPanel;
    [Tooltip("Red exclamation badge shown only when at least one talent is affordable.")]
    [SerializeField] Image availHint;

    [Header("Cost Curve")]
    [SerializeField] int baseCost = 5;
    [Tooltip("Cost growth per purchase. 1.15 => each level costs 15% more than the last.")]
    [SerializeField] float costMultiplier = 1.15f;

    [Header("Upgrade Buttons")]
    [SerializeField] Button upgradeHealthBtn;
    [SerializeField] Button upgradeDamageBtn;
    [SerializeField] Button upgradeAtkSpeedBtn;
    [SerializeField] Button upgradeCoinDropBtn;
    [SerializeField] Button upgradeExpGainBtn;
    [SerializeField] Button upgradeMoveSpeedBtn;

    PlayerStats stats;
    readonly Dictionary<TalentType, Button> buttons = new Dictionary<TalentType, Button>();

    void Start()
    {
        HidePanel();
        talentBtn.onClick.AddListener(ShowPanel);
        backBtn.onClick.AddListener(HidePanel);

        stats = PlayerController.Instance.Stats;

        // Map each talent to its button so we can price/refresh them in one loop.
        buttons[TalentType.Health]      = upgradeHealthBtn;
        buttons[TalentType.Damage]      = upgradeDamageBtn;
        buttons[TalentType.AttackSpeed] = upgradeAtkSpeedBtn;
        buttons[TalentType.CoinDrop]    = upgradeCoinDropBtn;
        buttons[TalentType.ExpGain]     = upgradeExpGainBtn;
        buttons[TalentType.MoveSpeed]   = upgradeMoveSpeedBtn;

        foreach (var pair in buttons)
        {
            TalentType type = pair.Key; // capture per-iteration to avoid the closure bug
            pair.Value.onClick.AddListener(() => TryPurchase(type));
        }

        // Keep affordability + labels current as coins change and after any stat recalc.
        CoinManager.Instance.OnCoinsChanged += OnCoinsChanged;
        stats.OnStatsRecalculated += RefreshAll;

        RefreshAll();
    }

    void OnDisable()
    {
        if (CoinManager.Instance != null) CoinManager.Instance.OnCoinsChanged -= OnCoinsChanged;
        if (stats != null) stats.OnStatsRecalculated -= RefreshAll;
    }

    void OnCoinsChanged(int _) => RefreshAll();

    int GetCost(TalentType type)
    {
        int level = stats.GetTalentLevel(type);
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, level));
    }

    void TryPurchase(TalentType type)
    {
        int cost = GetCost(type);
        if (!CoinManager.Instance.TrySpendCoins(cost)) return; // not enough coins

        stats.PurchaseTalent(type); // applies effect + triggers OnStatsRecalculated -> RefreshAll
    }

    void RefreshAll()
    {
        int coins = CoinManager.Instance != null ? CoinManager.Instance.CurrentCoins : 0;
        bool anyAffordable = false;

        foreach (var pair in buttons)
        {
            int cost = GetCost(pair.Key);
            Button btn = pair.Value;

            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = cost.ToString();

            bool canAfford = coins >= cost;
            btn.interactable = canAfford; // grey out when unaffordable
            if (canAfford) anyAffordable = true;
        }

        // Show the red exclamation badge only when something is buyable.
        if (availHint != null) availHint.gameObject.SetActive(anyAffordable);
    }

    void ShowPanel() => talentPanel.SetActive(true);
    void HidePanel() => talentPanel.SetActive(false);
}
