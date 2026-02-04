using UnityEngine;

public class Shield : ShopItem
{
    private bool isActive;

    private int playerIndex;

    public override void Init(int _playerIndex)
    {
        playerIndex = _playerIndex;
    }

    public override void Execute()
    {
        if (isActive) return;

        PlayerController pc = GameManager.Instance.players[playerIndex - 1].GetComponent<PlayerController>();
        isActive = true;
        Vector3 addPosition = new Vector3(0, 0.15f, 1f);
        pc.EquipItemInTheModel(this.gameObject, addPosition);
    }
}
