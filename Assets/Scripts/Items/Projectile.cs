using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Archetype archetype;
    public float speed = 50f;
    public int damage = 25;
    public float lifetime = 5f;
    public int playerId;

    Rigidbody rb;
    float spawnTime;

    [SerializeField] private AudioClip bulletSFX_1;
    [SerializeField] private AudioClip bulletSFX_2;

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
        if (other.CompareTag("PassProjectile")) return;

        if (other.CompareTag("Shield"))
        {
            if (bulletSFX_1 != null && bulletSFX_2 != null)
            {
                AudioManager.Instance.PlayRandomClip(bulletSFX_1, bulletSFX_2);
            }

            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Flammable"))
        {
            // Example: if the enemy has a script with a TakeDamage() method
            Flammable flammableObject = other.GetComponent<Flammable>();
            if (flammableObject != null)
            {
                flammableObject.AddHit();
            }
        }

        Character character = other.GetComponent<Character>();
        if (character != null && character.archetype != archetype) character.TakeDamage(damage, playerId);
        
        Destroy(gameObject);
    }
}
