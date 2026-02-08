using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

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
    private int counter = 0;
    private const int CounterMax = 2;

    private bool isPlayer1inDialogue = false;
    private bool isPlayer2inDialogue = false;

    private void Awake()
    {
        playerInRange = false;
        visualCue.SetActive(false);
    }

    private void Update()
    {
        if (PlayerInRange())// && !DialogueManager.GetInstance().AreBothPlayersInDialogue())
        {
            visualCue.SetActive(true);
            
            if(playerController1 != null)
            {
                if (!isPlayer1inDialogue && playerController1.GetInteractPressed())
                {
                    isPlayer1inDialogue = true;
                    DialogueManager.GetInstance().EnterDialogueMode(inkJSON, playerController1, this.gameObject);
                }
            }
            if (playerController2 != null)
            {
                if (!isPlayer2inDialogue &&  playerController2.GetInteractPressed())
                {
                    isPlayer2inDialogue = true;
                    DialogueManager.GetInstance().EnterDialogueMode(inkJSON, playerController2, this.gameObject);
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
            counter++;
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        GameObject colliderGO = collider.gameObject;
        if (colliderGO.tag == "Player")
        {
            if (colliderGO.GetComponent<PlayerController>().playerNumber == 1)
            {
                playerController1 = null;
            }
            if (colliderGO.GetComponent<PlayerController>().playerNumber == 2)
            {
                playerController2 = null;
            }
            counter--;
            playerInRange = false;
        }
    }

    private bool PlayerInRange()
    {
        return Mathf.Clamp(counter, 0, CounterMax) >=  1 ? true : false;
    }
}