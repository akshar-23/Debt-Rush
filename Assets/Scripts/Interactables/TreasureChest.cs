using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private float respawnTime = 30f;
    [SerializeField] private ShopItem weaponP1;
    [SerializeField] private ShopItem weaponP2;

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
            GameManager gameM = GameManager.Instance.GetComponent<GameManager>();
            if (gameM != null)
            {
                PlayerController player = other.gameObject.GetComponent<PlayerController>();
                int playerNumber = player.GetPlayerNumber();
                if (playerNumber == 1)
                {
                    ShopItem instance1 = Instantiate(weaponP1);
                    instance1.currentCount = instance1.maxCount;
                    player.AddInventoryItem(instance1);
                }
                else
                {
                    ShopItem instance2 = Instantiate(weaponP2);
                    instance2.currentCount = instance2.maxCount;
                    player.AddInventoryItem(instance2);
                }

                // updating opening chest count
                player.chestCount++;
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
        gameObject.SetActive(false);
        col.enabled = false;
        mesh.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        gameObject.SetActive(true);
        col.enabled = true;
        mesh.enabled = true;
    }
}
