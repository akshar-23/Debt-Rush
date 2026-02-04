using UnityEngine;

public class BombPackage : ShopItem
{
    public GameObject cursorObj;

    private void Start()
    {
        currentCount = maxCount;
    }

    public override void Execute()
    {
        if (isActiveItem)
        {
            PlayerController pc = GameManager.Instance.players[1].GetComponent<PlayerController>();

            if (currentCount <= 0)
            {
                Debug.Log("Out of Bomb Packages!");
                pc.SetCanPlayerMove(true);
                pc.SetCanPlayerAct(true);
                pc.DeleteInventoryItem(this);
                return;
            }

            if (cursorObj != null)
            {
                Vector3 spawnPos = pc.transform.position + pc.transform.forward * 3f;
                GameObject cursorGO = Instantiate(cursorObj, spawnPos, new Quaternion(90, 0, 0, 90));
                Cursor cursor = cursorGO.GetComponent<Cursor>();

                cursor.player = pc;
                pc.activeCursor = cursor;
                pc.SetCanPlayerMove(false);
                pc.SetCanPlayerAct(false);
            }
        }
    }
}
