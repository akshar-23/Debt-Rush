using UnityEngine;

public class Mine : MonoBehaviour
{
    public float explosionRadius = 5f;
    public float explosionDamage = 50f;
    public GameObject explosionEffect;
    public float explosionEffectLifetime = 1.5f;

    public int playerId;
    public int multiKill = 0;

    [SerializeField] private AudioClip explosionSFX_1;
    [SerializeField] private AudioClip explosionSFX_2;

    private bool hasExploded = false;

    void Start()
    {
        // A kinematic Rigidbody is required for trigger detection to work reliably
        var rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        Debug.Log("[Mine] Spawned. Rigidbody kinematic: " + rb.isKinematic);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("[Mine] OnTriggerEnter hit by: " + other.gameObject.name + " tag: " + other.tag);
        if (hasExploded) return;
        if (other.CompareTag("Enemy")) Explode();
    }

    void Explode()
    {
        hasExploded = true;

        if (explosionSFX_1 != null && explosionSFX_2 != null)
            AudioManager.Instance.PlayRandomClip(explosionSFX_1, explosionSFX_2);

        if (explosionEffect != null)
        {
            GameObject fireExplosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            fireExplosion.transform.localScale = Vector3.one * 2f;
            Destroy(fireExplosion, explosionEffectLifetime);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.CompareTag("Enemy"))
            {
                Character enemy = nearbyObject.GetComponent<Character>();
                if (enemy != null)
                {
                    enemy.TakeDamage(explosionDamage, playerId, isDead =>
                    {
                        if (isDead)
                        {
                            multiKill++;
                            Debug.Log("Enemy killed by mine! Total kills from this mine: " + multiKill);
                        }
                    });
                }
            }

            if (nearbyObject.CompareTag("Flammable"))
            {
                Flammable flammableObject = nearbyObject.GetComponent<Flammable>();
                if (flammableObject != null)
                    flammableObject.Ignite();
            }
        }

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.attachedRigidbody;
            if (rb != null)
                rb.AddExplosionForce(500f, transform.position, explosionRadius);
        }

        GameManager.Instance.players[playerId - 1].GetComponent<PlayerController>().lastMultiKillCount = multiKill;

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
