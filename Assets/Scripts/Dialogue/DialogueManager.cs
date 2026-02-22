using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;

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

    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private GameObject dialogueContext;
    [SerializeField] private GameObject dialogueContext_P1;
    [SerializeField] private GameObject dialogueContext_P2;

    [Header("Choices UI")]
    [SerializeField] private GameObject dialogueChoicesPanel;
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    public DialogueState currentState;
    private Story currentStory;

    private const string SPEAKER_TAG = "speaker";
    private const string PORTRAIT_TAG = "portrait";
    private const string AUDIO_TAG = "audio";
    private const string MOVEMENT_TAG = "movement";

    public bool dialogueIsPlaying { get; private set; }
    public bool isPlayer1inDialogue { get; private set; }
    public bool isPlayer2inDialogue { get; private set; }
    public bool canContinueToNextLine = false;

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

        dialogueChoicesPanel.SetActive(false);
        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;

        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    private void Update()
    {
        // return right away if dialogue ins't playing
        if (!dialogueIsPlaying)
        {
            return;
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON, PlayerController playerController, GameObject npcController)
    {
        dialogueIsPlaying = true;

        if (cameraManager.GetCurrentState() == CameraManager.CameraState.Single)
        {
            if (playerController.GetPlayerNumber() == 1)
            {
                isPlayer1inDialogue = true;
                playerController1 = playerController;
                playerController1.SetCanPlayerMove(false);
                playerController1.input.SwitchCurrentActionMap("UI");
            }
            if (playerController.GetPlayerNumber() == 2)
            {
                isPlayer2inDialogue = true;
                playerController2 = playerController;
                playerController2.SetCanPlayerMove(false);
                playerController2.input.SwitchCurrentActionMap("UI");
            }

            dialogueContext.GetComponent<DialogueContext>().EnterDialogueMode(inkJSON, playerController, npcController);
        }
        if (cameraManager.GetCurrentState() == CameraManager.CameraState.Split)
        {
            if (playerController.GetPlayerNumber() == 1)
            {
                playerController1 = playerController;
                playerController1.SetCanPlayerMove(false);
                playerController1.input.SwitchCurrentActionMap("UI");
                isPlayer1inDialogue = true;
                dialogueContext.SetActive(false);
                dialogueContext_P1.GetComponent<DialogueContext>().EnterDialogueMode(inkJSON, playerController, npcController);
            }

            if (playerController.GetPlayerNumber() == 2)
            {
                playerController2 = playerController;
                playerController2.SetCanPlayerMove(false);
                playerController2.input.SwitchCurrentActionMap("UI");
                isPlayer2inDialogue = true;
                dialogueContext.SetActive(false);
                dialogueContext_P2.GetComponent<DialogueContext>().EnterDialogueMode(inkJSON, playerController, npcController);
            }
        }
    }

    public void NotifyEndDialogue()
    {
        dialogueIsPlaying = false;

        if (isPlayer1inDialogue)
        {
            playerController1.input.SwitchCurrentActionMap("Player");
        }
        if (isPlayer2inDialogue)
        {
            playerController2.input.SwitchCurrentActionMap("Player");
        }
    }

    private void ExitDialogueMode()
    {
        dialogueIsPlaying = false;

        
        //TODO: WRAP THIS
        //playerController1.SetCanPlayerMove(true);
        if (playerController1 != null)
        {
            playerController1.SetCanPlayerMove(true);
            playerController1 = null;
        }
        if (playerController2 != null)
        {
            playerController2.SetCanPlayerMove(true);
            playerController2 = null;
        }
    }

    public bool GetDialogueChoicesActiveStatus()
    {
        return dialogueChoicesPanel.activeInHierarchy;
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
        }
        if (isPlayer2inDialogue)
        {
            DialogueContext dc_p2 = dialogueContext.GetComponent<DialogueContext>();
            dialogueContext_P2.SetActive(true);
            dialogueContext_P2.GetComponent<DialogueContext>().UpdateCurrentStory(dc_p2.GetCurrentStory(), dc_p2.GetCurrentController());
            dialogueContext.GetComponent<DialogueContext>().TransitionDialogue();
            playerController2.SetCanPlayerMove(false);
            playerController2.input.SwitchCurrentActionMap("UI");
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
        }
        if (isPlayer2inDialogue)
        {
            dc_cp = dialogueContext_P2.GetComponent<DialogueContext>();
            dialogueContext.GetComponent<DialogueContext>().UpdateCurrentStory(dc_cp.GetCurrentStory(), dc_cp.GetCurrentController());
            dialogueContext_P2.SetActive(false);
            dialogueContext_P2.GetComponent<DialogueContext>().TransitionDialogue();
            playerController2.SetCanPlayerMove(false);
            playerController2.input.SwitchCurrentActionMap("UI");
        }
    }
}