using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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

    [SerializeField] IndividualObjective objectiveData;

    // Indie obj panel elements
    VisualElement receivedText1, receivedText2;
    Button indieContinueButton;
    string p1ChosenObjective, p2ChosenObjective;
    bool p1Chose, p2Chose;

    // Join panel elements
    VisualElement playerJoinPanel;
    Label joinText1, joinText2;
    Button joinContinueButton;

    void OnEnable()
    {
        var root = ui.rootVisualElement;

        // Setup join panel
        playerJoinPanel   = root.Q<VisualElement>("PlayerJoinPanel");
        joinText1         = root.Q<Label>("Join_Text_1");
        joinText2         = root.Q<Label>("Join_Text_2");
        joinContinueButton = root.Q<VisualElement>("Button_Container")?.Q<Button>();

        if (joinContinueButton != null)
        {
            joinContinueButton.style.display = DisplayStyle.None;
            joinContinueButton.clicked += OnJoinContinue;
        }

        modePanel     = root.Q<VisualElement>("ModePanel");
        indieObjPanel = root.Q<VisualElement>("IndieObjPanel");
        receivedText1      = root.Q<VisualElement>("Received_Text_1");
        receivedText2      = root.Q<VisualElement>("Received_Text_2");
        indieContinueButton = root.Q<Button>("Continue_Button");
        if (indieContinueButton != null)
        {
            indieContinueButton.style.display = DisplayStyle.None;
            indieContinueButton.clicked += OnDoneChoosingGoals;
        }
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

        // Hook into ShopJoinManager events
        if (ShopJoinManager.Instance != null)
        {
            ShopJoinManager.Instance.OnPlayer1Joined += OnP1Joined;
            ShopJoinManager.Instance.OnPlayer2Joined += OnP2Joined;
        }

        // Show join panel first, hide everything else
        ShowJoinPanel();
    }

    void OnDisable()
    {
        if (returnButton != null)      returnButton.clicked      -= ResetToNormal;
        if (continueButton != null)    continueButton.clicked    -= LoadPrototypeScene;
        if (noSecretsButton != null)   noSecretsButton.clicked   -= OnNoSecrets;
        if (someSecretsButton != null) someSecretsButton.clicked -= OnSomeSecrets;
        if (joinContinueButton != null)   joinContinueButton.clicked   -= OnJoinContinue;
        if (indieContinueButton != null)  indieContinueButton.clicked  -= OnDoneChoosingGoals;

        if (ShopJoinManager.Instance != null)
        {
            ShopJoinManager.Instance.OnPlayer1Joined -= OnP1Joined;
            ShopJoinManager.Instance.OnPlayer2Joined -= OnP2Joined;
        }
    }

    void ShowModeSelection()
    {
        if (modePanel != null)     modePanel.style.display     = DisplayStyle.Flex;
        // Re-register callbacks every time panel is shown
        // because UI Toolkit drops clicked listeners when display:none is set
        if (noSecretsButton != null)
        {
            noSecretsButton.clicked -= OnNoSecrets;
            noSecretsButton.clicked += OnNoSecrets;
        }
        if (someSecretsButton != null)
        {
            someSecretsButton.clicked -= OnSomeSecrets;
            someSecretsButton.clicked += OnSomeSecrets;
        }
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

    void SetObjectiveTexts()
    {
        var root = ui.rootVisualElement;
        var source = objectiveData != null
            ? objectiveData.objectives
            : new System.Collections.Generic.List<string>();

        string[] p1Names = { "obj_1_1", "obj_1_2", "obj_1_3", "obj_1_4" };
        string[] p2Names = { "obj_2_1", "obj_2_2", "obj_2_3", "obj_2_4" };

        var p1 = PickRandom4(source);
        var p2 = PickRandom4(source);

        for (int i = 0; i < 4; i++)
        {
            SetObjText(root, p1Names[i], p1[i]);
            SetObjText(root, p2Names[i], p2[i]);
        }
    }

    string[] PickRandom4(System.Collections.Generic.List<string> source)
    {
        var shuffled = new System.Collections.Generic.List<string>(source);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }
        string[] result = new string[4];
        for (int i = 0; i < 4; i++)
            result[i] = i < shuffled.Count ? shuffled[i] : "";
        return result;
    }

    void SetObjText(VisualElement root, string objName, string text)
    {
        var obj = root.Q<VisualElement>(objName);
        var labels = obj?.Query<Label>().ToList();
        // Index 1 is the objective label (index 0 is the hint letter inside HintBox)
        if (labels != null && labels.Count > 1) labels[1].text = text;
    }

    void OnSomeSecrets()
    {
        if (modePanel != null)     modePanel.style.display     = DisplayStyle.None;
        if (indieObjPanel != null) indieObjPanel.style.display = DisplayStyle.Flex;
        if (playerPanels != null)  playerPanels.style.display  = DisplayStyle.None;
        if (moneyPanel != null)    moneyPanel.style.display    = DisplayStyle.None;

        // Reset state
        p1Chose = false;
        p2Chose = false;
        p1ChosenObjective = "";
        p2ChosenObjective = "";
        if (receivedText1 != null) receivedText1.style.display = DisplayStyle.None;
        if (receivedText2 != null) receivedText2.style.display = DisplayStyle.None;
        if (indieContinueButton != null) indieContinueButton.style.display = DisplayStyle.None;
        var oc1 = ui.rootVisualElement.Q<VisualElement>("ObjectivesContainer_1");
        var oc2 = ui.rootVisualElement.Q<VisualElement>("ObjectivesContainer_2");
        if (oc1 != null) oc1.style.display = DisplayStyle.Flex;
        if (oc2 != null) oc2.style.display = DisplayStyle.Flex;

        SetObjectiveTexts();
    }

    void OnDoneChoosingGoals()
    {
        if (indieObjPanel != null) indieObjPanel.style.display = DisplayStyle.None;
        ShowShop();
    }

    void CheckBothChose()
    {
        if (!p1Chose || !p2Chose) return;
        Debug.Log($"Player 1 chose: {p1ChosenObjective}");
        Debug.Log($"Player 2 chose: {p2ChosenObjective}");
        if (indieContinueButton != null) indieContinueButton.style.display = DisplayStyle.Flex;
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

        if (titleLabel != null) titleLabel.text = "";
        if (priceLabel != null) priceLabel.text = "";
        if (descLabel != null)  descLabel.text  = "";

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
                var btn = new Button { focusable = true };
                if (item.icon != null)
                {
                    var img = new Image { sprite = item.icon };
                    img.AddToClassList("item-icon");
                    btn.Add(img);
                }
                else
                {
                    btn.text = item.itemName;
                }
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
        var invBtn = new Button { focusable = true };
        if (item.icon != null)
        {
            var img = new Image { sprite = item.icon };
            img.AddToClassList("item-icon");
            invBtn.Add(img);
        }
        else
        {
            invBtn.text = item.itemName;
        }
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
                    // Focus adjacent button before removing so selection doesn't get lost
                    var siblings = invPanel.Query<Button>().ToList();
                    int idx = siblings.IndexOf(invBtn);
                    Button nextFocus = null;
                    if (idx + 1 < siblings.Count) nextFocus = siblings[idx + 1];
                    else if (idx - 1 >= 0)         nextFocus = siblings[idx - 1];

                    invPanel.Remove(invBtn);
                    MoneyManager.Instance.AddMoney(removed.itemPrice);
                    UpdateMoneyLabel();
                    UpdateCapacityLabels();

                    // If inventory is now empty, focus first shop item instead
                    if (nextFocus == null)
                    {
                        var shopPanel = invPanel.parent?.Q<VisualElement>(
                            playerIndex == 1 ? "ShopList_1" : "ShopList_2");
                        nextFocus = shopPanel?.Q<Button>();
                    }
                    nextFocus?.Focus();
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
                priceLabel.text = isBuy ? $"Buy For ${item.itemPrice}" : $"Sell For ${item.itemPrice}";
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

    void ShowJoinPanel()
    {
        if (playerJoinPanel != null) playerJoinPanel.style.display = DisplayStyle.Flex;
        if (modePanel != null)       modePanel.style.display       = DisplayStyle.None;
        if (indieObjPanel != null)   indieObjPanel.style.display   = DisplayStyle.None;
        if (playerPanels != null)    playerPanels.style.display    = DisplayStyle.None;
        if (moneyPanel != null)      moneyPanel.style.display      = DisplayStyle.None;
        if (endPanel != null)        endPanel.style.display        = DisplayStyle.None;
    }

    void OnP1Joined()
    {
        if (joinText1 != null) joinText1.text = "Joined!";
        CheckBothJoined();
    }

    void OnP2Joined()
    {
        if (joinText2 != null) joinText2.text = "Joined!";
        CheckBothJoined();
    }

    void CheckBothJoined()
    {
        if (ShopJoinManager.Instance != null &&
            ShopJoinManager.Instance.player1Joined &&
            ShopJoinManager.Instance.player2Joined)
        {
            if (joinContinueButton != null)
                joinContinueButton.style.display = DisplayStyle.Flex;
        }
    }

    void OnJoinContinue()
    {
        if (playerJoinPanel != null) playerJoinPanel.style.display = DisplayStyle.None;
        ShowModeSelection();
    }

    void LoadPrototypeScene() => SceneManager.LoadScene("Prototype");

    void HandleIndieObjInput()
    {
        // Get which button each player pressed: A/B/X/Y = Submit/Cancel/Interact/North
        // and map to the corresponding objective slot index (0=A, 1=B, 2=X, 3=Y)

        // P1 buttons: Space=A(0), C=B(1), E=X(2), Q=Y(3)
        // P2 buttons: RightCtrl=A(0), NumpadPeriod=B(1), Numpad0=X(2), Numpad1=Y(3)
        // Gamepad P1: ButtonSouth=A, ButtonEast=B, ButtonWest=X, ButtonNorth=Y
        // Gamepad P2: same

        if (!p1Chose)
        {
            int p1Slot = -1;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame)      p1Slot = 0;
                else if (Keyboard.current.cKey.wasPressedThisFrame)     p1Slot = 1;
                else if (Keyboard.current.eKey.wasPressedThisFrame)     p1Slot = 2;
                else if (Keyboard.current.qKey.wasPressedThisFrame)     p1Slot = 3;
            }
            var gp1 = GameManager.Instance.playerGamepads[0];
            if (p1Slot < 0 && gp1 != null)
            {
                if (gp1.buttonSouth.wasPressedThisFrame)  p1Slot = 0;
                else if (gp1.buttonEast.wasPressedThisFrame)  p1Slot = 1;
                else if (gp1.buttonWest.wasPressedThisFrame)  p1Slot = 2;
                else if (gp1.buttonNorth.wasPressedThisFrame) p1Slot = 3;
            }
            if (p1Slot >= 0)
            {
                p1ChosenObjective = GetObjText(ui.rootVisualElement, p1Slot, 1);
                p1Chose = true;
                var objContainer1 = ui.rootVisualElement.Q<VisualElement>("ObjectivesContainer_1");
                if (objContainer1 != null) objContainer1.style.display = DisplayStyle.None;
                if (receivedText1 != null) receivedText1.style.display = DisplayStyle.Flex;
                CheckBothChose();
            }
        }

        if (!p2Chose)
        {
            int p2Slot = -1;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.rightCtrlKey.wasPressedThisFrame)      p2Slot = 0;
                else if (Keyboard.current.numpadPeriodKey.wasPressedThisFrame) p2Slot = 1;
                else if (Keyboard.current.numpad0Key.wasPressedThisFrame)   p2Slot = 2;
                else if (Keyboard.current.numpad1Key.wasPressedThisFrame)   p2Slot = 3;
            }
            var gp2 = GameManager.Instance.playerGamepads[1];
            if (p2Slot < 0 && gp2 != null)
            {
                if (gp2.buttonSouth.wasPressedThisFrame)  p2Slot = 0;
                else if (gp2.buttonEast.wasPressedThisFrame)  p2Slot = 1;
                else if (gp2.buttonWest.wasPressedThisFrame)  p2Slot = 2;
                else if (gp2.buttonNorth.wasPressedThisFrame) p2Slot = 3;
            }
            if (p2Slot >= 0)
            {
                p2ChosenObjective = GetObjText(ui.rootVisualElement, p2Slot, 2);
                p2Chose = true;
                var objContainer2 = ui.rootVisualElement.Q<VisualElement>("ObjectivesContainer_2");
                if (objContainer2 != null) objContainer2.style.display = DisplayStyle.None;
                if (receivedText2 != null) receivedText2.style.display = DisplayStyle.Flex;
                CheckBothChose();
            }
        }
    }

    string GetObjText(VisualElement root, int slot, int playerIndex)
    {
        string[] p1Names = { "obj_1_1", "obj_1_2", "obj_1_3", "obj_1_4" };
        string[] p2Names = { "obj_2_1", "obj_2_2", "obj_2_3", "obj_2_4" };
        string objName = playerIndex == 1 ? p1Names[slot] : p2Names[slot];
        var obj = root.Q<VisualElement>(objName);
        var labels = obj?.Query<Label>().ToList();
        return (labels != null && labels.Count > 1) ? labels[1].text : "";
    }

    void Update()
    {
        // Handle indie objective selection
        if (indieObjPanel != null && indieObjPanel.style.display == DisplayStyle.Flex)
        {
            HandleIndieObjInput();
            return;
        }

        // Only active when shop panel is visible
        if (playerPanels == null || playerPanels.style.display == DisplayStyle.None) return;

        // P1: E key or gamepad 0 ButtonWest
        bool p1Interact = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        if (!p1Interact && GameManager.Instance.playerGamepads[0] != null)
            p1Interact = GameManager.Instance.playerGamepads[0].buttonWest.wasPressedThisFrame;

        // P2: Numpad0 or gamepad 1 ButtonWest
        bool p2Interact = Keyboard.current != null && Keyboard.current.numpad0Key.wasPressedThisFrame;
        if (!p2Interact && GameManager.Instance.playerGamepads[1] != null)
            p2Interact = GameManager.Instance.playerGamepads[1].buttonWest.wasPressedThisFrame;

        if (p1Interact && done1 != null) done1.value = !done1.value;
        if (p2Interact && done2 != null) done2.value = !done2.value;
    }
}
