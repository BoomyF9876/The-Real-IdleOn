using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Shop Item")]
public class ShopItem : ScriptableObject
{
    public Sprite itemSprite;
    public string itemName;
    public int itemCost;
    public float healthBoost;
    public float damageBoost;
    public float atkSpeedBoost;
    public float moveSpeedBoost;
    public float coinBoost;
    public float experienceBoost;
}
