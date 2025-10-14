using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class Shop_UI : MonoBehaviour
{
    [SerializeField] int inventoryLimit = 12;
    [SerializeField] int initialMoney = 1500;
    [SerializeField] UIDocument ui;

    Toggle done1, done2;
    Label moneyLabel;
    VisualElement endPanel, playerPanels, moneyPanel;
    Button returnButton, continueButton;

    // The capacity labels that live as the FIRST child of the inventory panels
    Label capacityLabel1, capacityLabel2;

    void OnEnable()
    {
        var root = ui.rootVisualElement;

        // Build both sides from GameManager data
        SetupSide(root, 1, "ShopList_1", "Inventory_1");
        SetupSide(root, 2, "ShopList_2", "Inventory_2");

        // Money
        moneyLabel = root.Q<Label>("MoneyLabel");
        MoneyManager.Instance.AddMoney(initialMoney);
        UpdateMoneyLabel();

        // Toggles & panels
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

        UpdateCapacityLabels();   // make sure text is correct on first frame
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

        shopPanel?.Clear();
        invPanel?.Clear();

        // --- create the capacity label FIRST inside the inventory panel ---
     

        var capLabel = new Label();
        capLabel.AddToClassList("capacity-labels"); 
        if (invPanel != null)
            invPanel.Insert(0, capLabel);
        if (playerIndex == 1) capacityLabel1 = capLabel;
        else capacityLabel2 = capLabel;

        // Info labels for this player column
        var playerRoot = shopPanel?.parent?.parent ?? shopPanel?.parent;
        var titleLabel = playerRoot?.Q<Label>(className: "description-title");
        var priceLabel = playerRoot?.Q<Label>(className: "price-text");
        var descLabel = playerRoot?.Q<Label>(className: "description-text");

        // Preload inventory (from GameManager) AFTER the capacity label
        var invList = GameManager.Instance.GetInventory(playerIndex);
        if (invPanel != null && invList != null)
        {
            foreach (var itm in invList)
                AddInventoryButton(itm, invPanel, removable: true, titleLabel, priceLabel, descLabel, playerIndex);
        }

        // Build shop buttons (buy)
        var shopList = GameManager.Instance.GetShop(playerIndex);
        if (shopPanel != null && shopList != null)
        {
            foreach (var item in shopList)
            {
                var btn = new Button { text = item.Name, focusable = true };
                btn.AddToClassList("button");

                // Preview info
                AttachInfoHandlers(btn, item, titleLabel, priceLabel, descLabel, isBuy: true);

                // Buy click
                btn.clicked += () =>
                {
                    if (invPanel == null) return;
                    if (invPanel.childCount - 1 >= inventoryLimit) return; // -1 because capacity label occupies index 0
                    if (MoneyManager.Instance.GetMoneyAmount() < item.Price) return;

                    MoneyManager.Instance.SubtractMoney(item.Price);
                    UpdateMoneyLabel();

                    // Add a fresh instance to inventory (set Current to full Limit if applicable)
                    var toAdd = item;
                    if (toAdd.Limit > 0) toAdd.Current = toAdd.Limit;

                    GameManager.Instance.AddToInventory(playerIndex, toAdd);
                    AddInventoryButton(toAdd, invPanel, removable: true, titleLabel, priceLabel, descLabel, playerIndex);

                    UpdateCapacityLabels();
                };

                shopPanel.Add(btn);

                // Ammo badge in shop (non-interactive)
                if (item.Limit > 0)
                {
                    var ammo = new Label($"{item.Limit}/{item.Limit}");
                    ammo.AddToClassList("ammo-label");
                    btn.Add(ammo);
                }
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
                    UpdateCapacityLabels();
                }
            };
        }

        invPanel.Add(invBtn); // will be after the capacity label because that was inserted first
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
        if (moneyLabel != null)
            moneyLabel.text = $"{MoneyManager.Instance.GetMoneyAmount()} $";
    }

    // shows remaining/limit for both players; NOTE: inventory count excludes the capacity label (already handled above)
    void UpdateCapacityLabels()
    {
        int count1 = GameManager.Instance.GetInventory(1).Count;
        int count2 = GameManager.Instance.GetInventory(2).Count;

        int remaining1 = Mathf.Max(0, inventoryLimit - count1);
        int remaining2 = Mathf.Max(0, inventoryLimit - count2);

        if (capacityLabel1 != null) capacityLabel1.text = $"{remaining1}/{inventoryLimit}";
        if (capacityLabel2 != null) capacityLabel2.text = $"{remaining2}/{inventoryLimit}";
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

    void LoadPrototypeScene() => SceneManager.LoadScene("Prototype");
}
