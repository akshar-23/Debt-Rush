using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HUD : MonoBehaviour
{
    [SerializeField] private UIDocument ui;

    private void OnEnable()
    {
        BuildUI();
    }

    // Call this after money or inventory changes to refresh the HUD
    public void BuildUI()
    {
        if (ui == null || GameManager.Instance == null) return;

        var root = ui.rootVisualElement;

        // Money
        var moneyLabel = root.Q<Label>("MoneyLabel");
        if (moneyLabel != null && MoneyManager.Instance != null)
            moneyLabel.text = $"{MoneyManager.Instance.GetMoneyAmount()} $";

        // Inventories
        var invPanel1 = root.Q<VisualElement>("Inventory_1");
        var invPanel2 = root.Q<VisualElement>("Inventory_2");

        PopulateInventory(invPanel1, GameManager.Instance.GetInventory(1));
        PopulateInventory(invPanel2, GameManager.Instance.GetInventory(2));
    }

    // NOTE: we now accept the rich item type from GameManager
    private void PopulateInventory(VisualElement panel, IReadOnlyList<GameManager.ShopItem> items)
    {
        if (panel == null || items == null) return;

        panel.Clear();

        foreach (var item in items)
        {
            // Main inventory button (keeps your existing .inventory-button style)
            var btn = new Button { text = item.Name, focusable = true };
            btn.AddToClassList("inventory-button");

            // Optional ammo/limit badge (e.g., "12/12") using your .ammo-label style
            if (item.Limit > 0)
            {
                var ammo = new Label($"{item.Current}/{item.Limit}");
                ammo.AddToClassList("ammo-label");
                btn.Add(ammo);
            }

            panel.Add(btn);
        }
    }
}
