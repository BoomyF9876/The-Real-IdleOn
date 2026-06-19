using System;
using UnityEngine;

/// <summary>
/// A single enemy instance. Health and damage are set by the spawner at spawn time
/// (so the spawner controls difficulty scaling, not the enemy itself).
/// </summary>
public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Drop Settings")]
    [SerializeField] private int baseCoinDrop = 1;

    [Header("Runtime (set by spawner, visible for debugging)")]
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private float currentHealth = 10f;
    [SerializeField] private float attackDamage = 2f;
    [SerializeField] private float attacksPerSecond = 1f;

    public bool IsDead => currentHealth <= 0f;
    public float AttackDamage => attackDamage;
    public float AttackInterval => attacksPerSecond > 0f ? 1f / attacksPerSecond : float.MaxValue;

    // Fired when this enemy dies, carrying how many coins it should award.
    // EnemySpawner (or a dedicated EnemyDeathHandler) subscribes per-instance to clean up + award coins.
    public event Action<Enemy, int> OnDied;

    /// <summary>
    /// Called by EnemySpawner right after Instantiate to set this enemy's power level.
    /// </summary>
    public void Initialize(float health, float damage, float atkPerSecond, int coinDrop)
    {
        maxHealth = health;
        currentHealth = health;
        attackDamage = damage;
        attacksPerSecond = atkPerSecond;
        baseCoinDrop = coinDrop;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDied?.Invoke(this, baseCoinDrop);
        Destroy(gameObject);
    }
}
