using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] Image itemSprite;
    [SerializeField] TMP_Text itemName;
    [SerializeField] TMP_Text itemDescription;
    [SerializeField] TMP_Text itemCost;

    public void Init(ShopItem item)
    {
        itemSprite.sprite = item.itemSprite;
        itemName.text = item.itemName;
        itemCost.text = item.itemCost.ToString();
        itemDescription.text = item.RenderDescription();
    }
}
