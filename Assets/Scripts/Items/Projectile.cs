using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Archetype archetype;
    public float speed = 20f;
    public int damage = 25;
    public float lifetime = 5f;
    public int playerId;

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
        HandleHit(other.gameObject);
    }

    void HandleHit(GameObject other)
    {
        Debug.Log("Enemy should take damage");
        // We should add more tags (Players) to ignore them for now, still in testing
        if (other.CompareTag("Projectile")) return;

        if (other.CompareTag("Shield"))
        {
            Destroy(gameObject);
            return;
        }

        Character character = other.GetComponent<Character>();
        if (character != null && character.archetype != archetype)
            character.TakeDamage(damage, playerId);

        // Deactivate/destroy
        //gameObject.SetActive(false); // better for pooling; otherwise Destroy(gameObject);
        Destroy(gameObject);
    }
}
