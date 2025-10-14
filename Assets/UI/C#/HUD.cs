using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HUD : MonoBehaviour
{
    [SerializeField] private UIDocument ui;

    private const string kMoneyLabelName = "MoneyLabel";
    private const string kButtonsP1Container = "Button_Style_Test_1";
    private const string kButtonsP2Container = "Button_Style_Test_2";

    private const string kButtonClass = "inventory-button"; // your USS style for HUD buttons
    private const string kAmmoClass = "ammo-label";       // your USS badge for ammo/limit

    private void OnEnable()
    {
        BuildUI();
    }

    /// <summary>
    /// Call this after money or inventory changes to refresh the HUD.
    /// </summary>
    public void BuildUI()
    {
        if (ui == null || GameManager.Instance == null) return;

        var root = ui.rootVisualElement;

        // Update money
        var moneyLabel = root.Q<Label>(kMoneyLabelName);
        if (moneyLabel != null && MoneyManager.Instance != null)
            moneyLabel.text = $"{MoneyManager.Instance.GetMoneyAmount()} $";

        // Find the dedicated button containers (we do NOT touch your numbers/labels pack)
        var buttonsP1 = root.Q<VisualElement>(kButtonsP1Container);
        var buttonsP2 = root.Q<VisualElement>(kButtonsP2Container);

        PopulateButtons(buttonsP1, GameManager.Instance.GetInventory(1));
        PopulateButtons(buttonsP2, GameManager.Instance.GetInventory(2));
    }

    /// <summary>
    /// Fills the given container with inventory buttons ONLY (safe to Clear()).
    /// Your static number labels stay in their own container and are never cleared.
    /// </summary>
    private void PopulateButtons(VisualElement container, IReadOnlyList<GameManager.ShopItem> items)
    {
        if (container == null || items == null) return;

        // This container is dedicated to runtime buttons, so it's safe to clear it
        container.Clear();

        foreach (var item in items)
        {
            var btn = new Button { text = item.Name, focusable = true };
            btn.AddToClassList(kButtonClass);

            // Optional ammo/limit badge (e.g. "12/12")
            if (item.Limit > 0)
            {
                var ammo = new Label($"{item.Current}/{item.Limit}");
                ammo.AddToClassList(kAmmoClass);
                btn.Add(ammo);
            }

            container.Add(btn);
        }
    }
}
