using UnityEngine;

public class Gun : ShopItem
{

    public GameObject projectile;

    public override void Execute()
    {
        //Gun Logic
        PlayerController pc = GameManager.Instance.players[0].GetComponent<PlayerController>();
        Vector3 spawnPos = pc.transform.position + pc.transform.forward * 2f;
        //Vector3 spawnPos = transform.parent.position + transform.parent.forward * 1f;
        
        //MoneyManager.Instance.SubtractMoney(itemEquipped.GetComponent<Consumable>().cost);
        GameObject proj = Instantiate(projectile, spawnPos, transform.rotation);
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.archetype = Archetype.Player;
            p.Init(pc.transform.forward);
            p.playerId = 1;
        }
    }
}
