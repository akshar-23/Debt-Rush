using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private GameObject continueIcon;

    [Header("Portrait UI")]
    [SerializeField] private Image npcPortraitImage;
    [SerializeField] private Image playerPortraitImage;
    [SerializeField] public Sprite p1Portrait;
    [SerializeField] public Sprite p2Portrait;

    [Header("Choices UI")]
    [SerializeField] private GameObject dialogueChoicesPanel;
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    private Story currentStory;
    private Sprite npcPortraitSprite;
    private int currentPlayerNumber;

    private const string SPEAKER_TAG = "speaker";
    private const string AUDIO_TAG = "audio";
    private const string MOVEMENT_TAG = "movement";
    private const string NOSKIP_TAG = "noskip";

    private bool noSkipCurrentLine = false;

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
        if (!dialogueIsPlaying) return;

        if (canContinueToNextLine 
            && currentController != null 
            && currentController.GetSubmitPressed()
            && !dialogueChoicesPanel.activeInHierarchy)
        {
            ContinueStory();
        }
    }

    public Story GetCurrentStory() => currentStory;
    public PlayerController GetCurrentController() => currentController;

    public void UpdateCurrentStory(Story _currentStory, PlayerController _currentController)
    {
        dialoguePanel.SetActive(true);
        currentStory = _currentStory;
        dialogueIsPlaying = true;
        dialogueText.text = currentStory.currentText;
        currentController = _currentController;
        currentController.SetCanPlayerMove(false);

        HandleTags(currentStory.currentTags);
        displayLineCoroutine = StartCoroutine(DisplayLine(currentStory.currentText));
    }

    public void EnterDialogueMode(TextAsset inkJSON, PlayerController playerController, GameObject npcController, Sprite portrait = null)
    {
        currentController = playerController;
        currentPlayerNumber = playerController.playerNumber;
        currentStory = new Story(inkJSON.text);

        // Store NPC portrait
        npcPortraitSprite = portrait;

        // Set player portrait based on who triggered the dialogue
        if (playerPortraitImage != null)
        {
            playerPortraitImage.sprite = currentPlayerNumber == 1 ? p1Portrait : p2Portrait;
            playerPortraitImage.gameObject.SetActive(false); // hidden until speaker tag says so
        }

        // Set NPC portrait
        if (npcPortraitImage != null)
        {
            npcPortraitImage.sprite = npcPortraitSprite;
            npcPortraitImage.gameObject.SetActive(npcPortraitSprite != null);
        }

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

        if (!isTransition) DialogueManager.GetInstance().NotifyEndDialogue(currentController);

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
                StopCoroutine(displayLineCoroutine);

            string nextLine = currentStory.Continue();

            if (nextLine.Equals("") && !currentStory.canContinue)
            {
                ExitDialogueMode();
                return;
            }

            if (nextLine.Equals(""))
            {
                ContinueStory();
                return;
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
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;

        continueIcon.SetActive(false);
        canContinueToNextLine = false;

        dialogueText.ForceMeshUpdate();
        int totalVisibleChars = dialogueText.textInfo.characterCount;

        int visibleCount = 0;
        while (visibleCount <= totalVisibleChars)
        {
            if (!noSkipCurrentLine && currentController != null && currentController.GetSubmitPressed())
            {
                dialogueText.maxVisibleCharacters = totalVisibleChars;
                break;
            }

            dialogueText.maxVisibleCharacters = visibleCount;
            visibleCount++;
            yield return new WaitForSeconds(typingSpeed);
        }

        dialogueText.maxVisibleCharacters = totalVisibleChars;
        DisplayChoices();
        continueIcon.SetActive(true);
        canContinueToNextLine = true;
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        if (currentChoices.Count > choices.Length)
            Debug.LogError("More choices were given that the UI can support. Number of choices given: " + currentChoices.Count);

        int index = 0;
        foreach (Choice choice in currentChoices)
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            dialogueChoicesPanel.SetActive(true);
            index++;
        }

        for (int i = index; i < choices.Length; i++)
            choices[i].gameObject.SetActive(false);

        if (currentChoices.Capacity > 0)
            StartCoroutine(SelectFirstChoice());
    }

    private void HandleTags(List<string> currentTags)
    {
        noSkipCurrentLine = false;
        foreach (string tag in currentTags)
        {
            string[] splitTag = tag.Split(':');
            if (splitTag.Length != 2)
            {
                Debug.LogError("Tag could not be appropriately parsed: " + tag);
                continue;
            }
            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim().ToLower();

            switch (tagKey)
            {
                case SPEAKER_TAG:
                    displayNameText.text = splitTag[1].Trim();
                    switch (tagValue)
                    {
                        case "npc":
                        case "gatekeeper":
                        case "the port":
                            if (npcPortraitImage != null)    npcPortraitImage.gameObject.SetActive(true);
                            if (playerPortraitImage != null) playerPortraitImage.gameObject.SetActive(false);
                            break;
                        case "player":
                        case "you":
                            if (npcPortraitImage != null)    npcPortraitImage.gameObject.SetActive(false);
                            if (playerPortraitImage != null) playerPortraitImage.gameObject.SetActive(true);
                            break;
                        case "initiator":
                            displayNameText.text = currentPlayerNumber == 1 ? "Player1" : "Player2";
                            if (playerPortraitImage != null)
                            {
                                playerPortraitImage.sprite = currentPlayerNumber == 1 ? p1Portrait : p2Portrait;
                                playerPortraitImage.gameObject.SetActive(true);
                            }
                            if (npcPortraitImage != null) npcPortraitImage.gameObject.SetActive(false);
                            break;
                        case "partner":
                            displayNameText.text = currentPlayerNumber == 1 ? "Player2" : "Player1";
                            if (playerPortraitImage != null)
                            {
                                playerPortraitImage.sprite = currentPlayerNumber == 1 ? p2Portrait : p1Portrait;
                                playerPortraitImage.gameObject.SetActive(true);
                            }
                            if (npcPortraitImage != null) npcPortraitImage.gameObject.SetActive(false);
                            break;
                        case "player1":
                            if (playerPortraitImage != null)
                            {
                                playerPortraitImage.sprite = p1Portrait;
                                playerPortraitImage.gameObject.SetActive(true);
                            }
                            if (npcPortraitImage != null) npcPortraitImage.gameObject.SetActive(false);
                            break;
                        case "player2":
                            if (playerPortraitImage != null)
                            {
                                playerPortraitImage.sprite = p2Portrait;
                                playerPortraitImage.gameObject.SetActive(true);
                            }
                            if (npcPortraitImage != null) npcPortraitImage.gameObject.SetActive(false);
                            break;
                    }
                    break;
                case AUDIO_TAG:
                    break;
                case MOVEMENT_TAG:
                    break;
                case NOSKIP_TAG:
                    noSkipCurrentLine = (tagValue == "true");
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
        ContinueStory();
        if (currentController != null)
            currentController.RegisterSubmitPressed();
    }

    public bool GetDialogueChoicesActiveStatus() => dialogueChoicesPanel.activeInHierarchy;
    public bool AreBothPlayersInDialogue() => isPlayer1inDialogue && isPlayer2inDialogue;
}
