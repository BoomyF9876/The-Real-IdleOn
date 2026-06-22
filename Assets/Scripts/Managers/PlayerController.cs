using System.Collections.Generic;
using Pathfinding;
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
    [SerializeField] private float retargetInterval = 0.5f;
    [SerializeField] private float nextWaypointDistance = 3f;
    [SerializeField] private float jumpNodeHeightRequirement = 0.8f;
    [SerializeField] private float jumpModifier = 0.3f;
    [SerializeField] private float jumpCheckOffset = 0.1f;
    [SerializeField] private float pathUpdateSeconds = 0.5f;

    [SerializeField] List<SpriteRenderer> restrictedArea;

    int currentWaypoint = 0;
    bool isGrounded = false;
    float attackTimer;
    float retargetTimer;
    float enemyAttackTimer;

    Path path;
    Enemy currentTarget;
    Seeker seeker;
    Rigidbody2D rb;
    Animator animator;

    public PlayerStats Stats { get; private set; }
    public static PlayerController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Stats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
        animator = GetComponent<Animator>();

        InvokeRepeating("UpdateTargeting", 0f, pathUpdateSeconds);
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
        if (currentTarget == null) return;

        CheckFacing();

        float distance = Vector2.Distance(transform.position, currentTarget.transform.position);
        if (distance > Stats.AttackRange)
        {
            animator.SetBool("isInCombat", false);
            animator.SetBool("isRunning", true);
            MoveTowardTarget();
        }
        else
        {
            animator.SetBool("isInCombat", true);
            animator.SetBool("isRunning", false);
            AttackTarget();
        }

        if (retargetTimer < retargetInterval) retargetTimer += Time.deltaTime;
    }

    private void UpdateTargeting()
    {
        if (Stats.IsDead || animator.GetBool("isInCombat")) return;
        // If current target died or was cleared, find a new one immediately.
        if (currentTarget == null)
        {
            currentTarget = EnemySpawner.Instance.GetNearestEnemy(transform.position);
            return;
        }

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

        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, currentTarget.transform.position, OnPathComplete);
        }
    }

    private void CheckFacing()
    {
        int direction = 1;
        if (rb.linearVelocityX != 0)
        {
            direction = rb.linearVelocityX < -0.05f ? -1 : 1;
        }
        else
        {
            direction = transform.position.x > currentTarget.transform.position.x ? -1 : 1;
        }

        transform.localScale = new Vector3(direction * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    private void MoveTowardTarget()
    {
        if (path == null) return;
        if (currentWaypoint >= path.vectorPath.Count) return;

        isGrounded = Physics2D.Raycast(transform.position, Vector3.down, GetComponent<Collider2D>().bounds.extents.y + jumpCheckOffset);
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        direction = FixDirection(direction);

        if (isGrounded)
        {
            if (direction.y > jumpNodeHeightRequirement)
            {
                rb.AddForce(Vector2.up * Stats.MoveSpeed * jumpModifier * Time.deltaTime);
            }
        }

        direction.y = 0f;
        direction.Normalize();
        rb.AddForce(direction * Stats.MoveSpeed * Time.deltaTime);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        enemyAttackTimer = 0f;
    }

    private Vector2 FixDirection(Vector2 direction)
    {
        if (direction.y < 0)
        {
            direction.y = 0;
        }

        foreach (SpriteRenderer t in restrictedArea)
        {
            if (t == null) continue;
            if (t.bounds.Contains(transform.position))
            {
                direction.y = 0;
                break;
            }
        }

        return direction.normalized;
    }

    private void AttackTarget()
    {
        // Player attacks on their own cooldown.
        attackTimer += Time.deltaTime;
        float direction = currentTarget.transform.position.x - transform.position.x;

        if (attackTimer >= Stats.AttackInterval)
        {
            attackTimer = 0f;
            currentTarget.TakeDamage(Stats.AttackDamage);
            currentTarget.PushBack(direction);
            animator.SetTrigger("attack");

            if (currentTarget.IsDead)
            {
                Stats.GainExperience(currentTarget.Exp * (1f + Stats.ExpGain));
                Stats.ApplyHealOnKill();
                currentTarget = null;
                animator.SetBool("isInCombat", false);
                return;
            }
        }

        // Enemy attacks back on its own cooldown while in range.
        enemyAttackTimer += Time.deltaTime;
        if (currentTarget != null && enemyAttackTimer >= currentTarget.AttackInterval)
        {
            enemyAttackTimer = 0f;
            Stats.TakeDamage(currentTarget.AttackDamage);
            animator.SetTrigger("hit");
        }
    }

    private void HandlePlayerDied()
    {
        animator.SetBool("isInCombat", false);
        currentTarget = null;
        enabled = false;
        GameManager.Instance.ChangeState(GameState.GameOver);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Stats.AttackRange);
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }
}
