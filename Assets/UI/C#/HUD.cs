using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HUD : MonoBehaviour
{
    [SerializeField] UIDocument ui;

    void OnEnable()
    {
        BuildUI();
    }

    
    public void BuildUI()
    {
        if (ui == null) return;

        var root = ui.rootVisualElement;

        // Money
        var moneyLabel = root.Q<Label>("MoneyLabel");
        if (moneyLabel != null && MoneyManager.Instance != null)
            moneyLabel.text = $"{MoneyManager.Instance.GetMoneyAmount()} $";

        // Inventories
        var invPanel1 = root.Q<VisualElement>("Inventory_1");
        var invPanel2 = root.Q<VisualElement>("Inventory_2");

        PopulateInventory(invPanel1, GameManager.Instance?.GetInventory(1));
        PopulateInventory(invPanel2, GameManager.Instance?.GetInventory(2));
    }

    void PopulateInventory(VisualElement panel, IReadOnlyList<string> items)
    {
        if (panel == null || items == null) return;

        panel.Clear();

        foreach (var name in items)
        {
            var btn = new Button { text = name, focusable = true };
            btn.AddToClassList("inventory-button"); // uses your USS style
            panel.Add(btn);
        }
    }
}
