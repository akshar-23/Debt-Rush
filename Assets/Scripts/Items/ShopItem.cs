using UnityEngine;

public abstract class ShopItem : MonoBehaviour
{
    public int Id;
    public string Name;
    public int Price;
    [TextArea] public string Description;
    public int Limit;
    [SerializeField, HideInInspector] public int Current;

    public abstract void Execute();
}
