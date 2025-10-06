using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; // for scene loading

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

    // Player 1 (left)
    [SerializeField] List<ListItem> ShopItems1 = new();
    [SerializeField] List<ListItem> InventoryItems1 = new();

    // Player 2 (right)
    [SerializeField] List<ListItem> ShopItems2 = new();
    [SerializeField] List<ListItem> InventoryItems2 = new();

    Toggle done1, done2;
    Label moneyLabel;

    // Panels
    VisualElement endPanel;
    VisualElement playerPanels;
    VisualElement moneyPanel;

    // Buttons inside EndPanel
    Button returnButton;
    Button continueButton;               // NEW: loads "Prototype"

    void OnEnable()
    {
        var root = ui.rootVisualElement;

        SetupSide(root, "ShopList_1", "Inventory_1", ShopItems1, InventoryItems1);
        SetupSide(root, "ShopList_2", "Inventory_2", ShopItems2, InventoryItems2);

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
            continueButton = endPanel.Q<Button>("ContinueButton"); // query by name
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
                if (MoneyManager.Instance.GetMoneyAmount() < item.itemPrice) return; // not enough funds

                MoneyManager.Instance.SubtractMoney(item.itemPrice);   // deduct money
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
                MoneyManager.Instance.AddMoney(item.itemPrice);
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
                    ? $"Buy For -${item.itemPrice}"
                    : $"Sell For +${item.itemPrice}";
            if (descLabel != null) descLabel.text = item.itemDescription;
        };

        ve.RegisterCallback<MouseEnterEvent>(_ => update());
        ve.RegisterCallback<FocusInEvent>(_ => update());
    }

    void UpdateMoneyLabel()
    {
        if (moneyLabel != null) moneyLabel.text = $"{MoneyManager.Instance.GetMoneyAmount()} $";
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

    // ReturnButton: restore panels and uncheck toggles
    void ResetToNormal()
    {
        if (done1 != null) done1.value = false;
        if (done2 != null) done2.value = false;
        UpdateEndState();
    }

    // ContinueButton: load the next scene
    void LoadPrototypeScene()
    {
        // Make sure "Prototype" is added under File ? Build Settings ? Scenes In Build
        SceneManager.LoadScene("Prototype");
    }
}
