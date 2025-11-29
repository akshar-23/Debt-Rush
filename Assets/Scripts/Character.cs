using UnityEngine;
using System.Collections;


public class Character : MonoBehaviour
{
    public enum Archetype
    {
        Player,
        Enemy
    }

    public Archetype archetype;
    public static event System.Action<int> OnPlayerDied;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    public bool isDead = false;
    public int id;

    protected virtual void Awake()
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

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " died!");
        if (archetype == Archetype.Player)
        {
            OnPlayerDied?.Invoke(id);
            gameObject.SetActive(false);
            isDead = true;
        }
        else if (archetype == Archetype.Enemy)
        {
            Destroy(gameObject);
        }
    }
}
