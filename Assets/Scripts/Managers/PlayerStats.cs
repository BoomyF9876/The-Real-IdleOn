using System;
using UnityEngine;

/// <summary>
/// Holds all player stats that the future upgrade system will modify.
/// Keep this as the single source of truth for "how strong is the player right now".
/// Other scripts (PlayerController, UpgradeManager, SaveSystem) should read/write through here,
/// not store their own copies of these numbers.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats (Inspector defaults / starting values)")]
    [SerializeField] private float baseMaxExp = 2f;
    [SerializeField] private float baseAttackRange = 1f;
    [SerializeField] private float baseLvlUpHealthGain = 5f;
    [SerializeField] private float baseMaxHealth = 50f;
    [SerializeField] private float baseMoveSpeed = 2.5f;
    [SerializeField] private float baseAttackDamage = 5f;
    [SerializeField] private float baseAttacksPerSecond = 1f;

    // Current values, separate from base so upgrades can add/multiply onto base later.
    public int Level { get; private set; }
    public float MaxExp { get; private set; }
    public float MaxHealth { get; private set; }
    public float CurrentExp { get; private set; }
    public float CurrentHealth { get; private set; }
    public float MoveSpeed { get; private set; }
    public float AttackDamage { get; private set; }
    public float AttackRange { get; private set; }
    public float AttacksPerSecond { get; private set; }
    public float CoinMultiplier { get; private set; }
    public float ExpGain { get; private set; }
    public float HealthGain { get; private set; }

    public bool IsDead => CurrentHealth <= 0f;

    // UI / other systems subscribe to these instead of polling every frame.
    public event Action<int> OnLevelChanged; // current, max
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action<float, float> OnExpChanged; // current, max
    public event Action OnPlayerDied;
    public event Action OnStatsRecalculated;

    private void Awake()
    {
        CurrentHealth = MaxHealth;
        CoinMultiplier = 0;
        CurrentExp = 0;
        ExpGain = 0;
        Level = 1;
        RecalculateStats();
    }

    /// <summary>
    /// Recomputes Current-tier stats from base values.
    /// Call this after any upgrade purchase. For now it just copies base -> current;
    /// later, plug upgrade levels in here (e.g. MaxHealth = baseMaxHealth + healthLevel * healthPerLevel).
    /// </summary>
    public void RecalculateStats()
    {
        float oldMaxHealth = MaxHealth;
        MaxHealth = baseMaxHealth;
        MaxExp = baseMaxExp;
        MoveSpeed = baseMoveSpeed;
        AttackDamage = baseAttackDamage;
        AttackRange = baseAttackRange;
        AttacksPerSecond = baseAttacksPerSecond;
        HealthGain = baseLvlUpHealthGain;

        // If max health increased (e.g. from an upgrade), grant the difference to current health
        // rather than fully healing, so upgrading isn't also a free full heal mid-fight.
        if (MaxHealth > oldMaxHealth)
        {
            CurrentHealth += (MaxHealth - oldMaxHealth);
        }
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);

        OnStatsRecalculated?.Invoke();
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

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

    public void GainExperience(float amount)
    {
        CurrentExp += amount;
        int oldLvl = Level;

        while (CurrentExp > MaxExp)
        {
            CurrentExp -= MaxExp;
            MaxExp += Level;
            MaxHealth += HealthGain;
            Level++;
        }

        OnExpChanged?.Invoke(CurrentExp, MaxExp);
        if (Level > oldLvl)
        {
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            OnLevelChanged?.Invoke(Level);
        }
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

    /// <summary>
    /// Seconds between attacks, derived from AttacksPerSecond. Used by PlayerController's attack timer.
    /// </summary>
    public float AttackInterval => AttacksPerSecond > 0f ? 1f / AttacksPerSecond : float.MaxValue;
}
