using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One row in the shop. Displays an item and a buy button. Reports clicks back to
/// ShopPanelUI via a callback; ShopPanelUI owns the coins/ownership logic.
/// </summary>
public class ShopItemUI : MonoBehaviour
{
    [SerializeField] Image itemSprite;
    [SerializeField] TMP_Text itemName;
    [SerializeField] TMP_Text itemDescription;
    [SerializeField] TMP_Text itemCost;
    [SerializeField] Button buyBtn;

    ShopItem item;
    Action<ShopItem, ShopItemUI> onPurchase;
    bool owned;

    public ShopItem Item => item;

    public void Init(ShopItem shopItem, Action<ShopItem, ShopItemUI> purchaseCallback)
    {
        item = shopItem;
        onPurchase = purchaseCallback;

        itemSprite.sprite = item.itemSprite;
        itemName.text = item.itemName;
        itemCost.text = item.itemCost.ToString();
        itemDescription.text = item.RenderDescription();

        buyBtn.onClick.RemoveAllListeners();
        buyBtn.onClick.AddListener(() => onPurchase?.Invoke(item, this));
    }

    /// <summary>Greys the buy button when the player can't afford it (ignored once owned).</summary>
    public void RefreshAffordability(int coins)
    {
        if (owned) return;
        buyBtn.interactable = coins >= item.itemCost;
    }

    public void MarkOwned()
    {
        owned = true;
        buyBtn.interactable = false;
        var label = buyBtn.GetComponentInChildren<TMP_Text>();
        if (label != null) label.text = "Owned";
    }
}
