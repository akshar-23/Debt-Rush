using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    [Header("Params")]
    [SerializeField] private float typingSpeed = 0.04f;
    [SerializeField] public PlayerController playerController1;
    [SerializeField] public PlayerController playerController2;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private GameObject continueIcon;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    [Header("Visual")]
    [SerializeField] private GameObject visualPortrait;
    [SerializeField] private GameObject visualRadar;

    private Story currentStory;

    private const string SPEAKER_TAG = "speaker";
    private const string PORTRAIT_TAG = "portrait";
    private const string RADAR_TAG = "radar";
    private const string AUDIO_TAG = "audio";
    private const string MOVEMENT_TAG = "movement";

    public bool dialogueIsPlaying { get; private set; }
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
        dialoguePanel.SetActive(false);

        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;

        if (visualPortrait != null)
        {
            visualPortrait.SetActive(false);
        }

        if (visualRadar != null)
        {
            visualRadar.SetActive(false);
        }

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
        if (canContinueToNextLine && playerController1.GetInteractPressed())
        {
            ContinueStory();
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);

        //currentStory.BindExternalFunction("sendFearValue", (int newFearValue) => {
        //    fearValue = newFearValue;
        //});

        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);
        //playerController.canMove = false;

        ContinueStory();
    }

    public void EnterDialogueMode(TextAsset inkJSON, PlayerController playerController)
    {
        playerController1 = playerController;

        currentStory = new Story(inkJSON.text);

        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);
        playerController1.SetCanPlayerMove(false);

        ContinueStory();
    }

    private void ExitDialogueMode()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        if (visualPortrait != null)
        {
            visualPortrait.SetActive(false);
        }
        dialogueText.text = "";
        playerController1.SetCanPlayerMove(true);
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            if (displayLineCoroutine != null)
            {
                StopCoroutine(displayLineCoroutine);
            }

            displayLineCoroutine = StartCoroutine(DisplayLine(currentStory.Continue()));
            //dialogueText.text = currentStory.Continue();

            DisplayChoices();

            HandleTags(currentStory.currentTags);
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
            
            if (playerController1.GetInteractPressed())
            {
                dialogueText.text = line;
                break;
            }

            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

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
                    if (tagValue == "ON")
                    {
                        visualPortrait.SetActive(true);
                    }
                    else
                    {
                        visualPortrait.SetActive(false);
                    }
                    break;
                case RADAR_TAG:
                    if (tagValue == "ON")
                    {
                        visualRadar.SetActive(true);
                    }
                    else
                    {
                        visualRadar.SetActive(false);
                    }
                    break;
                case AUDIO_TAG:
                    if (tagValue == "radio")
                    {
                        //audioManagerInstance.PlayInterference();
                    }
                    else if (tagValue == "helloelias")
                    {
                        //audioManagerInstance.PlayHelloElias();
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
        currentStory.ChooseChoiceIndex(choiceIndex);
    }

}