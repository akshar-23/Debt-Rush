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

        // Disable built-in nav just for these bars
        DisableBuiltInNavFor(bar1);
        DisableBuiltInNavFor(bar2);

        // Also guard at root so nothing slips through
        root.RegisterCallback<NavigationMoveEvent>(ev => ev.StopImmediatePropagation(), TrickleDown.TrickleDown);
        root.RegisterCallback<KeyDownEvent>(ev =>
        {
            // swallow default nav keys
            switch (ev.keyCode)
            {
                case KeyCode.Tab:
                case KeyCode.LeftArrow:
                case KeyCode.RightArrow:
                case KeyCode.UpArrow:
                case KeyCode.DownArrow:
                case KeyCode.W:
                case KeyCode.A:
                case KeyCode.S:
                case KeyCode.D:
                    ev.StopImmediatePropagation();
                    break;
            }
        }, TrickleDown.TrickleDown);

        // Global number keys:
        // - Row 1–6 => left bar
        // - Numpad 1–6 => right bar
        root.RegisterCallback<KeyDownEvent>(OnRootKeyDown, TrickleDown.TrickleDown);

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
            var btn = new Button { text = item.Name, focusable = true };
            btn.tabIndex = -1;                       // keep it out of Tab focus order
            btn.AddToClassList("inventory-button");  // your HUD button style

            if (item.Limit > 0)
            {
                var ammo = new Label($"{item.Current}/{item.Limit}");
                ammo.AddToClassList("ammo-label");
                btn.Add(ammo);
            }

            bar.Add(btn);
        }
    }

    // Row 1–6 -> LEFT bar; Numpad 1–6 -> RIGHT bar
    private void OnRootKeyDown(KeyDownEvent e)
    {
        int index = -1;
        VisualElement target = null;

        switch (e.keyCode)
        {
            // Row numbers -> left
            case KeyCode.Alpha1: index = 0; target = bar1; break;
            case KeyCode.Alpha2: index = 1; target = bar1; break;
            case KeyCode.Alpha3: index = 2; target = bar1; break;
            case KeyCode.Alpha4: index = 3; target = bar1; break;
            case KeyCode.Alpha5: index = 4; target = bar1; break;
            case KeyCode.Alpha6: index = 5; target = bar1; break;

            // Numpad numbers -> right
            case KeyCode.Keypad1: index = 0; target = bar2; break;
            case KeyCode.Keypad2: index = 1; target = bar2; break;
            case KeyCode.Keypad3: index = 2; target = bar2; break;
            case KeyCode.Keypad4: index = 3; target = bar2; break;
            case KeyCode.Keypad5: index = 4; target = bar2; break;
            case KeyCode.Keypad6: index = 5; target = bar2; break;

            default: return; // not our keys
        }

        if (target == null) { e.StopImmediatePropagation(); return; }

        // Focus Nth button (ignore fixed labels)
        int seen = 0;
        foreach (var child in target.Children())
        {
            if (child is Button b)
            {
                if (seen == index)
                {
                    b.Focus(); // triggers :focus for your focused style
                    e.StopImmediatePropagation();
                    return;
                }
                seen++;
            }
        }

        // Even if there isn't a button that far, swallow so it never becomes nav
        e.StopImmediatePropagation();
    }

    // Block arrow/WASD/Tab navigation for this container
    private void DisableBuiltInNavFor(VisualElement container)
    {
        if (container == null) return;

        container.RegisterCallback<NavigationMoveEvent>(ev =>
        {
            ev.StopImmediatePropagation();
        }, TrickleDown.TrickleDown);

        container.RegisterCallback<KeyDownEvent>(ev =>
        {
            switch (ev.keyCode)
            {
                case KeyCode.Tab:
                case KeyCode.LeftArrow:
                case KeyCode.RightArrow:
                case KeyCode.UpArrow:
                case KeyCode.DownArrow:
                case KeyCode.W:
                case KeyCode.A:
                case KeyCode.S:
                case KeyCode.D:
                    ev.StopImmediatePropagation();
                    break;
            }
        }, TrickleDown.TrickleDown);
    }
}
