using System.Collections;
using UnityEngine;

public class MoneyBag : MonoBehaviour
{
    [Header("Money Settings")]
    [SerializeField] private int moneyValue = 100;
    [SerializeField] private float respawnTime = 15f;

    [Header("Optional Effects")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private AudioClip pickupSound;

    private Collider col;
    private MeshRenderer mesh;

    private void Awake()
    {
        col = GetComponent<Collider>();
        mesh = GetComponent<MeshRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Only trigger if the object has the Player tag
        if (other.CompareTag("Player"))
        {
            // Try to get player's money component
            MoneyManager moneyM = MoneyManager.Instance.GetComponent<MoneyManager>();
            if (moneyM != null)
            {
                moneyM.AddMoney(moneyValue);
            }

            // Spawn pickup effect (optional)
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            // Play sound (optional)
            if (pickupSound != null)
            {
                AudioManager.Instance.PlaySFX(pickupSound);
            }

            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        col.enabled = false;
        mesh.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        col.enabled = true;
        mesh.enabled = true;
    }
}
