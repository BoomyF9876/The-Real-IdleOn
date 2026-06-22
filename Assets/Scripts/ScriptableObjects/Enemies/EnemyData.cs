using UnityEngine;

/// <summary>
/// Data definition for one enemy TYPE (like ShopItem is for shop entries).
/// The spawner picks a type each spawn by weighted random among types whose
/// unlock time has passed, then instantiates this type's prefab and initializes
/// it with these base stats scaled by the spawner's time-tier growth.
///
/// Spawn frequency rises over time: a type's weight grows by weightRampPerMinute
/// each minute after it unlocks, so stronger enemies steadily crowd out weaker ones.
/// </summary>
[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName;
    [Tooltip("The prefab to spawn for this type (its own sprite/animator/collider + Enemy script). " +
             "Can be a variant of your existing enemy prefab.")]
    public Sprite enemySprite;

    [Header("Base Stats (before time-tier scaling)")]
    public float baseHealth = 10f;
    public float baseDamage = 2f;
    public float baseAttacksPerSecond = 1f;
    public int baseCoinDrop = 1;
    public float baseExp = 1f;

    [Header("Spawn Rules")]
    [Tooltip("Seconds into the run before this type can start spawning. Starter = 0.")]
    public float unlockTimeSeconds = 0f;
    [Tooltip("Relative spawn weight the moment it unlocks.")]
    public float baseWeight = 20f;
    [Tooltip("How much the weight grows per minute after unlocking. Positive = becomes more common over time.")]
    public float weightRampPerMinute = 3f;
    [Tooltip("Weight never drops below this (useful to keep a decaying type spawning occasionally).")]
    public float minWeight = 0f;

    public bool IsUnlocked(float elapsedSeconds) => elapsedSeconds >= unlockTimeSeconds;

    /// <summary>
    /// Current spawn weight at the given elapsed time. 0 before unlock; otherwise
    /// baseWeight grown by the ramp, floored at minWeight.
    /// </summary>
    public float GetCurrentWeight(float elapsedSeconds)
    {
        if (elapsedSeconds < unlockTimeSeconds) return 0f;
        float minutesSinceUnlock = (elapsedSeconds - unlockTimeSeconds) / 60f;
        float w = baseWeight + weightRampPerMinute * minutesSinceUnlock;
        return Mathf.Max(minWeight, w);
    }
}
