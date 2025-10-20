using UnityEngine;



public class Character : MonoBehaviour
{
    public enum Archetype
    {
        Player,
        Enemy
    }

    public Archetype archetype;
    public static event System.Action<Transform> OnPlayerDied;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float receivedDamage)
    {
        currentHealth -= receivedDamage;
        Debug.Log(gameObject.name + " took " + receivedDamage + " damage. Current HP: " + currentHealth);

        // Health below 0
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (archetype == Archetype.Player)
        {
            OnPlayerDied?.Invoke(transform);
        }
        Debug.Log(gameObject.name + " died!");
        // GameManager.Instance.CheckWinLossConditions();
        Destroy(gameObject);
    }
}
