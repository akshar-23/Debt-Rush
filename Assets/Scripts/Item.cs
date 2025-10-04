using UnityEngine;

public class Item : MonoBehaviour
{
    public string itemName;
    public string description;
    public int cost;
    public Sprite icon;

    public Item(string name, string description, int cost, Sprite icon = null)
    {
        this.itemName = name;
        this.description = description;
        this.cost = cost;
        this.icon = icon;
    }

    public virtual void Use()
    {
        Debug.Log("Using item: " + itemName);
    }
}
