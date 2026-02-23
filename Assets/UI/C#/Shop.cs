using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class Shop_UI : MonoBehaviour
{
    int inventoryLimit => GameManager.Instance.inventoryLimit;
    [SerializeField] int initialMoney = 1500;
    [SerializeField] UIDocument ui;

    Toggle done1, done2;
    Label moneyLabel;
    VisualElement endPanel, playerPanels, moneyPanel, modePanel, indieObjPanel;
    Button returnButton, continueButton, noSecretsButton, someSecretsButton;

    Label capacityLabel1, capacityLabel2;

    void OnEnable()
    {
        var root = ui.rootVisualElement;

        modePanel     = root.Q<VisualElement>("ModePanel");
        indieObjPanel = root.Q<VisualElement>("IndieObjPanel");
        noSecretsButton   = root.Q<Button>("ModeButton_1");
        someSecretsButton = root.Q<Button>("ModeButton_2");

        if (noSecretsButton != null)   noSecretsButton.clicked   += OnNoSecrets;
        if (someSecretsButton != null) someSecretsButton.clicked += OnSomeSecrets;

        SetupSide(root, 1, "ShopList_1", "Inventory_1");
        SetupSide(root, 2, "ShopList_2", "Inventory_2");

        moneyLabel = root.Q<Label>("MoneyLabel");
        MoneyManager.Instance.AddMoney(initialMoney);
        UpdateMoneyLabel();

        done1 = root.Q<Toggle>("DoneToggle_1");
        done2 = root.Q<Toggle>("DoneToggle_2");

        endPanel     = root.Q<VisualElement>("EndPanel");
        playerPanels = root.Q<VisualElement>("PlayerPanels");
        moneyPanel   = root.Q<VisualElement>("MoneyPanel");

        if (endPanel != null)
        {
            returnButton   = endPanel.Q<Button>("ReturnButton");
            continueButton = endPanel.Q<Button>("ContinueButton");
        }

        if (done1 != null) done1.RegisterValueChangedCallback(_ => UpdateEndState());
        if (done2 != null) done2.RegisterValueChangedCallback(_ => UpdateEndState());
        if (returnButton != null)   returnButton.clicked   += ResetToNormal;
        if (continueButton != null) continueButton.clicked += LoadPrototypeScene;

        UpdateCapacityLabels();
        ShowModeSelection();
    }

    void OnDisable()
    {
        if (returnButton != null)      returnButton.clicked      -= ResetToNormal;
        if (continueButton != null)    continueButton.clicked    -= LoadPrototypeScene;
        if (noSecretsButton != null)   noSecretsButton.clicked   -= OnNoSecrets;
        if (someSecretsButton != null) someSecretsButton.clicked -= OnSomeSecrets;
    }

    void ShowModeSelection()
    {
        if (modePanel != null)     modePanel.style.display     = DisplayStyle.Flex;
        if (indieObjPanel != null) indieObjPanel.style.display = DisplayStyle.None;
        if (playerPanels != null)  playerPanels.style.display  = DisplayStyle.None;
        if (moneyPanel != null)    moneyPanel.style.display    = DisplayStyle.None;
    }

    void OnNoSecrets()
    {
        if (modePanel != null)     modePanel.style.display     = DisplayStyle.None;
        if (indieObjPanel != null) indieObjPanel.style.display = DisplayStyle.None;
        ShowShop();
    }

    void OnSomeSecrets()
    {
        if (modePanel != null)     modePanel.style.display     = DisplayStyle.None;
        if (indieObjPanel != null) indieObjPanel.style.display = DisplayStyle.Flex;
        if (playerPanels != null)  playerPanels.style.display  = DisplayStyle.None;
        if (moneyPanel != null)    moneyPanel.style.display    = DisplayStyle.None;

        foreach (var btn in indieObjPanel.Query<Button>().ToList())
            btn.clicked += OnDoneChoosingGoals;
    }

    void OnDoneChoosingGoals()
    {
        foreach (var btn in indieObjPanel.Query<Button>().ToList())
            btn.clicked -= OnDoneChoosingGoals;

        if (indieObjPanel != null) indieObjPanel.style.display = DisplayStyle.None;
        ShowShop();
    }

    void ShowShop()
    {
        if (playerPanels != null) playerPanels.style.display = DisplayStyle.Flex;
        if (moneyPanel != null)   moneyPanel.style.display   = DisplayStyle.Flex;
        UpdateEndState();
    }

    void SetupSide(VisualElement root, int playerIndex, string shopPanelName, string invPanelName)
    {
        var shopPanel = root.Q<VisualElement>(shopPanelName);
        var invPanel  = root.Q<VisualElement>(invPanelName);

        shopPanel?.Clear();
        invPanel?.Clear();

        var capLabel = new Label();
        capLabel.AddToClassList("capacity-labels");
        if (invPanel != null) invPanel.Add(capLabel);
        if (playerIndex == 1) capacityLabel1 = capLabel;
        else capacityLabel2 = capLabel;

        var playerRoot = shopPanel?.parent?.parent ?? shopPanel?.parent;
        var titleLabel = playerRoot?.Q<Label>(className: "description-title");
        var priceLabel = playerRoot?.Q<Label>(className: "price-text");
        var descLabel  = playerRoot?.Q<Label>(className: "description-text");

        var invList = GameManager.Instance.GetInventory(playerIndex);
        if (invPanel != null && invList != null)
        {
            foreach (var itm in invList)
                AddInventoryButton(itm, invPanel, removable: true, titleLabel, priceLabel, descLabel, playerIndex);
        }

        var shopList = GameManager.Instance.GetShop(playerIndex);
        if (shopPanel != null && shopList != null)
        {
            foreach (var item in shopList)
            {
                var btn = new Button { text = item.itemName, focusable = true };
                btn.AddToClassList("button");

                AttachInfoHandlers(btn, item, titleLabel, priceLabel, descLabel, isBuy: true);

                btn.clicked += () =>
                {
                    if (invPanel == null) return;
                    if (invPanel.childCount - 1 >= inventoryLimit) return;
                    if (MoneyManager.Instance.GetMoneyAmount() < item.itemPrice) return;

                    MoneyManager.Instance.SubtractMoney(item.itemPrice);
                    UpdateMoneyLabel();

                    var toAdd = item;
                    if (toAdd.maxCount > 0) toAdd.currentCount = toAdd.maxCount;

                    GameManager.Instance.AddToInventory(playerIndex, toAdd);
                    AddInventoryButton(toAdd, invPanel, removable: true, titleLabel, priceLabel, descLabel, playerIndex);

                    UpdateCapacityLabels();
                };

                shopPanel.Add(btn);

                if (item.maxCount > 0)
                {
                    var ammo = new Label($"{item.maxCount}");
                    ammo.AddToClassList("ammo-label");
                    btn.Add(ammo);
                }
            }
        }
    }

    void AddInventoryButton(ShopItem item, VisualElement invPanel, bool removable,
                            Label titleLabel, Label priceLabel, Label descLabel, int playerIndex)
    {
        var invBtn = new Button { text = item.itemName, focusable = true };
        invBtn.AddToClassList("button");

        AttachInfoHandlers(invBtn, item, titleLabel, priceLabel, descLabel, isBuy: false);

        if (item.maxCount > 0)
        {
            var ammo = new Label($"{item.currentCount}");
            ammo.AddToClassList("ammo-label");
            invBtn.Add(ammo);
        }

        if (removable)
        {
            invBtn.clicked += () =>
            {
                if (GameManager.Instance.TryRemoveFromInventory(playerIndex, item.itemName, out var removed))
                {
                    invPanel.Remove(invBtn);
                    MoneyManager.Instance.AddMoney(removed.itemPrice);
                    UpdateMoneyLabel();
                    UpdateCapacityLabels();
                }
            };
        }

        invPanel.Add(invBtn);
    }

    void AttachInfoHandlers(VisualElement ve, ShopItem item,
                            Label titleLabel, Label priceLabel, Label descLabel, bool isBuy)
    {
        System.Action update = () =>
        {
            if (titleLabel != null) titleLabel.text = item.itemName;
            if (priceLabel != null)
                priceLabel.text = isBuy ? $"Buy For -${item.itemPrice}" : $"Sell For +${item.itemPrice}";
            if (descLabel != null) descLabel.text = item.description;
        };

        ve.RegisterCallback<MouseEnterEvent>(_ => update());
        ve.RegisterCallback<FocusInEvent>(_ => update());
    }

    void UpdateMoneyLabel()
    {
        if (moneyLabel != null)
            moneyLabel.text = $"{MoneyManager.Instance.GetMoneyAmount()} $";
    }

    void UpdateCapacityLabels()
    {
        int count1 = GameManager.Instance.GetInventory(1).Count;
        int count2 = GameManager.Instance.GetInventory(2).Count;

        if (capacityLabel1 != null) capacityLabel1.text = $"{count1}/{inventoryLimit}";
        if (capacityLabel2 != null) capacityLabel2.text = $"{count2}/{inventoryLimit}";
    }

    void UpdateEndState()
    {
        bool bothDone = (done1 != null && done1.value) && (done2 != null && done2.value);

        if (endPanel != null)     endPanel.style.display     = bothDone ? DisplayStyle.Flex : DisplayStyle.None;
        if (playerPanels != null) playerPanels.style.display = bothDone ? DisplayStyle.None : DisplayStyle.Flex;
        if (moneyPanel != null)   moneyPanel.style.display   = bothDone ? DisplayStyle.None : DisplayStyle.Flex;
    }

    void ResetToNormal()
    {
        if (done1 != null) done1.value = false;
        if (done2 != null) done2.value = false;
        UpdateEndState();
    }

    void LoadPrototypeScene() => SceneManager.LoadScene("Prototype");
}
