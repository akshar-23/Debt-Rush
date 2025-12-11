using UnityEngine;

public abstract class ShopItem : MonoBehaviour
{
    public int id;
    public string itemName;
    public int itemPrice;
    [TextArea] public string description;
    public int maxCount;
    [SerializeField, HideInInspector] public int currentCount;
    public bool isActiveItem = false;

    public abstract void Execute();
}
