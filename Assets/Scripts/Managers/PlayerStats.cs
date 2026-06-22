using System;
using UnityEngine;

/// <summary>
/// The six things coins can be spent on. Shared by PlayerStats and TalentPanelUI
/// so the panel and the stat logic agree on what each upgrade row means.
/// </summary>
public enum TalentType
{
    Health,
    Damage,
    AttackSpeed,
    CoinDrop,
    ExpGain,
    MoveSpeed
}

/// <summary>
/// Single source of truth for "how strong is the player right now".
///
/// Stats are COMPOSED, not mutated in place:
///   current = (base + level bonuses + shop bonuses) [+ or x] upgrade growth
/// RecalculateStats() rebuilds every current value from those inputs, so it is
/// idempotent and safe to call after any purchase or level-up without wiping
/// previous upgrades. Nothing outside this class should write to the Current* values.
///
/// Health and Damage scale MULTIPLICATIVELY per upgrade level (to keep pace with the
/// exponential enemy curve). Attack speed / move speed add flat per level. Coin and
/// exp upgrades add a fractional bonus (0.10 = +10%).
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats (starting values)")]
    [SerializeField] private float baseMaxExp = 2f;
    [SerializeField] private float baseAttackRange = 1f;
    [SerializeField] private float baseLvlUpHealthGain = 5f;
    [SerializeField] private float baseMaxHealth = 50f;
    [SerializeField] private float baseMoveSpeed = 2.5f;
    [SerializeField] private float baseAttackDamage = 5f;
    [SerializeField] private float baseAttacksPerSecond = 1f;

    [Header("Talent Growth Per Level (tuned against the enemy curve)")]
    [Tooltip("Health multiplier per level. 0.08 => MaxHealth x1.08 per Health level (matches enemy DMG x1.08/tier).")]
    [SerializeField] private float healthGrowthPerLevel = 0.08f;
    [Tooltip("Damage multiplier per level. 0.10 => AttackDamage x1.10 per Damage level (matches enemy HP x1.10/tier).")]
    [SerializeField] private float damageGrowthPerLevel = 0.10f;
    [Tooltip("Flat attacks/sec added per Attack Speed level.")]
    [SerializeField] private float attackSpeedPerLevel = 0.05f;
    [Tooltip("Flat move speed added per Move Speed level.")]
    [SerializeField] private float moveSpeedPerLevel = 0.15f;
    [Tooltip("Fractional coin bonus per Coin level. 0.10 => +10% coins per level.")]
    [SerializeField] private float coinBonusPerLevel = 0.10f;
    [Tooltip("Fractional exp bonus per Exp level. 0.10 => +10% exp per level.")]
    [SerializeField] private float expBonusPerLevel = 0.10f;

    [Header("Sustain")]
    [Tooltip("Fraction of MAX health healed on each kill. 0.04 => heal 4% of max HP per kill. Without sustain the player bleeds out; this is the main survivability knob.")]
    [SerializeField] private float healOnKillPercent = 0.04f;

    // ---- Talent levels (single source of truth; TalentPanelUI reads these to price upgrades) ----
    private int healthLevel;
    private int damageLevel;
    private int attackSpeedLevel;
    private int coinLevel;
    private int expLevel;
    private int moveSpeedLevel;

    // ---- Accumulated flat bonuses from one-time shop items ----
    private float shopHealth;
    private float shopDamage;
    private float shopAttackSpeed;
    private float shopMoveSpeed;
    private float shopCoinBonus; // fractional
    private float shopExpBonus;  // fractional

    // ---- Composed current values (read-only to the outside) ----
    public int Level { get; private set; }
    public float MaxExp { get; private set; }
    public float MaxHealth { get; private set; }
    public float CurrentExp { get; private set; }
    public float CurrentHealth { get; private set; }
    public float MoveSpeed { get; private set; }
    public float AttackDamage { get; private set; }
    public float AttackRange { get; private set; }
    public float AttacksPerSecond { get; private set; }
    public float CoinMultiplier { get; private set; } // fractional bonus, e.g. 0.30 = +30% coins
    public float ExpGain { get; private set; }         // fractional bonus, e.g. 0.20 = +20% exp
    public float HealthGain { get; private set; }      // health granted per level (for display)

    public bool IsDead => CurrentHealth <= 0f;

    // UI subscribes to these instead of polling.
    public event Action<int> OnLevelChanged;
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action<float, float> OnExpChanged;    // current, max
    public event Action OnPlayerDied;
    public event Action OnStatsRecalculated;

    private void Awake()
    {
        Level = 1;
        CurrentExp = 0f;
        RecalculateStats();
        CurrentHealth = MaxHealth; // start at full
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    /// <summary>
    /// Rebuilds every Current* value from base + levels + shop bonuses.
    /// Idempotent: calling it repeatedly with the same inputs gives the same result,
    /// so it can run after any purchase or level-up without compounding or wiping.
    /// </summary>
    public void RecalculateStats()
    {
        float oldMaxHealth = MaxHealth;

        // Health & damage scale multiplicatively to track the exponential enemy curve.
        float healthPool = baseMaxHealth + (Level - 1) * baseLvlUpHealthGain + shopHealth;
        MaxHealth = healthPool * Mathf.Pow(1f + healthGrowthPerLevel, healthLevel);

        AttackDamage = (baseAttackDamage + shopDamage) * Mathf.Pow(1f + damageGrowthPerLevel, damageLevel);

        // Secondary stats add flat.
        AttacksPerSecond = baseAttacksPerSecond + attackSpeedPerLevel * attackSpeedLevel + shopAttackSpeed;
        MoveSpeed = baseMoveSpeed + moveSpeedPerLevel * moveSpeedLevel + shopMoveSpeed;
        AttackRange = baseAttackRange;

        // Economy stats are fractional bonuses.
        CoinMultiplier = coinBonusPerLevel * coinLevel + shopCoinBonus;
        ExpGain = expBonusPerLevel * expLevel + shopExpBonus;

        MaxExp = Mathf.Max(MaxExp, baseMaxExp); // MaxExp grows via leveling; never below base
        HealthGain = baseLvlUpHealthGain;

        // When max health grows (health upgrade or level-up), grant the increase to current
        // health so investing in health also heals, but don't fully refill on unrelated upgrades.
        if (MaxHealth > oldMaxHealth)
        {
            CurrentHealth += (MaxHealth - oldMaxHealth);
        }
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);

        OnStatsRecalculated?.Invoke();
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    // ----------------------------------------------------------------------
    // Talent purchases
    // ----------------------------------------------------------------------

    /// <summary>Current purchased level of a talent. TalentPanelUI uses this to compute the next cost.</summary>
    public int GetTalentLevel(TalentType type)
    {
        switch (type)
        {
            case TalentType.Health:      return healthLevel;
            case TalentType.Damage:      return damageLevel;
            case TalentType.AttackSpeed: return attackSpeedLevel;
            case TalentType.CoinDrop:    return coinLevel;
            case TalentType.ExpGain:     return expLevel;
            case TalentType.MoveSpeed:   return moveSpeedLevel;
            default:                     return 0;
        }
    }

    /// <summary>
    /// Applies one level of the given talent and recomputes stats. Does NOT touch coins -
    /// the caller (TalentPanelUI) is responsible for charging the player first.
    /// </summary>
    public void PurchaseTalent(TalentType type)
    {
        switch (type)
        {
            case TalentType.Health:      healthLevel++;      break;
            case TalentType.Damage:      damageLevel++;      break;
            case TalentType.AttackSpeed: attackSpeedLevel++; break;
            case TalentType.CoinDrop:    coinLevel++;        break;
            case TalentType.ExpGain:     expLevel++;         break;
            case TalentType.MoveSpeed:   moveSpeedLevel++;   break;
        }
        RecalculateStats();
    }

    // ----------------------------------------------------------------------
    // Shop (one-time) purchases
    // ----------------------------------------------------------------------

    /// <summary>
    /// Adds a one-time shop item's flat bonuses to the accumulators and recomputes.
    /// Caller (ShopPanelUI) charges the player and prevents re-buying.
    /// coinBoost / experienceBoost are treated as FRACTIONS (0.25 = +25%).
    /// </summary>
    public void ApplyShopItem(ShopItem item)
    {
        shopHealth      += item.healthBoost;
        shopDamage      += item.damageBoost;
        shopAttackSpeed += item.atkSpeedBoost;
        shopMoveSpeed   += item.moveSpeedBoost;
        shopCoinBonus   += item.coinBoost;
        shopExpBonus    += item.experienceBoost;
        RecalculateStats();
    }

    // ----------------------------------------------------------------------
    // Combat / progression
    // ----------------------------------------------------------------------

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0f)
        {
            OnPlayerDied?.Invoke();
        }
    }

    /// <summary>Heal a flat amount of max HP on each kill. Called by PlayerController.</summary>
    public void ApplyHealOnKill()
    {
        if (IsDead) return;
        //Heal(MaxHealth * healOnKillPercent);
    }

    public void GainExperience(float amount)
    {
        CurrentExp += amount;
        int oldLvl = Level;

        while (CurrentExp >= MaxExp)
        {
            CurrentExp -= MaxExp;
            MaxExp += Level;
            Level++;
        }

        if (Level > oldLvl)
        {
            // Recompute so the new Level feeds MaxHealth through the composition
            // (this also heals by the max-health increase via RecalculateStats).
            RecalculateStats();
            OnLevelChanged?.Invoke(Level);
        }

        OnExpChanged?.Invoke(CurrentExp, MaxExp);
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void FullHeal()
    {
        CurrentHealth = MaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    /// <summary>Seconds between attacks, derived from AttacksPerSecond.</summary>
    public float AttackInterval => AttacksPerSecond > 0f ? 1f / AttacksPerSecond : float.MaxValue;
}
