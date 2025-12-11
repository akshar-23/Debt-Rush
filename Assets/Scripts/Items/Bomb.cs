using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float explosionRadius = 5f;   // Area of effect
    public float explosionDamage = 50f;  // Damage amount
    public GameObject explosionEffect;   // Optional visual effect

    public int playerId;
    public int multiKill = 0;

    private bool hasExploded = false;

    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return; // Prevent multiple explosions

        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Explosion!");
            Explode();
        }
    }

    void Explode()
    {
        hasExploded = true;

        // Optional: spawn a particle effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Find all nearby colliders within the radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Debug.Log("Enemies nearby!");
            // Apply damage to enemies
            if (nearbyObject.CompareTag("Enemy"))
            {
                // Example: if the enemy has a script with a TakeDamage() method
                Character enemy = nearbyObject.GetComponent<Character>();
                if (enemy != null)
                {
                    enemy.TakeDamage(explosionDamage, playerId, isDead =>
                    {
                        if (isDead)
                        {
                            multiKill++;
                            Debug.Log("Enemy killed by bomb! Total kills from this bomb: " + multiKill);
                        }
                    });
                }
            }
        }

        // Optionally: add a physics explosion force
        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.attachedRigidbody;
            if (rb != null)
            {
                rb.AddExplosionForce(500f, transform.position, explosionRadius);
            }
        }

        GameManager.Instance.players[playerId - 1].GetComponent<PlayerController>().lastMultiKillCount = multiKill;

        // Destroy the bomb
        Destroy(gameObject);
    }

    // Optional: visualize explosion range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
