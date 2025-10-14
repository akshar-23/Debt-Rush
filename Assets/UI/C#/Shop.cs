using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class Shop_UI : MonoBehaviour
{
    [SerializeField] int inventoryLimit = 12;
    [SerializeField] int initialMoney = 1500;
    [SerializeField] UIDocument ui;

    Toggle done1, done2;
    Label moneyLabel;
    VisualElement endPanel, playerPanels, moneyPanel;
    Button returnButton, continueButton;

    void OnEnable()
    {
        var root = ui.rootVisualElement;

        // Build both sides from GameManager data
        SetupSide(root, 1, "ShopList_1", "Inventory_1");
        SetupSide(root, 2, "ShopList_2", "Inventory_2");

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
    void SetupSide(VisualElement root, int playerIndex, string shopPanelName, string invPanelName)
    {
        var shopPanel = root.Q<VisualElement>(shopPanelName);
        var invPanel = root.Q<VisualElement>(invPanelName);
        shopPanel.Clear();
        invPanel.Clear();

        // Info labels for this player column
        var playerRoot = shopPanel.parent.parent;
        var titleLabel = playerRoot.Q<Label>(className: "description-title");
        var priceLabel = playerRoot.Q<Label>(className: "price-text");
        var descLabel = playerRoot.Q<Label>(className: "description-text");

        // Preload inventory (from GameManager)
        foreach (var itm in GameManager.Instance.GetInventory(playerIndex))
            AddInventoryButton(itm, invPanel, removable: true, titleLabel, priceLabel, descLabel, playerIndex);

        // Build shop buttons (buy)
        foreach (var item in GameManager.Instance.GetShop(playerIndex))
        {
            var btn = new Button { text = item.Name, focusable = true };
            btn.AddToClassList("button");

            // preview info
            AttachInfoHandlers(btn, item, titleLabel, priceLabel, descLabel, isBuy: true);

            // buy click
            btn.clicked += () =>
            {
                if (invPanel.childCount >= inventoryLimit) return;
                if (MoneyManager.Instance.GetMoneyAmount() < item.Price) return;

                MoneyManager.Instance.SubtractMoney(item.Price);
                UpdateMoneyLabel();

                // Add a fresh instance to inventory;
                // for ammo-like items, start Current at full Limit
                var toAdd = item;
                if (toAdd.Limit > 0) toAdd.Current = toAdd.Limit;

                GameManager.Instance.AddToInventory(playerIndex, toAdd);
                AddInventoryButton(toAdd, invPanel, removable: true, titleLabel, priceLabel, descLabel, playerIndex);
            };

            shopPanel.Add(btn);

            // show ammo badge in shop (non-interactive)
            if (item.Limit > 0)
            {
                var ammo = new Label($"{item.Limit}/{item.Limit}");
                ammo.AddToClassList("ammo-label");
                btn.Add(ammo);
            }
        }
    }

    // Create one inventory button with optional ammo label and sell behavior
    void AddInventoryButton(GameManager.ShopItem item, VisualElement invPanel, bool removable,
                            Label titleLabel, Label priceLabel, Label descLabel, int playerIndex)
    {
        var invBtn = new Button { text = item.Name, focusable = true };
        invBtn.AddToClassList("button");

        AttachInfoHandlers(invBtn, item, titleLabel, priceLabel, descLabel, isBuy: false);

        if (item.Limit > 0)
        {
            var ammo = new Label($"{item.Current}/{item.Limit}");
            ammo.AddToClassList("ammo-label");
            invBtn.Add(ammo);
        }

        if (removable)
        {
            invBtn.clicked += () =>
            {
                if (GameManager.Instance.TryRemoveFromInventory(playerIndex, item.Name, out var removed))
                {
                    invPanel.Remove(invBtn);
                    MoneyManager.Instance.AddMoney(removed.Price);
                    UpdateMoneyLabel();
                }
            };
        }

        invPanel.Add(invBtn);
    }

    // ---------- Shared helpers ----------
    void AttachInfoHandlers(VisualElement ve, GameManager.ShopItem item,
                            Label titleLabel, Label priceLabel, Label descLabel, bool isBuy)
    {
        System.Action update = () =>
        {
            if (titleLabel != null) titleLabel.text = item.Name;
            if (priceLabel != null)
                priceLabel.text = isBuy ? $"Buy For -${item.Price}" : $"Sell For +${item.Price}";
            if (descLabel != null) descLabel.text = item.Description;
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

        if (endPanel != null) endPanel.style.display = bothDone ? DisplayStyle.Flex : DisplayStyle.None;
        if (playerPanels != null) playerPanels.style.display = bothDone ? DisplayStyle.None : DisplayStyle.Flex;
        if (moneyPanel != null) moneyPanel.style.display = bothDone ? DisplayStyle.None : DisplayStyle.Flex;
    }

    void ResetToNormal()
    {
        if (done1 != null) done1.value = false;
        if (done2 != null) done2.value = false;
        UpdateEndState();
    }

    void LoadPrototypeScene() => SceneManager.LoadScene("Prototype"); // ensure in Build Settings
}
