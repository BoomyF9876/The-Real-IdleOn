using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] Image healthFill;
    [SerializeField] TMP_Text healthText;

    public void RefreshHealthBar(float curHealth, float maxHealth)
    {
        healthFill.fillAmount = curHealth / maxHealth;
        healthText.text = $"{curHealth:F1}/{maxHealth:F1}";
    }
}
