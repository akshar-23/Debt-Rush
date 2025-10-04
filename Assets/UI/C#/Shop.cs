using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class Shop_UI : MonoBehaviour
{
    [SerializeField] UIDocument ui;
    // Player 1 (left)
    [SerializeField] List<string> ShopItems1 = new();
    [SerializeField] List<string> InventoryItems1 = new();

    // Player 2 (right)
    [SerializeField] List<string> ShopItems2 = new();
    [SerializeField] List<string> InventoryItems2 = new();

    [SerializeField] int inventoryLimit = 12;

    void OnEnable()
    {
        var root = ui.rootVisualElement;

        // Left side (P1)
        SetupSide(root, "ShopList_1", "Inventory_1", ShopItems1, InventoryItems1);

        // Right side (P2)
        SetupSide(root, "ShopList_2", "Inventory_2", ShopItems2, InventoryItems2);

    }

    void SetupSide(VisualElement root, string shopPanelName, string invPanelName,
               List<string> shopItems, List<string> invItems)
    {
        var shopPanel = root.Q<VisualElement>(shopPanelName);
        var invPanel = root.Q<VisualElement>(invPanelName);

        shopPanel.Clear();
        invPanel.Clear();

        // preload inventory
        foreach (var name in invItems)
            AddInventoryButton(name, invPanel);

        // build shop
        foreach (var name in shopItems)
        {
            var btn = new Button { text = name };
            btn.AddToClassList("button");

            btn.clicked += () =>
            {
                if (invPanel.childCount >= inventoryLimit) return;
                AddInventoryButton(name, invPanel);
            };

            shopPanel.Add(btn);
        }
    }

    void AddInventoryButton(string itemName, VisualElement invPanel)
    {
        var invBtn = new Button { text = itemName };
        invBtn.AddToClassList("button");
        invBtn.clicked += () => invPanel.Remove(invBtn); // remove on click
        invPanel.Add(invBtn);
    }
}
