using System.Collections; // Required for Coroutines
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    public TextMeshProUGUI gameOverText;
    public Button restartButton;
    
    [Header("Fade Settings")]
    public Image backgroundImage;
    public float fadeDuration = 1.5f;

    private void Start()
    {
        restartButton.onClick.AddListener(RestartGame);
    }

    private void OnEnable()
    {
        if (backgroundImage != null)
        {
            StartCoroutine(FadeInBackground());
        }
    }

    private IEnumerator FadeInBackground()
    {
        Color color = backgroundImage.color;
        color.a = 0f;
        backgroundImage.color = color;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            backgroundImage.color = color;

            yield return null;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetState();
        }

        SceneManager.LoadScene("UI_Scene");
    }

    public void SetGameOverText(string newGameOverText)
    {
        gameOverText.text = newGameOverText;
    }

    public void Reset()
    {
        
    }
}