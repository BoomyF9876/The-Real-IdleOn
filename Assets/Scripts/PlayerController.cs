using UnityEngine;

/// <summary>
/// Drives the player's autonomous behavior:
/// 1. Find nearest enemy
/// 2. Move toward it (simple transform movement, no physics)
/// 3. When in attack range, stop and attack on a cooldown derived from AttacksPerSecond
/// 4. Take damage back from whatever it's currently fighting
/// 
/// No player input at all - this is intentional per the design (player only manages upgrades).
/// </summary>
[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Combat")]
    [SerializeField] private float attackRange = 0.75f;
    [Tooltip("How often (seconds) we re-check for a nearer enemy while already in combat.")]
    [SerializeField] private float retargetInterval = 0.5f;

    public PlayerStats Stats { get; private set; }

    private Enemy currentTarget;
    private float attackTimer;
    private float retargetTimer;
    private float enemyAttackTimer;

    private void Awake()
    {
        Instance = this;
        Stats = GetComponent<PlayerStats>();
    }

    private void OnEnable()
    {
        Stats.OnPlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        Stats.OnPlayerDied -= HandlePlayerDied;
    }

    private void Update()
    {
        if (Stats.IsDead) return;

        UpdateTargeting();

        if (currentTarget == null)
        {
            return; // nothing to fight, idle
        }

        float distance = Vector2.Distance(transform.position, currentTarget.transform.position);

        if (distance > attackRange)
        {
            MoveTowardTarget();
        }
        else
        {
            AttackTarget();
        }
    }

    private void UpdateTargeting()
    {
        // If current target died or was cleared, find a new one immediately.
        if (currentTarget == null)
        {
            currentTarget = EnemySpawner.Instance != null
                ? EnemySpawner.Instance.GetNearestEnemy(transform.position)
                : null;
            return;
        }

        // Periodically check if a closer enemy has spawned, so the player doesn't
        // beeline across the map past enemies that just appeared nearby.
        retargetTimer += Time.deltaTime;
        if (retargetTimer >= retargetInterval)
        {
            retargetTimer = 0f;
            Enemy nearest = EnemySpawner.Instance != null
                ? EnemySpawner.Instance.GetNearestEnemy(transform.position)
                : null;

            if (nearest != null && nearest != currentTarget)
            {
                float currentDist = Vector2.Distance(transform.position, currentTarget.transform.position);
                float nearestDist = Vector2.Distance(transform.position, nearest.transform.position);
                if (nearestDist < currentDist)
                {
                    currentTarget = nearest;
                }
            }
        }
    }

    private void MoveTowardTarget()
    {
        Vector3 targetPos = currentTarget.transform.position;
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            Stats.MoveSpeed * Time.deltaTime
        );

        // Reset enemy attack timer while out of its range so it doesn't "bank" a free hit.
        enemyAttackTimer = 0f;
    }

    private void AttackTarget()
    {
        // Player attacks on their own cooldown.
        attackTimer += Time.deltaTime;
        if (attackTimer >= Stats.AttackInterval)
        {
            attackTimer = 0f;
            currentTarget.TakeDamage(Stats.AttackDamage);

            if (currentTarget.IsDead)
            {
                currentTarget = null;
                return;
            }
        }

        // Enemy attacks back on its own cooldown while in range.
        enemyAttackTimer += Time.deltaTime;
        if (currentTarget != null && enemyAttackTimer >= currentTarget.AttackInterval)
        {
            enemyAttackTimer = 0f;
            Stats.TakeDamage(currentTarget.AttackDamage);
        }
    }

    private void HandlePlayerDied()
    {
        // Hook point for game-over / respawn logic.
        // For the core-loop milestone, just stop acting.
        currentTarget = null;
        enabled = false;
        Debug.Log("Player died.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
