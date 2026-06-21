using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private float attackDamage = 2f;
    [SerializeField] private float attacksPerSecond = 1f;
    [SerializeField] private float hitPushBack = 5f;

    [Header("Coin")]
    [SerializeField] GameObject coinPrefab;

    [Header("Health Bar")]
    [SerializeField] HealthBarUI healthBar;

    Animator animator;
    Rigidbody2D rb;
    float currentHealth;

    public bool IsDead => currentHealth <= 0f;
    public float AttackDamage => attackDamage;
    public float AttackInterval => attacksPerSecond > 0f ? 1f / attacksPerSecond : float.MaxValue;

    public event Action<Enemy, int> OnDied;

    /// <summary>
    /// Called by EnemySpawner right after Instantiate to set this enemy's power level.
    /// </summary>
    public void Initialize(float health, float damage, float atkPerSecond, int coinDrop)
    {
        maxHealth = health;
        attackDamage = damage;
        attacksPerSecond = atkPerSecond;
        baseCoinDrop = coinDrop;

        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        healthBar.gameObject.SetActive(false);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        animator.SetTrigger("hit");
        healthBar.gameObject.SetActive(true);
        healthBar.RefreshHealthBar(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void PushBack(float direction)
    {
        float mag = direction > 0 ? hitPushBack : -hitPushBack;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(new Vector2(mag, Mathf.Abs(mag)));
    }

    public float GetSize()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null) return 0f;

        return collider.bounds.size.magnitude;
    }

    private void Die()
    {
        OnDied?.Invoke(this, baseCoinDrop);
        Instantiate(coinPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
