using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

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
        Debug.Log(gameObject.name + " died!");
        GameManager.Instance.CheckWinLossConditions();
        Destroy(gameObject);
    }
}
