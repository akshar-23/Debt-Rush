using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class HUD : MonoBehaviour
{
    [SerializeField] private UIDocument ui;

    public VisualElement bar1;       // left row
    public VisualElement bar2;       // right row

    public static class InventoryButtonStates
    {
        public const string Selected = "selected";
        public const string Normal = "normal";
        public const string Passive = "passive";
    }

    private void OnEnable()
    {
        if (ui == null) return;

        var root = ui.rootVisualElement;

        // Prefer Button_Style_Test_*; fall back to Inventory_*
        bar1 = root.Q<VisualElement>("Button_Style_Test_1") ?? root.Q<VisualElement>("Inventory_1");
        bar2 = root.Q<VisualElement>("Button_Style_Test_2") ?? root.Q<VisualElement>("Inventory_2");

        // Also guard at root so nothing slips through
        root.RegisterCallback<NavigationMoveEvent>(ev => ev.StopImmediatePropagation(), TrickleDown.TrickleDown);

        // Subscribe to money changes
        MoneyManager.OnMoneyChanged += UpdateMoneyDisplay;

        BuildUI();
    }

    private void OnDisable()
    {
        // Unsubscribe when disabled
        MoneyManager.OnMoneyChanged -= UpdateMoneyDisplay;
    }

    /// Call this when money or inventories change.
    public void BuildUI()
    {
        if (ui == null || GameManager.Instance == null) return;

        var root = ui.rootVisualElement;

        UpdateMoneyDisplay();

        // Inventories -> bars (keep fixed number labels, clear only buttons)
        PopulateBar(bar1, GameManager.Instance.GetInventory(1));
        PopulateBar(bar2, GameManager.Instance.GetInventory(2));
    }

    public void UpdateItemCount(int _id, ShopItem itemEquipped)
    {
        VisualElement bar = _id == 0 ? bar1 : bar2;
        
        if (bar == null || itemEquipped == null) return;

        var ammoLabel = bar.Q<Button>(itemEquipped.itemName)?.Q<Label>(className: "ammo-label");

        if (ammoLabel != null)
        {
            ammoLabel.text = $"{itemEquipped.currentCount}";
        }
    }

    // Update just the money label
    private void UpdateMoneyDisplay()
    {
        if (ui == null) return;

        var root = ui.rootVisualElement;
        var moneyLabel = root.Q<Label>("MoneyLabel");

        if (moneyLabel != null && MoneyManager.Instance != null)
            moneyLabel.text = $"{MoneyManager.Instance.GetMoneyAmount()} $";
    }

    // ---------- Helpers ----------

    // Keep fixed labels; remove only buttons
    private void ClearButtonsOnly(VisualElement bar)
    {
        if (bar == null) return;

        var remove = new List<VisualElement>();
        foreach (var child in bar.Children())
            if (child is Button) remove.Add(child);

        foreach (var c in remove) bar.Remove(c);
    }

    private void PopulateBar(VisualElement bar, IReadOnlyList<ShopItem> items)
    {
        if (bar == null || items == null) return;

        ClearButtonsOnly(bar);

        var sorted = items.OrderBy(i => i.isPassiveItem ? 0 : 1).ToList();
        foreach (var item in sorted)
        {
            var btn = new Button { focusable = true };
            btn.name = item.itemName;
            btn.tabIndex = -1;                       // keep it out of Tab focus order
            btn.AddToClassList("inventory-button");  // your HUD button style

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

            if (item.maxCount > 0)
            {
                var ammo = new Label($"{item.currentCount}");
                ammo.AddToClassList("ammo-label");
                btn.Add(ammo);
            }

            bar.Add(btn);

            if (item.isPassiveItem)
            {
                SetState(btn, InventoryButtonStates.Passive);
            }
        }
    }

    public void SetStateToIndex(int playerIndex, int position, string state)
    {
        VisualElement currentInventory = playerIndex == 1 ? bar1 : bar2;
        var inventory = GameManager.Instance.GetInventory(playerIndex);

        if (position < 0 || position >= inventory.Count) return;

        string itemName = inventory[position].itemName;
        var btn = currentInventory.Q<Button>(itemName);
        if (btn != null) SetState(btn, state);
    }

    private void SetState(VisualElement btn, string state)
    {
        btn.RemoveFromClassList(InventoryButtonStates.Selected);
        btn.RemoveFromClassList(InventoryButtonStates.Normal);
        btn.RemoveFromClassList(InventoryButtonStates.Passive);

        btn.AddToClassList(state); // "selected" / "normal" / "passive"
    }
}
