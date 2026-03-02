using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;

public class DialogueContext : MonoBehaviour
{
    [Header("Params")]
    [SerializeField] private float typingSpeed = 0.04f;
    [SerializeField] public PlayerController currentController;
    [SerializeField] public GameObject NPCController;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private Animator portraitAnimator;
    [SerializeField] private GameObject continueIcon;

    [Header("Choices UI")]
    [SerializeField] private GameObject dialogueChoicesPanel;
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;


    private Story currentStory;

    private const string SPEAKER_TAG = "speaker";
    private const string PORTRAIT_TAG = "portrait";
    private const string AUDIO_TAG = "audio";
    private const string MOVEMENT_TAG = "movement";

    public bool dialogueIsPlaying { get; private set; }
    public bool isPlayer1inDialogue { get; private set; }
    public bool isPlayer2inDialogue { get; private set; }
    public bool canContinueToNextLine = false;

    private Coroutine displayLineCoroutine;

    public AudioManager audioManagerInstance;

    public void Start()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);

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

        // handle continuing to the next line in the dialogue when submit is pressed
        if (canContinueToNextLine && (currentController != null && currentController.GetSubmitPressed()))
        {
            ContinueStory();
        }
    }

    public Story GetCurrentStory()
    {
        return currentStory;
    }

    public PlayerController GetCurrentController()
    {
        return currentController;
    }

    public void UpdateCurrentStory(Story _currentStory, PlayerController _currentController)
    {
        dialoguePanel.SetActive(true);
        currentStory = _currentStory;
        dialogueIsPlaying = true;
        dialogueText.text = currentStory.currentText;
        currentController = _currentController;
        currentController.SetCanPlayerMove(false);

        HandleTags(currentStory.currentTags);
        //DisplayChoices();
        //continueIcon.SetActive(true);
        //canContinueToNextLine = true;
        displayLineCoroutine = StartCoroutine(DisplayLine(currentStory.currentText));
    }

    public void EnterDialogueMode(TextAsset inkJSON, PlayerController playerController, GameObject npcController)
    {
        currentController = playerController;
        currentStory = new Story(inkJSON.text);

        // Defining functions that we are going to use from the NPC
        NPCController = npcController;
        currentStory.BindExternalFunction("openGate", () => {
            NPCController.gameObject.GetComponentInParent<MoneyGate>().TryToOpenGate();
        });

        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);

        ContinueStory();
    }
    public void TransitionDialogue()
    {
        ExitDialogueMode(true);
    }

    private void ExitDialogueMode(bool isTransition = false)
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueChoicesPanel.SetActive(false);
        dialogueText.text = "";

        if(!isTransition) DialogueManager.GetInstance().NotifyEndDialogue(currentController);

        if (currentController != null)
        {
            currentController.SetCanPlayerMove(true);
            currentController = null;
        }
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            if (displayLineCoroutine != null)
            {
                StopCoroutine(displayLineCoroutine);
            }

            string nextLine = currentStory.Continue();
            //dialogueText.text = currentStory.Continue();

            if (nextLine.Equals("") && !currentStory.canContinue)
            {
                ExitDialogueMode();
            }

            HandleTags(currentStory.currentTags);
            displayLineCoroutine = StartCoroutine(DisplayLine(nextLine));
        }
        else
        {
            ExitDialogueMode();
        }
    }

    private IEnumerator DisplayLine(string line)
    {
        dialogueText.text = "";

        continueIcon.SetActive(false);
        canContinueToNextLine = false;

        foreach (char letter in line.ToCharArray())
        {
            // Check both controllers, whichever press break
            if ((currentController != null && currentController.GetSubmitPressed()))
            {
                dialogueText.text = line;
                break;
            }

            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        DisplayChoices();
        continueIcon.SetActive(true);
        canContinueToNextLine = true;
    }


    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        if (currentChoices.Count > choices.Length)
        {
            Debug.LogError("More choices were given that the UI can support. Number of choices given: " + currentChoices.Count);
        }

        int index = 0;
        

        foreach (Choice choice in currentChoices)
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            dialogueChoicesPanel.SetActive(true);
            index++;
        }

        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }
        if (currentChoices.Capacity > 0)
        {
            StartCoroutine(SelectFirstChoice());
        }
    }

    private void HandleTags(List<string> currentTags)
    {
        // Loop through each tag and handle it accordingly
        foreach (string tag in currentTags)
        {
            // Parse the tag
            string[] splitTag = tag.Split(':');
            if (splitTag.Length != 2)
            {
                Debug.LogError("Tag could not be appropriately parsed: " + tag);
            }
            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();

            // Handle the tag
            switch (tagKey)
            {
                case SPEAKER_TAG:
                    displayNameText.text = tagValue;
                    break;
                case PORTRAIT_TAG:
                    portraitAnimator.Play(tagValue);
                    break;
                case AUDIO_TAG:
                    if (tagValue == "radio")
                    {
                        //audioManagerInstance.PlayInterference();
                    }
                    break;
                case MOVEMENT_TAG:
                    //playerController.canMove = true;
                    break;
                default:
                    Debug.LogWarning("Tag came in but is not currently being handled: " + tag);
                    break;
            }
        }
    }

    private IEnumerator SelectFirstChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    }

    public void MakeChoice(int choiceIndex)
    {
        dialogueChoicesPanel.SetActive(false);
        currentStory.ChooseChoiceIndex(choiceIndex);
        
        if(currentController != null)
        {
            currentController.RegisterSubmitPressed();
        }

        ContinueStory();
    }

    public bool GetDialogueChoicesActiveStatus()
    {
        return dialogueChoicesPanel.activeInHierarchy;
    }

    public bool AreBothPlayersInDialogue()
    {
        return isPlayer1inDialogue && isPlayer2inDialogue;
    }

}