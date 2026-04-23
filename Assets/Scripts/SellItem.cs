using UnityEditor.VersionControl;
using UnityEngine;

public class SellItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private ShopItem weaponP1;
    [SerializeField] private ShopItem weaponP2;
    [SerializeField] private float priceMultiplier = 1.2f;

    [Header("Messages")]
    [SerializeField] private string successMessage = "Thank you kind stranger...";
    [SerializeField] private string insufficentFundsMessage = "Sorry, you don't have enough money...";
    [SerializeField] private string noInventorySpaceMessage = "Seems you can't carry more items, you could come back later...";




    public string GetItemName(PlayerController currentController)
    {
        string itemNameText = "";

        if (currentController.playerNumber == 1 && weaponP1 != null) itemNameText = weaponP1.itemName;
        if (currentController.playerNumber == 2 && weaponP2 != null) itemNameText = weaponP2.itemName;

        return itemNameText;
    }
    public string GetItemPrice(PlayerController currentController)
    {
        string itemPriceText = "";

        if (currentController.playerNumber == 1 && weaponP1 != null) itemPriceText = Mathf.RoundToInt(weaponP1.itemPrice * priceMultiplier).ToString();
        if (currentController.playerNumber == 2 && weaponP2 != null) itemPriceText = Mathf.RoundToInt(weaponP2.itemPrice * priceMultiplier).ToString();

        return itemPriceText;
    }

    public string TryToSellItem(PlayerController currentController)
    {
        bool success = false;
        string message = "...";
        int itemCost = 0;
        ShopItem weapon = null;

        if (currentController == null) return message;

        if (currentController.playerNumber == 1 && weaponP1 != null)
        {
            itemCost = Mathf.RoundToInt(weaponP1.itemPrice * priceMultiplier);
            weapon = weaponP1;
        }
        else if (currentController.playerNumber == 2 && weaponP2 != null)
        {
            itemCost = Mathf.RoundToInt(weaponP2.itemPrice * priceMultiplier);
            weapon = weaponP2;
        }

        if (MoneyManager.Instance.GetMoneyAmount() >= itemCost)
        {
            MoneyManager.Instance.SubtractMoney(itemCost);

            if (currentController.CheckInventorySpace())
            {
                currentController.AddInventoryItem(weapon);
                success = true;
            }
            else
            {
                message = noInventorySpaceMessage;
            }
        }
        else
        {
            message = insufficentFundsMessage;
        }

        if (success)
        {
            message = successMessage;
        }

        return message;
    }
}
