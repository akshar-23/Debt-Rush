using UnityEngine;

public class Boots : ShopItem
{

    private bool isActive;

    [Header("Speed Attributes")]
    [SerializeField] private int addedMoveSpeed = 5;

    private int playerIndex;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }
    public override void Init(int _playerIndex)
    {
        playerIndex = _playerIndex;
    }

    public override void Execute()
    {
        if (isActive) return;

        PlayerController pc = GameManager.Instance.players[playerIndex-1].GetComponent<PlayerController>();
        pc.moveSpeed += addedMoveSpeed;
        isActive = true;
    }
}
