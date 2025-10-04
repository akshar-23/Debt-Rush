using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Shop_UI : MonoBehaviour
{
    [SerializeField] int inventoryLimit = 12;
    [SerializeField] float money = 1500f;

    [System.Serializable]
    public class ListItem
    {
        public string itemName;
        public float itemPrice;
        [TextArea] public string itemDescription;
    }

    [SerializeField] UIDocument ui;

    // Player 1 (left)
    [SerializeField] List<ListItem> ShopItems1 = new();
    [SerializeField] List<ListItem> InventoryItems1 = new();

    // Player 2 (right)
    [SerializeField] List<ListItem> ShopItems2 = new();
    [SerializeField] List<ListItem> InventoryItems2 = new();

    Label moneyLabel;

    void OnEnable()
    {
        var root = ui.rootVisualElement;

        SetupSide(root, "ShopList_1", "Inventory_1", ShopItems1, InventoryItems1);
        SetupSide(root, "ShopList_2", "Inventory_2", ShopItems2, InventoryItems2);

        moneyLabel = root.Q<Label>("MoneyLabel");
        UpdateMoneyLabel();
    }

    void SetupSide(VisualElement root, string shopPanelName, string invPanelName,
                   List<ListItem> shopItems, List<ListItem> invItems)
    {
        var shopPanel = root.Q<VisualElement>(shopPanelName);
        var invPanel = root.Q<VisualElement>(invPanelName);

        shopPanel.Clear();
        invPanel.Clear();

        var playerRoot = shopPanel.parent.parent;
        var titleLabel = playerRoot.Q<Label>(className: "description-title");
        var priceLabel = playerRoot.Q<Label>(className: "price-text");
        var descLabel = playerRoot.Q<Label>(className: "description-text");

        // preload inventory (not removable)
        foreach (var item in invItems)
            AddInventoryButton(item, invPanel, false, titleLabel, priceLabel, descLabel);

        // build shop
        foreach (var item in shopItems)
        {
            var btn = new Button { text = item.itemName, focusable = true };
            btn.AddToClassList("button");

            AttachInfoHandlers(btn, item, titleLabel, priceLabel, descLabel, isBuy: true);

            btn.clicked += () =>
            {
                if (invPanel.childCount >= inventoryLimit) return;
                if (money < item.itemPrice) return; // not enough funds

                money -= item.itemPrice;            // deduct money
                UpdateMoneyLabel();

                AddInventoryButton(item, invPanel, true, titleLabel, priceLabel, descLabel);
            };

            shopPanel.Add(btn);
        }
    }

    void AddInventoryButton(ListItem item, VisualElement invPanel, bool removable,
                            Label titleLabel, Label priceLabel, Label descLabel)
    {
        var invBtn = new Button { text = item.itemName, focusable = true };
        invBtn.AddToClassList("button");

        AttachInfoHandlers(invBtn, item, titleLabel, priceLabel, descLabel, isBuy: false);

        if (removable)
        {
            invBtn.clicked += () =>
            {
                invPanel.Remove(invBtn);

                // add money back on selling
                money += item.itemPrice;
                UpdateMoneyLabel();
            };
        }

        invPanel.Add(invBtn);
    }

    void AttachInfoHandlers(VisualElement ve, ListItem item,
                            Label titleLabel, Label priceLabel, Label descLabel,
                            bool isBuy)
    {
        System.Action update = () =>
        {
            if (titleLabel != null) titleLabel.text = item.itemName;
            if (priceLabel != null)
                priceLabel.text = isBuy
                    ? $"Buy For -${item.itemPrice:0.##}"
                    : $"Sell For +${item.itemPrice:0.##}";
            if (descLabel != null) descLabel.text = item.itemDescription;
        };

        ve.RegisterCallback<MouseEnterEvent>(_ => update());
        ve.RegisterCallback<FocusInEvent>(_ => update());
    }

    void UpdateMoneyLabel()
    {
        if (moneyLabel != null) moneyLabel.text = $"{money} $";
    }
}
