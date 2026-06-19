using UnityEngine;

/// <summary>
/// Throwaway debug overlay using OnGUI - no Canvas/UI setup required.
/// Lets you verify the core loop (coins increasing, HP changing) before building real UI.
/// Delete or disable this once you build a proper UI with the upgrade system.
/// </summary>
public class DebugHUD : MonoBehaviour
{
    private void OnGUI()
    {
        GUI.skin.label.fontSize = 20;

        GUILayout.BeginArea(new Rect(10, 10, 300, 150), GUI.skin.box);

        int coins = CoinManager.Instance != null ? CoinManager.Instance.CurrentCoins : 0;
        GUILayout.Label($"Coins: {coins}");

        if (PlayerController.Instance != null && PlayerController.Instance.Stats != null)
        {
            PlayerStats stats = PlayerController.Instance.Stats;
            GUILayout.Label($"HP: {stats.CurrentHealth:0} / {stats.MaxHealth:0}");
            GUILayout.Label($"Move Speed: {stats.MoveSpeed:0.0}");
            GUILayout.Label($"Attack Dmg: {stats.AttackDamage:0.0}");
            GUILayout.Label($"Atk/Sec: {stats.AttacksPerSecond:0.0}");

            if (stats.IsDead)
            {
                GUILayout.Label("PLAYER DEAD");
            }
        }

        GUILayout.EndArea();
    }
}
