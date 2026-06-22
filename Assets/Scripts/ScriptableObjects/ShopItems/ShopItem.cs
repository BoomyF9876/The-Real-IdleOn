using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Shop Item")]
public class ShopItem : ScriptableObject
{
    public Sprite itemSprite;
    public string itemName;
    public int itemCost;

    [Header("Flat stat bonuses")]
    public float healthBoost;
    public float damageBoost;
    public float atkSpeedBoost;
    public float moveSpeedBoost;

    [Header("Fractional bonuses (0.25 = +25%)")]
    public float coinBoost;
    public float experienceBoost;

    /// <summary>
    /// Builds a human-readable summary of this item's effects, e.g.
    /// "+60 HP  +30 DMG  +0.15 Move  +25% Coin". Only non-zero stats are shown.
    /// </summary>
    public string RenderDescription()
    {
        StringBuilder sb = new StringBuilder();

        if (healthBoost != 0f)    sb.Append($"+{healthBoost:0.##} HP  ");
        if (damageBoost != 0f)    sb.Append($"+{damageBoost:0.##} DMG  ");
        if (atkSpeedBoost != 0f)  sb.Append($"+{atkSpeedBoost:0.##} ATK/s  ");
        if (moveSpeedBoost != 0f) sb.Append($"+{moveSpeedBoost:0.##} Move  ");
        if (coinBoost != 0f)      sb.Append($"+{coinBoost * 100f:0.#}% Coin  ");
        if (experienceBoost != 0f) sb.Append($"+{experienceBoost * 100f:0.#}% EXP  ");

        return sb.ToString().TrimEnd();
    }
}
