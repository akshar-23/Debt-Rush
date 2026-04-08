using UnityEngine;

public class MineItem : ShopItem
{
    public GameObject minePrefab;
    [SerializeField] private int ownerPlayerNumber = 2;

    private void Start()
    {
        currentCount = maxCount;
    }

    public override void Execute()
    {
        if (isActiveItem)
        {
            PlayerController pc = GameManager.Instance.players[ownerPlayerNumber - 1].GetComponent<PlayerController>();

            if (currentCount <= 0)
            {
                Debug.Log("Out of Mines!");
                pc.DeleteInventoryItem(this);
                return;
            }

            if (minePrefab != null)
            {
                Vector3 spawnPos = pc.transform.position + pc.transform.right * 0.6f;
                if (Physics.Raycast(spawnPos + Vector3.up, Vector3.down, out RaycastHit hit, 5f))
                    spawnPos.y = hit.point.y;
                Debug.Log($"[MineItem] Spawning mine at {spawnPos}");
                GameObject mineGO = Instantiate(minePrefab, spawnPos, Quaternion.identity);

                Mine mine = mineGO.GetComponent<Mine>();
                if (mine != null) mine.playerId = ownerPlayerNumber;

                SubtractMineCount();
                pc.hudref.UpdateItemCount(pc.id, pc.GetInventoryPos(), this);
                if (currentCount <= 0)
                    pc.DeleteInventoryItem(this);
            }
            else
            {
                Debug.LogWarning("[MineItem] minePrefab is not assigned!");
            }
        }
    }

    public void SubtractMineCount()
    {
        currentCount--;
    }
}
