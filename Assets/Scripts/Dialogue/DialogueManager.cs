using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class DialogueManager : MonoBehaviour
{
    public enum DialogueState
    {
        Single,
        Split
    }

    [Header("Params")]
    [SerializeField] private float typingSpeed = 0.04f;
    [SerializeField] public PlayerController playerController1;
    [SerializeField] public PlayerController playerController2;
    [SerializeField] public GameObject NPCController;

    [Header("Shared UI")]
    [SerializeField] public Canvas sharedCanvas;
    [SerializeField] public GameObject sharedFirstButton;
    [SerializeField] public MultiplayerEventSystem sharedEventSystem;
    [SerializeField] public InputSystemUIInputModule sharedUIModule;

    [Header("Split UI")]
    [SerializeField] public Canvas canvas_P1;
    [SerializeField] public GameObject firstButton_P1;
    [SerializeField] public MultiplayerEventSystem eventSystem_P1;
    [SerializeField] public Canvas canvas_P2;
    [SerializeField] public GameObject firstButton_P2;
    [SerializeField] public MultiplayerEventSystem eventSystem_P2;

    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private GameObject dialogueContext;
    [SerializeField] private GameObject dialogueContext_P1;
    [SerializeField] private GameObject dialogueContext_P2;

    public DialogueState currentState;
    private Story currentStory;

    public bool dialogueIsPlaying { get; private set; }
    public bool isPlayer1inDialogue { get; private set; }
    public bool isPlayer2inDialogue { get; private set; }

    private static DialogueManager instance;

    private Coroutine displayLineCoroutine;

    public AudioManager audioManagerInstance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Input Manager in the scene.");
        }
        instance = this;
    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    public void Start()
    {
        dialogueIsPlaying = false;
    }

    private void Update()
    {
        // return right away if dialogue ins't playing
        if (!dialogueIsPlaying)
        {
            return;
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON, PlayerController playerController, GameObject npcController, Sprite portrait = null)
    {
        dialogueIsPlaying = true;

        if (cameraManager.GetCurrentState() == CameraManager.CameraState.Single)
        {
            PlayerController currentController = null;
            dialogueContext.SetActive(true);

            if (playerController.GetPlayerNumber() == 1)
            {
                isPlayer1inDialogue = true;
                playerController1 = playerController;
                playerController1.SetCanPlayerMove(false);
                playerController1.input.SwitchCurrentActionMap("UI");
                currentController = playerController1;
                playerController1.BindUI(eventSystem_P1, sharedCanvas, sharedFirstButton);
                dialogueContext.GetComponent<DialogueContext>().EnterDialogueMode(inkJSON, playerController1, npcController, portrait);
            }
            if (playerController.GetPlayerNumber() == 2)
            {
                isPlayer2inDialogue = true;
                playerController2 = playerController;
                playerController2.SetCanPlayerMove(false);
                playerController2.input.SwitchCurrentActionMap("UI");
                currentController = playerController2;
                playerController2.BindUI(eventSystem_P2, sharedCanvas, sharedFirstButton);
                dialogueContext.GetComponent<DialogueContext>().EnterDialogueMode(inkJSON, playerController2, npcController, portrait);
            }

        }
        if (cameraManager.GetCurrentState() == CameraManager.CameraState.Split)
        {
            if (playerController.GetPlayerNumber() == 1)
            {
                canvas_P1.enabled = true;
                dialogueContext_P1.SetActive(true);
                playerController1 = playerController;
                playerController1.SetCanPlayerMove(false);
                playerController1.input.SwitchCurrentActionMap("UI");
                isPlayer1inDialogue = true;
                dialogueContext.SetActive(false);
                playerController1.BindUI(eventSystem_P1, canvas_P1, firstButton_P1);
                dialogueContext_P1.GetComponent<DialogueContext>().EnterDialogueMode(inkJSON, playerController1, npcController, portrait);
            }

            if (playerController.GetPlayerNumber() == 2)
            {
                canvas_P2.enabled = true;
                dialogueContext_P2.SetActive(true);
                playerController2 = playerController;
                playerController2.SetCanPlayerMove(false);
                playerController2.input.SwitchCurrentActionMap("UI");
                isPlayer2inDialogue = true;
                dialogueContext.SetActive(false);
                playerController2.BindUI(eventSystem_P2, canvas_P2, firstButton_P2);
                dialogueContext_P2.GetComponent<DialogueContext>().EnterDialogueMode(inkJSON, playerController2, npcController, portrait);
            }
        }
    }

    public void NotifyEndDialogue(PlayerController player)
    {
        dialogueIsPlaying = false;

        if (player.playerNumber == 1)
        {
            playerController1.input.SwitchCurrentActionMap("Player");
            playerController1.BindUI(eventSystem_P1, canvas_P1, firstButton_P1);
            isPlayer1inDialogue = false;
        }
        if (player.playerNumber == 2)
        {
            playerController2.input.SwitchCurrentActionMap("Player");
            playerController2.BindUI(eventSystem_P2, canvas_P2, firstButton_P2);
            isPlayer2inDialogue = false;
        }

        //sharedCanvas.enabled = false;
        //sharedEventSystem.enabled = false;
        //sharedUIModule.enabled = false;
    }

    public void SwitchToSplitScreenDialogue()
    {
        if (!dialogueIsPlaying) return;

        dialogueContext.SetActive(false);
        if (isPlayer1inDialogue)
        {
            DialogueContext dc_p1 = dialogueContext.GetComponent<DialogueContext>();
            dialogueContext_P1.SetActive(true);
            dialogueContext_P1.GetComponent<DialogueContext>().UpdateCurrentStory(dc_p1.GetCurrentStory(), dc_p1.GetCurrentController());
            dialogueContext.GetComponent<DialogueContext>().TransitionDialogue();
            playerController1.SetCanPlayerMove(false);
            playerController1.input.SwitchCurrentActionMap("UI");
            playerController1.BindUI(eventSystem_P1, canvas_P1, firstButton_P1);
        }
        if (isPlayer2inDialogue)
        {
            DialogueContext dc_p2 = dialogueContext.GetComponent<DialogueContext>();
            canvas_P2.enabled = true;
            dialogueContext_P2.SetActive(true);
            dialogueContext_P2.GetComponent<DialogueContext>().UpdateCurrentStory(dc_p2.GetCurrentStory(), dc_p2.GetCurrentController());
            dialogueContext.GetComponent<DialogueContext>().TransitionDialogue();
            playerController2.SetCanPlayerMove(false);
            playerController2.input.SwitchCurrentActionMap("UI");
            playerController2.BindUI(eventSystem_P2, canvas_P2, firstButton_P2);
        }
    }

    public void SwitchToSingleScreenDialogue()
    {
        if (!dialogueIsPlaying) return;

        DialogueContext dc_cp = null;
        dialogueContext.SetActive(true);

        if (isPlayer1inDialogue)
        {
            dc_cp = dialogueContext_P1.GetComponent<DialogueContext>();
            dialogueContext.GetComponent<DialogueContext>().UpdateCurrentStory(dc_cp.GetCurrentStory(), dc_cp.GetCurrentController());
            dialogueContext_P1.SetActive(false);
            dialogueContext_P1.GetComponent<DialogueContext>().TransitionDialogue();
            playerController1.SetCanPlayerMove(false);
            playerController1.input.SwitchCurrentActionMap("UI");
            playerController1.BindUI(eventSystem_P1, sharedCanvas, sharedFirstButton);
        }
        if (isPlayer2inDialogue)
        {
            dc_cp = dialogueContext_P2.GetComponent<DialogueContext>();
            dialogueContext.GetComponent<DialogueContext>().UpdateCurrentStory(dc_cp.GetCurrentStory(), dc_cp.GetCurrentController());
            dialogueContext_P2.SetActive(false);
            dialogueContext_P2.GetComponent<DialogueContext>().TransitionDialogue();
            playerController2.SetCanPlayerMove(false);
            playerController2.input.SwitchCurrentActionMap("UI");
            playerController2.BindUI(eventSystem_P2, sharedCanvas, sharedFirstButton);
        }
    }
}