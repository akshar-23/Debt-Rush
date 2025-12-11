using UnityEngine;
using System.Collections;
using System;

public enum Archetype
{
    Player,
    Enemy
}

public class Character : MonoBehaviour
{
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

    public void TakeDamage(float receivedDamage, int _playerId, Action<bool> isDead = null)
    {
        currentHealth -= receivedDamage;
        Debug.Log(gameObject.name + " took " + receivedDamage + " damage. Current HP: " + currentHealth);

        // Health below 0
        if (currentHealth <= 0f)
        {
            Die(_playerId);
            isDead?.Invoke(true);
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    private void Die(int _playerId)
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
            GameManager.Instance.players[_playerId-1].GetComponent<PlayerController>().killCount += 1;
            Debug.LogWarning("Enemy killed by Player " + (_playerId-1) + ". Player's kill count: " + GameManager.Instance.players[_playerId-1].GetComponent<PlayerController>().killCount);
            Destroy(gameObject);
        }
    }

    public void Reset()
    {
        currentHealth = maxHealth;
        isDead = false;
    }
}
