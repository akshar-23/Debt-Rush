using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EncounterSpawnPoint : MonoBehaviour
{

    public List<GameObject> enemies;

    public bool isTriggered = false;
    public bool isFightOver = false;

    public AudioClip backgroundFightMusicClip;

    [Header("Hint UI")]
    public HintText hintUI;
    private const string EquippedWeaponHintText = "□ - Use Item";

    public void Update()
    {
        if (isFightOver) return;

        if (isTriggered)
        {
            CheckAllChildren();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (hintUI != null)
            {
                hintUI.ShowHint(EquippedWeaponHintText);
            }

            if (player != null)
            {
                ActivateAllChildren();
                Debug.Log("One Player has reached the encounter spawn zone!");
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        GameObject colliderGO = collider.gameObject;
        if (colliderGO.tag == "Player")
        {
            if (hintUI != null)
            {
                hintUI.HideHint();
            }
        }
    }

    public void ActivateAllChildren()
    {
        AudioManager.Instance.PlayMusic(backgroundFightMusicClip);

        // Iterate through all immediate children of this GameObject's transform
        foreach (Transform child in transform)
        {
            // Set the child's GameObject to active
            child.gameObject.SetActive(true);
            enemies.Add(child.gameObject);
        }
        this.gameObject.GetComponent<BoxCollider>().enabled = false;
        isTriggered = true;
    }

    public void CheckAllChildren()
    {
        int counter = 0;

        foreach (GameObject child in enemies)
        {
            // Counter plus 1 each time a childred died
            if(child == null)
            {
                counter++;
            }

            if(counter == enemies.Count)
            {
                AudioManager.Instance.PlayDefaultMusic();
                isFightOver = true;
            }
        }
    }
}
