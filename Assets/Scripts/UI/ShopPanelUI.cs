using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelUI : MonoBehaviour
{
    [SerializeField] Button shopBtn;
    [SerializeField] Button backBtn;
    [SerializeField] GameObject shopPanel;
    [SerializeField] GameObject shopItemContainer;
    [SerializeField] GameObject shopItemPrefab;
    [SerializeField] List<ShopItem> itemList;

    void Start()
    {
        HidePanel();
        shopBtn.onClick.AddListener(ShowPanel);
        backBtn.onClick.AddListener(HidePanel);
    }

    void ShowPanel()
    {
        shopPanel.SetActive(true);
    }

    void HidePanel()
    {
        shopPanel.SetActive(false);
    }
}
