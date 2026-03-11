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

    [Header("Portrait")]
    [SerializeField] private Sprite portrait;

    private PlayerController playerController1;
    private PlayerController playerController2;

    private bool isPlayerinDialogue = false;
    private bool isPlayer1inDialogue = false;
    private bool isPlayer2inDialogue = false;

    [Header("Hint UI")]
    public HintText hintUI;
    private const string TalkHintText = "X - Talk";

    private void Awake()
    {
        if(visualCue != null)
            visualCue.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerinDialogue) return;

        if(playerController1 != null && playerController1.GetSubmitPressed())
        {
            Debug.Log("[DialogueTrigger] Triggered by P1 | Ink: " + inkJSON.name + " | Object: " + transform.parent.name);
            isPlayerinDialogue = true;
            DialogueManager.GetInstance().EnterDialogueMode(inkJSON, playerController1, this.gameObject, portrait);

            if (hintUI != null)
                hintUI.HideHint();
        }
        if (playerController2 != null && playerController2.GetSubmitPressed())
        {
            Debug.Log("[DialogueTrigger] Triggered by P2 | Ink: " + inkJSON.name + " | Object: " + transform.parent.name);
            isPlayerinDialogue = true;
            DialogueManager.GetInstance().EnterDialogueMode(inkJSON, playerController2, this.gameObject, portrait);

            if (hintUI != null)
                hintUI.HideHint();
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        GameObject colliderGO = collider.gameObject;
        if (colliderGO.tag == "Player")
        {
            if(colliderGO.GetComponent<PlayerController>().playerNumber == 1)
                playerController1 = colliderGO.GetComponent<PlayerController>();

            if(colliderGO.GetComponent<PlayerController>().playerNumber == 2)
                playerController2 = colliderGO.GetComponent<PlayerController>();

            Debug.Log("[DialogueTrigger] Player entered: " + transform.parent.name);

            if (visualCue != null)
                visualCue.SetActive(true);

            if(hintUI != null)
                hintUI.ShowHint(TalkHintText);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        GameObject colliderGO = collider.gameObject;
        if (colliderGO.tag == "Player")
        {
            if (colliderGO.GetComponent<PlayerController>().playerNumber == 1)
                playerController1 = null;

            if (colliderGO.GetComponent<PlayerController>().playerNumber == 2)
                playerController2 = null;

            Debug.Log("[DialogueTrigger] Player exited: " + transform.parent.name);
        }
        if(playerController1 == null && playerController2 == null && visualCue != null)
        {
            visualCue.SetActive(false);
            isPlayerinDialogue = false;

            if (hintUI != null)
                hintUI.HideHint();
        }
    }
}
