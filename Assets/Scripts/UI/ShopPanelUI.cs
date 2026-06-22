using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The shop panel. Renders one-time items; buying an item charges coins, applies its
/// bonuses to PlayerStats, and locks it as Owned. Affordability updates live with coins.
/// </summary>
public class ShopPanelUI : MonoBehaviour
{
    [SerializeField] Button shopBtn;
    [SerializeField] Button backBtn;
    [SerializeField] GameObject shopPanel;
    [SerializeField] GameObject shopItemContainer;
    [SerializeField] ShopItemUI shopItemPrefab;
    [SerializeField] List<ShopItem> itemList;
    [Tooltip("Red exclamation badge shown only when an unowned item is affordable.")]
    [SerializeField] Image availHint;

    PlayerStats stats;
    readonly List<ShopItemUI> spawnedItems = new List<ShopItemUI>();
    readonly HashSet<ShopItem> owned = new HashSet<ShopItem>();

    void Start()
    {
        HidePanel();
        shopBtn.onClick.AddListener(ShowPanel);
        backBtn.onClick.AddListener(HidePanel);

        stats = PlayerController.Instance.Stats;

        RenderItems();

        CoinManager.Instance.OnCoinsChanged += OnCoinsChanged;
        RefreshAffordability(CoinManager.Instance.CurrentCoins);
    }

    void OnDisable()
    {
        if (CoinManager.Instance != null) CoinManager.Instance.OnCoinsChanged -= OnCoinsChanged;
    }

    void OnCoinsChanged(int coins) => RefreshAffordability(coins);

    void RenderItems()
    {
        int index = 0;
        foreach (var item in itemList)
        {
            ShopItemUI obj = Instantiate(shopItemPrefab, shopItemContainer.transform);
            obj.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 250 - index * 180, 0);
            obj.Init(item, TryPurchase);
            spawnedItems.Add(obj);
            index++;
        }
    }

    void TryPurchase(ShopItem item, ShopItemUI ui)
    {
        if (owned.Contains(item)) return;                 // one-time only
        if (!CoinManager.Instance.TrySpendCoins(item.itemCost)) return; // can't afford

        stats.ApplyShopItem(item);
        owned.Add(item);
        ui.MarkOwned();

        // Recompute after ownership is recorded so the hint reflects the final state
        // (the coin-change event fired mid-spend, before this item was marked owned).
        RefreshAffordability(CoinManager.Instance.CurrentCoins);
    }

    void RefreshAffordability(int coins)
    {
        bool anyAvailable = false;

        foreach (var ui in spawnedItems)
        {
            ui.RefreshAffordability(coins);

            // "Available" = not already owned AND affordable.
            if (ui.Item != null && !owned.Contains(ui.Item) && coins >= ui.Item.itemCost)
                anyAvailable = true;
        }

        // Show the red exclamation badge only when something buyable is in the shop.
        if (availHint != null) availHint.gameObject.SetActive(anyAvailable);
    }

    void ShowPanel() => shopPanel.SetActive(true);
    void HidePanel() => shopPanel.SetActive(false);
}
