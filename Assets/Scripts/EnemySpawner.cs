using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns enemies at random points within a defined area, on a timer.
/// Difficulty scales with elapsed game time: every `scalingInterval` seconds,
/// enemy health/damage/coin drop all increase by their respective growth rates.
/// 
/// This is the main "knob" for game balance later - tune scalingInterval and the
/// growth multipliers to control how fast the game ramps up.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private Enemy enemyPrefab;

    [Header("Spawn Area")]
    [Tooltip("Spawning uses a box centered on this transform's position. Defaults to this object if left empty.")]
    [SerializeField] private Transform spawnAreaCenter;
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(16f, 8f);

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxAliveEnemies = 10;

    [Header("Enemy Base Stats (at game start)")]
    [SerializeField] private float baseEnemyHealth = 10f;
    [SerializeField] private float baseEnemyDamage = 2f;
    [SerializeField] private float baseEnemyAttacksPerSecond = 1f;
    [SerializeField] private int baseCoinDrop = 1;

    [Header("Difficulty Scaling Over Time")]
    [Tooltip("How often (seconds) difficulty ramps up a notch.")]
    [SerializeField] private float scalingInterval = 30f;
    [Tooltip("Health multiplier applied per scaling tick (e.g. 1.1 = +10% per tick).")]
    [SerializeField] private float healthGrowthPerTick = 1.10f;
    [Tooltip("Damage multiplier applied per scaling tick.")]
    [SerializeField] private float damageGrowthPerTick = 1.08f;
    [Tooltip("Coin drop multiplier applied per scaling tick - lets coin income scale with difficulty.")]
    [SerializeField] private float coinGrowthPerTick = 1.05f;

    private float spawnTimer;
    private float scalingTimer;
    private int difficultyTier; // how many scaling ticks have elapsed

    private readonly List<Enemy> aliveEnemies = new List<Enemy>();

    public static EnemySpawner Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        if (spawnAreaCenter == null) spawnAreaCenter = transform;
    }

    private void Update()
    {
        UpdateScalingTimer();
        UpdateSpawnTimer();
    }

    private void UpdateScalingTimer()
    {
        scalingTimer += Time.deltaTime;
        if (scalingTimer >= scalingInterval)
        {
            scalingTimer -= scalingInterval;
            difficultyTier++;
        }
    }

    private void UpdateSpawnTimer()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer < spawnInterval) return;

        spawnTimer = 0f;

        // Clean up null entries (enemies destroyed without going through OnEnemyDied, just in case)
        aliveEnemies.RemoveAll(e => e == null);

        if (aliveEnemies.Count >= maxAliveEnemies) return;

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        Vector2 center = spawnAreaCenter.position;
        Vector2 halfSize = spawnAreaSize * 0.5f;
        Vector2 spawnPos = new Vector2(
            Random.Range(center.x - halfSize.x, center.x + halfSize.x),
            Random.Range(center.y - halfSize.y, center.y + halfSize.y)
        );

        Enemy enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        float healthMult = Mathf.Pow(healthGrowthPerTick, difficultyTier);
        float damageMult = Mathf.Pow(damageGrowthPerTick, difficultyTier);
        float coinMult = Mathf.Pow(coinGrowthPerTick, difficultyTier);

        float health = baseEnemyHealth * healthMult;
        float damage = baseEnemyDamage * damageMult;
        int coins = Mathf.Max(1, Mathf.RoundToInt(baseCoinDrop * coinMult));

        enemy.Initialize(health, damage, baseEnemyAttacksPerSecond, coins);
        enemy.OnDied += HandleEnemyDied;

        aliveEnemies.Add(enemy);
    }

    private void HandleEnemyDied(Enemy enemy, int coinDrop)
    {
        enemy.OnDied -= HandleEnemyDied;
        aliveEnemies.Remove(enemy);

        if (CoinManager.Instance != null)
        {
            // Apply player's coin multiplier upgrade here at the point of awarding.
            float multiplier = 1f;
            if (PlayerController.Instance != null && PlayerController.Instance.Stats != null)
            {
                multiplier = PlayerController.Instance.Stats.CoinMultiplier;
            }
            int awarded = Mathf.Max(1, Mathf.RoundToInt(coinDrop * multiplier));
            CoinManager.Instance.AddCoins(awarded);
        }
    }

    /// <summary>
    /// Returns the nearest living enemy to a given position, or null if none alive.
    /// Used by PlayerController to pick its target.
    /// </summary>
    public Enemy GetNearestEnemy(Vector2 fromPosition)
    {
        aliveEnemies.RemoveAll(e => e == null);

        Enemy nearest = null;
        float nearestSqrDist = float.MaxValue;

        foreach (Enemy enemy in aliveEnemies)
        {
            float sqrDist = ((Vector2)enemy.transform.position - fromPosition).sqrMagnitude;
            if (sqrDist < nearestSqrDist)
            {
                nearestSqrDist = sqrDist;
                nearest = enemy;
            }
        }

        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        Transform center = spawnAreaCenter != null ? spawnAreaCenter : transform;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center.position, spawnAreaSize);
    }
}
