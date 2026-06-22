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
    [SerializeField] GameObject enemySpawnZones;

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxAliveEnemies = 10;

    [Header("Enemy Base Stats (at game start)")]
    [SerializeField] private float baseEnemyHealth = 10f;
    [SerializeField] private float baseEnemyDamage = 2f;
    [SerializeField] private float baseEnemyAttacksPerSecond = 1f;
    [SerializeField] private float baseExp = 1f;
    [SerializeField] private int baseCoinDrop = 1;

    [Header("Difficulty Scaling Over Time")]
    [Tooltip("How often (seconds) difficulty ramps up a notch.")]
    [SerializeField] private float scalingInterval = 30f;
    [Tooltip("Health multiplier applied per scaling tick (e.g. 1.1 = +10% per tick).")]
    [SerializeField] private float healthGrowthPerTick = 1.10f;
    [Tooltip("Damage multiplier applied per scaling tick.")]
    [SerializeField] private float damageGrowthPerTick = 1.08f;
    [Tooltip("Coin-drop multiplier per tick. MUST scale (>1) or the economy can't fund exponential upgrade costs. 1.10 tracks enemy HP growth.")]
    [SerializeField] private float coinGrowthPerTick = 1.10f;
    [Tooltip("Exp-drop multiplier per tick. Keeps leveling progressing as enemies scale. 1.10 tracks enemy HP growth.")]
    [SerializeField] private float expGrowthPerTick = 1.10f;

    [Header("Duplicate Position Avoidance")]
    [Tooltip("Safety cap on retries before giving up and using the last position generated.")]
    [SerializeField] private int maxSpawnPositionAttempts = 30;

    float spawnTimer;
    float scalingTimer;
    float minDistanceFromOtherEnemies;
    int difficultyTier; // how many scaling ticks have elapsed
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
        minDistanceFromOtherEnemies = enemyPrefab.GetSize();
    }

    void Update()
    {
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

        // Clean up null entries (enemies destroyed without going through OnEnemyDied, just in case)
        aliveEnemies.RemoveAll(e => e == null);

        if (aliveEnemies.Count >= maxAliveEnemies) return;

        SpawnEnemy();
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

            float sqrDist = ((Vector2)enemy.transform.position - position).magnitude;
            if (sqrDist < minDistanceFromOtherEnemies)
            {
                return true;
            }
        }

        return false;
    }

    private void SpawnEnemy()
    {
        Vector2 spawnPos = GenerateNonOverlappingSpawnPos();
        Enemy enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, transform);

        float healthMult = Mathf.Pow(healthGrowthPerTick, difficultyTier);
        float damageMult = Mathf.Pow(damageGrowthPerTick, difficultyTier);

        float health = baseEnemyHealth * healthMult;
        float damage = baseEnemyDamage * damageMult;
        // Coins and exp scale with tier too, so income/leveling keep pace with rising
        // enemy power and exponential upgrade costs.
        int coins = Mathf.Max(1, Mathf.RoundToInt(baseCoinDrop * Mathf.Pow(coinGrowthPerTick, difficultyTier)));
        float exp = baseExp * Mathf.Pow(expGrowthPerTick, difficultyTier);

        enemy.Initialize(health, damage, baseEnemyAttacksPerSecond, coins, exp);
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
