using UnityEngine;
using UnityEngine.ProBuilder;

public class TriGun : ShopItem
{
    [Header("TriGun Settings")]
    [SerializeField] public float fireRate = 0.1f;
    public GameObject triGunProjectile;
    [SerializeField] float angleOffset = 15f;
    [SerializeField] float sideOffset = 0.2f;

    private float nextFireTime = 0f;

    [SerializeField] AudioClip ShotgunSFX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentCount = maxCount;
    }

    public override void Execute()
    {
        if (isActiveItem)
        {
            PlayerController pc = GameManager.Instance.players[0].GetComponent<PlayerController>();

            if (ShotgunSFX != null)
            {
                AudioManager.Instance.PlaySFX(ShotgunSFX);
            }

            Vector3 spawnPos = pc.transform.position + pc.transform.forward * 2f;

            Vector3 leftPos = spawnPos - transform.right * sideOffset;
            Vector3 midPos = spawnPos;
            Vector3 rightPos = spawnPos + transform.right * sideOffset;

            GameObject projLeft = Instantiate(triGunProjectile, leftPos, Quaternion.LookRotation(pc.transform.forward) * Quaternion.Euler(0f, 90f - angleOffset, 90f));
            GameObject proj = Instantiate(triGunProjectile, midPos, Quaternion.LookRotation(pc.transform.forward) * Quaternion.Euler(0f, 90f, 90f));
            GameObject projRight = Instantiate(triGunProjectile, rightPos, Quaternion.LookRotation(pc.transform.forward) * Quaternion.Euler(0f, 90f + angleOffset, 90f));

            Projectile pL = projLeft.GetComponent<Projectile>();
            Projectile p = proj.GetComponent<Projectile>();
            Projectile pR = projRight.GetComponent<Projectile>();


            Vector3 baseDir = pc.transform.forward;
            Vector3 dirLeft = Quaternion.AngleAxis(-angleOffset, Vector3.up) * baseDir;
            Vector3 dirMid = baseDir;
            Vector3 dirRight = Quaternion.AngleAxis(angleOffset, Vector3.up) * baseDir;

            if (p != null)
            {
                pL.archetype = Archetype.Player;
                p.archetype = Archetype.Player;
                pR.archetype = Archetype.Player;

                pL.playerId = 1;
                p.playerId = 1;
                pR.playerId = 1;

                pL.Init(dirLeft);
                p.Init(dirMid);
                pR.Init(dirRight);

            }

            nextFireTime = Time.time + fireRate;
            currentCount--;

            if (currentCount <= 0)
            {
                Debug.Log("Out of Ammo!");
                pc.DeleteInventoryItem(this);
                return;
            }

            pc.hudref.UpdateItemCount(pc.id, pc.GetInventoryPos(), this);
        }
    }
}
