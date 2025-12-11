using UnityEngine;

public class BombPackage : ShopItem
{
    public GameObject cursorObj;

    public override void Execute()
    {
        PlayerController pc = GameManager.Instance.players[1].GetComponent<PlayerController>();

        if (cursorObj != null)
        {
            GameObject cursor = Instantiate(cursorObj, pc.transform.position, new Quaternion(90, 0, 0, 90));

            cursor.GetComponent<Cursor>().player = pc;
            pc.SetCanPlayerMove(false);
            pc.SetCanPlayerAct(false);

        }

    }
}
