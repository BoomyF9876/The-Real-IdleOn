using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns enemies at random points within the spawn zones, on a timer.
///
/// Two difficulty axes:
///  1. TIME-TIER SCALING: every scalingInterval seconds, whatever enemy spawns gets its
///     HP/damage/coin/exp multiplied by the per-tick growth rates (gradual ramp).
///  2. ENEMY TYPES: each spawn picks a type (EnemyData) by weighted random among types whose
///     unlock time has passed. Stronger types unlock later and their spawn weight ramps up over
///     time, so the mix shifts from weak to strong as the run goes on (sharper ramp).
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Types (assign your EnemyData assets; starter first)")]
    [SerializeField] private List<EnemyData> enemyTypes = new List<EnemyData>();
    [SerializeField] Enemy enemyPrefab;

    [Header("Spawn Area")]
    [SerializeField] GameObject enemySpawnZones;

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxAliveEnemies = 10;

    [Header("Time-Tier Scaling (applies to every type)")]
    [Tooltip("How often (seconds) difficulty ramps up a notch.")]
    [SerializeField] private float scalingInterval = 30f;
    [Tooltip("Health multiplier applied per scaling tick.")]
    [SerializeField] private float healthGrowthPerTick = 1.10f;
    [Tooltip("Damage multiplier applied per scaling tick.")]
    [SerializeField] private float damageGrowthPerTick = 1.08f;
    [Tooltip("Coin-drop multiplier per tick. Keep >1 so income funds exponential upgrade costs.")]
    [SerializeField] private float coinGrowthPerTick = 1.10f;
    [Tooltip("Exp-drop multiplier per tick.")]
    [SerializeField] private float expGrowthPerTick = 1.10f;

    [Header("Duplicate Position Avoidance")]
    [Tooltip("Used when an enemy prefab's size can't be measured. Spacing falls back to this.")]
    [SerializeField] private float fallbackSpacing = 0.5f;
    [Tooltip("Safety cap on retries before giving up and using the last position generated.")]
    [SerializeField] private int maxSpawnPositionAttempts = 30;

    float spawnTimer;
    float scalingTimer;
    float elapsedTime;                  // total seconds since the run started (drives unlocks/weights)
    float minDistanceFromOtherEnemies;  // set per-spawn from the chosen prefab's size
    int difficultyTier;                 // how many scaling ticks have elapsed
    int numSpawnZone;
    readonly List<Enemy> aliveEnemies = new List<Enemy>();

    public static EnemySpawner Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        numSpawnZone = enemySpawnZones.transform.childCount;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        UpdateScalingTimer();
        UpdateSpawnTimer();
    }

    void UpdateScalingTimer()
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

        aliveEnemies.RemoveAll(e => e == null);

        if (aliveEnemies.Count >= maxAliveEnemies) return;

        SpawnEnemy();
    }

    /// <summary>
    /// Picks an enemy type by weighted random among types whose unlock time has passed.
    /// Weights ramp over time (see EnemyData.GetCurrentWeight), so stronger types
    /// become more likely the longer the run goes.
    /// </summary>
    private EnemyData PickEnemyType()
    {
        float total = 0f;
        foreach (EnemyData d in enemyTypes)
        {
            if (d == null) continue;
            total += d.GetCurrentWeight(elapsedTime);
        }

        if (total <= 0f)
        {
            return enemyTypes.Count > 0 ? enemyTypes[0] : null; // fallback: starter
        }

        float r = Random.value * total;
        foreach (EnemyData d in enemyTypes)
        {
            if (d == null) continue;
            float w = d.GetCurrentWeight(elapsedTime);
            if (r < w) return d;
            r -= w;
        }

        return enemyTypes[enemyTypes.Count - 1];
    }

    private Vector2 GenerateRandomPos()
    {
        int randZone = Random.Range(0, numSpawnZone);
        Transform selectedArea = enemySpawnZones.transform.GetChild(randZone);

        Vector2 center = selectedArea.position;
        Vector2 halfSize = selectedArea.gameObject.GetComponent<SpriteRenderer>().bounds.size * 0.5f;
        return new Vector2(
            Random.Range(center.x - halfSize.x, center.x + halfSize.x),
            Random.Range(center.y - halfSize.y, center.y + halfSize.y)
        );
    }

    private Vector2 GenerateNonOverlappingSpawnPos()
    {
        Vector2 spawnPos = GenerateRandomPos();

        int attempts = 0;
        while (IsPositionTooCloseToAliveEnemy(spawnPos) && attempts < maxSpawnPositionAttempts)
        {
            spawnPos = GenerateRandomPos();
            attempts++;
        }

        return spawnPos;
    }

    private bool IsPositionTooCloseToAliveEnemy(Vector2 position)
    {
        foreach (Enemy enemy in aliveEnemies)
        {
            if (enemy == null) continue;

            float dist = ((Vector2)enemy.transform.position - position).magnitude;
            if (dist < minDistanceFromOtherEnemies)
            {
                return true;
            }
        }

        return false;
    }

    private void SpawnEnemy()
    {
        EnemyData data = PickEnemyType();
        if (data == null || enemyPrefab == null) return;

        // Spacing is based on the chosen type's prefab size (falls back if unmeasurable).
        float size = enemyPrefab.GetSize();
        minDistanceFromOtherEnemies = size > 0f ? size : fallbackSpacing;

        Vector2 spawnPos = GenerateNonOverlappingSpawnPos();
        Enemy enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, transform);

        // Apply time-tier scaling on top of this type's base stats.
        float healthMult = Mathf.Pow(healthGrowthPerTick, difficultyTier);
        float damageMult = Mathf.Pow(damageGrowthPerTick, difficultyTier);
        float coinMult   = Mathf.Pow(coinGrowthPerTick, difficultyTier);
        float expMult    = Mathf.Pow(expGrowthPerTick, difficultyTier);

        float health = data.baseHealth * healthMult;
        float damage = data.baseDamage * damageMult;
        int coins    = Mathf.Max(1, Mathf.RoundToInt(data.baseCoinDrop * coinMult));
        float exp    = data.baseExp * expMult;

        enemy.Initialize(health, damage, data.baseAttacksPerSecond, coins, exp, data.enemySprite);
        enemy.OnDied += HandleEnemyDied;

        aliveEnemies.Add(enemy);
    }

    private void HandleEnemyDied(Enemy enemy, int coinDrop)
    {
        enemy.OnDied -= HandleEnemyDied;
        aliveEnemies.Remove(enemy);

        // CoinMultiplier is a fractional bonus (0.30 = +30%), so multiply rather than add.
        int awarded = Mathf.Max(1, Mathf.RoundToInt(coinDrop * (1f + PlayerController.Instance.Stats.CoinMultiplier)));
        CoinManager.Instance.AddCoins(awarded);
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
}
