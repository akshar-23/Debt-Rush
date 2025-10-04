using UnityEngine;
using System.Collections;

public class Consumable : Item
{
    public int uses;

    public Consumable(string name, string description, int value, int cost, Sprite icon = null)
        : base(name, description, cost, icon)
    {
        
    }

    public override void Use()
    {
        base.Use();
        Debug.Log("You pay " + cost + " money!");
        MoneyManager.Instance.SubtractMoney(cost);
        uses--;
    }
}
