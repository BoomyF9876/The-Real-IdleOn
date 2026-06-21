using UnityEngine;

public class StatPanelUI : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] HealthBarUI healthBar;

    void Start()
    {
        healthBar.RefreshHealthBar(PlayerController.Instance.Stats.CurrentHealth, PlayerController.Instance.Stats.MaxHealth);
        PlayerController.Instance.Stats.OnHealthChanged += healthBar.RefreshHealthBar;
    }

    void OnDisable()
    {
        PlayerController.Instance.Stats.OnHealthChanged -= healthBar.RefreshHealthBar;
    }
}
