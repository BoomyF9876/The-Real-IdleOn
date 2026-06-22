using System;

/// <summary>
/// Anything that can take damage and die implements this.
/// Shared by Player and Enemy so combat code doesn't care which it's hitting.
/// </summary>
public interface IDamageable
{
    bool IsDead { get; }
    void TakeDamage(float amount);
}
