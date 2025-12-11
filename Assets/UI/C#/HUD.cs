using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HUD : MonoBehaviour
{
    [SerializeField] private UIDocument ui;

    private VisualElement bar1;       // left row
    private VisualElement bar2;       // right row

    private void OnEnable()
    {
        if (ui == null) return;

        var root = ui.rootVisualElement;

        // Prefer Button_Style_Test_*; fall back to Inventory_*
        bar1 = root.Q<VisualElement>("Button_Style_Test_1") ?? root.Q<VisualElement>("Inventory_1");
        bar2 = root.Q<VisualElement>("Button_Style_Test_2") ?? root.Q<VisualElement>("Inventory_2");



        // Also guard at root so nothing slips through
        root.RegisterCallback<NavigationMoveEvent>(ev => ev.StopImmediatePropagation(), TrickleDown.TrickleDown);


        BuildUI();
    }

    /// Call this when money or inventories change.
    public void BuildUI()
    {
        if (ui == null || GameManager.Instance == null) return;

        var root = ui.rootVisualElement;

        // Money
        var moneyLabel = root.Q<Label>("MoneyLabel");
        if (moneyLabel != null && MoneyManager.Instance != null)
            moneyLabel.text = $"{MoneyManager.Instance.GetMoneyAmount()} $";

        // Inventories -> bars (keep fixed number labels, clear only buttons)
        PopulateBar(bar1, GameManager.Instance.GetInventory(1));
        PopulateBar(bar2, GameManager.Instance.GetInventory(2));
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

        foreach (var item in items)
        {
            var btn = new Button { text = item.itemName, focusable = true };
            btn.tabIndex = -1;                       // keep it out of Tab focus order
            btn.AddToClassList("inventory-button");  // your HUD button style

            if (item.maxCount > 0)
            {
                var ammo = new Label($"{item.currentCount}/{item.maxCount}");
                ammo.AddToClassList("ammo-label");
                btn.Add(ammo);
            }

            bar.Add(btn);
        }
    }


}




