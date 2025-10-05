using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 10;
    public float lifetime = 5f;
    public bool usePhysicsCollision = false; // true -> OnCollisionEnter, false -> OnTriggerEnter

    Rigidbody rb;
    float spawnTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        if (Time.time - spawnTime >= lifetime)
            Destroy(gameObject);
    }

    public void Init(Vector3 direction)
    {
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
        else
        {
            // fallback: move by transform
            transform.position += direction.normalized * speed * Time.deltaTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (usePhysicsCollision) return;
        HandleHit(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!usePhysicsCollision) return;
        HandleHit(collision.gameObject);
    }

    void HandleHit(GameObject other)
    {
        // We should add more tags (Players) to ignore them for now, still in testing
        if (other.CompareTag("Projectile")) return;

        var hp = other.GetComponent<Character>();
        if (hp != null)
            hp.TakeDamage(damage);

        // Deactivate/destroy
        //gameObject.SetActive(false); // better for pooling; otherwise Destroy(gameObject);
        Destroy(gameObject);
    }
}
