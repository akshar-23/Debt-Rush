using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to the GameOver UI prefab/panel.
/// Assign scene names and EventSystem references in the Inspector.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameOverText;

    [Tooltip("The name of the UI/Shop scene (objectives + shop phase).")]
    [SerializeField] private string uiSceneName = "UI_Scene";

    [Tooltip("The name of the Start Menu scene.")]
    [SerializeField] private string startSceneName = "StartMenu";

    [Header("Event Systems")]
    [Tooltip("Drag EventSystem_Shared here")]
    [SerializeField] private GameObject eventSystemShared;
    [Tooltip("Drag EventSystem_P1 here")]
    [SerializeField] private GameObject eventSystemP1;
    [Tooltip("Drag EventSystem_P2 here")]
    [SerializeField] private GameObject eventSystemP2;

    [Header("Input Settings")]
    [Tooltip("How long to wait before accepting input, to avoid accidental restart")]
    [SerializeField] private float inputDelay = 0.5f;

    private float enabledTime;
    private bool isActive = false;

    private void OnEnable()
    {
        SwitchToSharedEventSystem();
        enabledTime = Time.unscaledTime;
        isActive = true;
    }

    private void OnDisable()
    {
        isActive = false;
    }

    private void Update()
    {
        if (!isActive) return;

        // Wait a short delay to avoid instantly restarting
        if (Time.unscaledTime - enabledTime < inputDelay) return;

        if (AnyPlayerPressedConfirm())
            OnRestartPressed();
    }

    private bool AnyPlayerPressedConfirm()
    {
        // Keyboard P1: Space
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            return true;

        // Keyboard P2: RightCtrl
        if (Keyboard.current != null && Keyboard.current.rightCtrlKey.wasPressedThisFrame)
            return true;

        // Enter key (works for either player)
        if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            return true;

        // Any gamepad South button (A on Xbox, Cross on PS)
        foreach (var gp in Gamepad.all)
        {
            if (gp.buttonSouth.wasPressedThisFrame)
                return true;
        }

        return false;
    }

    private void SwitchToSharedEventSystem()
    {
        if (eventSystemShared == null)
            eventSystemShared = GameObject.Find("EventSystem_Shared");
        if (eventSystemP1 == null)
            eventSystemP1 = GameObject.Find("EventSystem_P1");
        if (eventSystemP2 == null)
            eventSystemP2 = GameObject.Find("EventSystem_P2");

        if (eventSystemP1 != null) eventSystemP1.SetActive(false);
        if (eventSystemP2 != null) eventSystemP2.SetActive(false);
        if (eventSystemShared != null) eventSystemShared.SetActive(true);
    }

    public void SetGameOverText(string text)
    {
        if (gameOverText != null)
            gameOverText.text = text;
    }

    /// <summary>
    /// Called by the Restart button (mouse click) OR automatically by any player input.
    /// </summary>
    public void OnRestartPressed()
    {
        isActive = false; // prevent double-trigger
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.ResetStateForRestart();

        if (MoneyManager.Instance != null)
            MoneyManager.Instance.ResetState();

        if (ShopJoinManager.Instance != null)
            ShopJoinManager.Instance.RestoreJoinFromGameManager();

        SceneManager.LoadScene(uiSceneName);
    }

    /// <summary>
    /// Called by the Main Menu button. Full reset back to start.
    /// </summary>
    public void OnMainMenuPressed()
    {
        isActive = false;
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.ResetState();

        if (MoneyManager.Instance != null)
            MoneyManager.Instance.ResetState();

        if (ShopJoinManager.Instance != null)
            ShopJoinManager.Instance.ResetJoin();

        SceneManager.LoadScene(startSceneName);
    }
}
