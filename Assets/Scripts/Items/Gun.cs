using UnityEngine;

public class Gun : ShopItem
{
    [Header("Gun Settings")]
    public float fireRate = 0.1f;
    public GameObject projectile;

    private float nextFireTime = 0f;

    [SerializeField] private AudioClip shootSFX_1;
    [SerializeField] private AudioClip shootSFX_2;

    private void Start()
    {
        currentCount = maxCount;
    }

    public override void Execute()
    {
        if (isActiveItem)
        {
            PlayerController pc = GameManager.Instance.players[0].GetComponent<PlayerController>();

            Vector3 spawnPos = pc.transform.position + pc.transform.forward * 2f;

            GameObject proj = Instantiate(projectile, spawnPos, transform.rotation);
            Projectile p = proj.GetComponent<Projectile>();

            if (p != null)
            {
                p.archetype = Archetype.Player;
                if (shootSFX_1 != null && shootSFX_2 != null)
                {
                    AudioManager.Instance.PlayRandomClip(shootSFX_1, shootSFX_2);
                }
                p.Init(pc.transform.forward);
                p.playerId = 1;
            }

            nextFireTime = Time.time + fireRate;
            currentCount--;

            if (currentCount <= 0)
            {
                Debug.Log("Out of Ammo!");
                pc.DeleteInventoryItem(this);
                return;
            }

            pc.hudref.UpdateItemCount(pc.id, this);
        }
    }
}