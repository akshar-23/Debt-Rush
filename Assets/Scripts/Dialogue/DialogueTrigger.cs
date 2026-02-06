using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;

    [Header("Emote Animator")]
    [SerializeField] private Animator emoteAnimator;

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;

    private bool playerInRange;

    private PlayerController playerController1;
    private PlayerController playerController2;

    private void Awake()
    {
        playerInRange = false;
        visualCue.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && !DialogueManager.GetInstance().dialogueIsPlaying)
        {
            visualCue.SetActive(true);
            
            if(playerController1 != null)
            {
                if (playerController1.GetInteractPressed())
                {
                    DialogueManager.GetInstance().EnterDialogueMode(inkJSON, playerController1);
                }
            }

        }
        else
        {
            visualCue.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        GameObject colliderGO = collider.gameObject;
        if (colliderGO.tag == "Player")
        {
            if(colliderGO.GetComponent<PlayerController>().playerNumber == 1)
            {
                playerController1 = colliderGO.GetComponent<PlayerController>();
            }
            if(colliderGO.GetComponent<PlayerController>().playerNumber == 2)
            {
                playerController2 = colliderGO.GetComponent<PlayerController>();
            }
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        GameObject colliderGO = collider.gameObject;
        if (colliderGO.tag == "Player")
        {
            if (colliderGO.GetComponent<PlayerController>().playerNumber == 0)
            {
                playerController1 = null;
            }
            if (colliderGO.GetComponent<PlayerController>().playerNumber == 1)
            {
                playerController2 = null;
            }
            playerInRange = false;
        }
    }
}