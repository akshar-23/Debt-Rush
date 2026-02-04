using UnityEngine;

public class Strawhat : ShopItem
{
    private bool isActive;

    [Header("HP Attributes")]
    [SerializeField] private int addedHP = 50;

    private int playerIndex;

    public override void Init(int _playerIndex)
    {
        playerIndex = _playerIndex;
    }

    public override void Execute()
    {
        if (isActive) return;

        PlayerController pc = GameManager.Instance.players[playerIndex - 1].GetComponent<PlayerController>();
        pc.maxHealth += addedHP;
        pc.Reset();
        isActive = true;
        Vector3 addPosition = new Vector3(0, 0.75f, 0);
        pc.EquipItemInTheModel(this.gameObject, addPosition);
    }
}
