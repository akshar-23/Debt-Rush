using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class Shop_UI : MonoBehaviour
{
    [SerializeField] int inventoryLimit = 12;
    [SerializeField] int initialMoney = 1500;

    [System.Serializable]
    public class ListItem
    {
        public string itemName;
        public int itemPrice;
        [TextArea] public string itemDescription;
    }

    [SerializeField] UIDocument ui;

    // Shop data (rich: name/price/description)
    [SerializeField] List<ListItem> ShopItems1 = new();
    [SerializeField] List<ListItem> ShopItems2 = new();

    Toggle done1, done2;
    Label moneyLabel;

    VisualElement endPanel;
    VisualElement playerPanels;
    VisualElement moneyPanel;

    Button returnButton;
    Button continueButton;

    void OnEnable()
    {
        var root = ui.rootVisualElement;

        // Build both sides from shop data + GameManager inventories
        SetupSide(root, playerIndex: 1, "ShopList_1", "Inventory_1", ShopItems1);
        SetupSide(root, playerIndex: 2, "ShopList_2", "Inventory_2", ShopItems2);

        moneyLabel = root.Q<Label>("MoneyLabel");
        MoneyManager.Instance.AddMoney(initialMoney);
        UpdateMoneyLabel();

        done1 = root.Q<Toggle>("DoneToggle_1");
        done2 = root.Q<Toggle>("DoneToggle_2");

        endPanel = root.Q<VisualElement>("EndPanel");
        playerPanels = root.Q<VisualElement>("PlayerPanels");
        moneyPanel = root.Q<VisualElement>("MoneyPanel");

        if (endPanel != null)
        {
            returnButton = endPanel.Q<Button>("ReturnButton");
            continueButton = endPanel.Q<Button>("ContinueButton");
        }

        if (done1 != null) done1.RegisterValueChangedCallback(_ => UpdateEndState());
        if (done2 != null) done2.RegisterValueChangedCallback(_ => UpdateEndState());

        if (returnButton != null) returnButton.clicked += ResetToNormal;
        if (continueButton != null) continueButton.clicked += LoadPrototypeScene;

        UpdateEndState();
    }

    void OnDisable()
    {
        if (returnButton != null) returnButton.clicked -= ResetToNormal;
        if (continueButton != null) continueButton.clicked -= LoadPrototypeScene;
    }

    // ---------- UI BUILD PER PLAYER ----------
    void SetupSide(VisualElement root, int playerIndex,
                   string shopPanelName, string invPanelName,
                   List<ListItem> shopItems)
    {
        var shopPanel = root.Q<VisualElement>(shopPanelName);
        var invPanel = root.Q<VisualElement>(invPanelName);
        shopPanel.Clear();
        invPanel.Clear();

        // Resolve labels for the info box under this player
        var playerRoot = shopPanel.parent.parent;
        var titleLabel = playerRoot.Q<Label>(className: "description-title");
        var priceLabel = playerRoot.Q<Label>(className: "price-text");
        var descLabel = playerRoot.Q<Label>(className: "description-text");

        // Preload INVENTORY from GameManager (names only)
        foreach (var name in GameManager.Instance.GetInventory(playerIndex))
            AddInventoryButton(name, invPanel, removable: true,  // preloaded items can be sold by your current logic; set false if not
                               playerIndex, titleLabel, priceLabel, descLabel);

        // Build SHOP buttons (buy)
        foreach (var item in shopItems)
        {
            var btn = new Button { text = item.itemName, focusable = true };
            btn.AddToClassList("button");

            AttachInfoHandlers(btn, item, titleLabel, priceLabel, descLabel, isBuy: true);

            btn.clicked += () =>
            {
                // capacity check can be UI-based or inventory-based
                if (invPanel.childCount >= inventoryLimit) return;
                if (MoneyManager.Instance.GetMoneyAmount() < item.itemPrice) return;

                // Deduct money, add to central inventory, and reflect in UI
                MoneyManager.Instance.SubtractMoney(item.itemPrice);
                UpdateMoneyLabel();

                GameManager.Instance.AddToInventory(playerIndex, item.itemName);
                AddInventoryButton(item.itemName, invPanel, removable: true,
                                   playerIndex, titleLabel, priceLabel, descLabel);
            };

            shopPanel.Add(btn);
        }
    }

    // Create a button inside inventory list for a single item name
    void AddInventoryButton(string itemName, VisualElement invPanel, bool removable,
                            int playerIndex, Label titleLabel, Label priceLabel, Label descLabel)
    {
        var invBtn = new Button { text = itemName, focusable = true };
        invBtn.AddToClassList("button");

        // For SELL preview we need price/description; pull from SHOP data by name
        var itemData = FindItemDataForPlayer(playerIndex, itemName);
        if (itemData != null)
            AttachInfoHandlers(invBtn, itemData, titleLabel, priceLabel, descLabel, isBuy: false);

        if (removable)
        {
            invBtn.clicked += () =>
            {
                // Remove one occurrence from central inventory, give money back, and update UI
                if (GameManager.Instance.RemoveFromInventory(playerIndex, itemName))
                {
                    invPanel.Remove(invBtn);
                    if (itemData != null)
                    {
                        MoneyManager.Instance.AddMoney(itemData.itemPrice);
                        UpdateMoneyLabel();
                    }
                }
            };
        }

        invPanel.Add(invBtn);
    }

    // Lookup helper: given player and name, find the shop item (to show price/desc on hover)
    ListItem FindItemDataForPlayer(int playerIndex, string itemName)
    {
        var list = (playerIndex == 1) ? ShopItems1 : ShopItems2;
        return list.Find(x => x.itemName == itemName);
    }

    // ---------- Shared helpers ----------
    void AttachInfoHandlers(VisualElement ve, ListItem item,
                            Label titleLabel, Label priceLabel, Label descLabel,
                            bool isBuy)
    {
        System.Action update = () =>
        {
            if (titleLabel != null) titleLabel.text = item.itemName;
            if (priceLabel != null)
                priceLabel.text = isBuy
                    ? $"Buy For -${item.itemPrice}"
                    : $"Sell For +${item.itemPrice}";
            if (descLabel != null) descLabel.text = item.itemDescription;
        };

        ve.RegisterCallback<MouseEnterEvent>(_ => update());
        ve.RegisterCallback<FocusInEvent>(_ => update());
    }

    void UpdateMoneyLabel()
    {
        if (moneyLabel != null)
            moneyLabel.text = $"{MoneyManager.Instance.GetMoneyAmount()} $";
    }

    void UpdateEndState()
    {
        bool bothDone = (done1 != null && done1.value) && (done2 != null && done2.value);

        if (endPanel != null)
            endPanel.style.display = bothDone ? DisplayStyle.Flex : DisplayStyle.None;

        if (playerPanels != null)
            playerPanels.style.display = bothDone ? DisplayStyle.None : DisplayStyle.Flex;

        if (moneyPanel != null)
            moneyPanel.style.display = bothDone ? DisplayStyle.None : DisplayStyle.Flex;
    }

    void ResetToNormal()
    {
        if (done1 != null) done1.value = false;
        if (done2 != null) done2.value = false;
        UpdateEndState();
    }

    void LoadPrototypeScene()
    {
        SceneManager.LoadScene("Prototype"); // ensure in Build Settings
    }
}
