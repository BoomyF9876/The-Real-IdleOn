using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelUI : MonoBehaviour
{
    [SerializeField] Button shopBtn;
    [SerializeField] Button backBtn;
    [SerializeField] GameObject shopPanel;
    [SerializeField] GameObject shopItemContainer;
    [SerializeField] ShopItemUI shopItemPrefab;
    [SerializeField] List<ShopItem> itemList;

    void Start()
    {
        HidePanel();
        shopBtn.onClick.AddListener(ShowPanel);
        backBtn.onClick.AddListener(HidePanel);

        RenderItems();
    }

    void ShowPanel()
    {
        shopPanel.SetActive(true);
    }

    void HidePanel()
    {
        shopPanel.SetActive(false);
    }

    void RenderItems()
    {
        int index = 0;
        foreach (var item in itemList)
        {
            ShopItemUI obj = Instantiate(shopItemPrefab, new Vector3(0, 250 - index * 100, 0), Quaternion.identity, shopItemContainer.transform);
            obj.Init(item);
            index++;
        }
    }
}
