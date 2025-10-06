using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameOverScreen : MonoBehaviour
{
    public TextMeshProUGUI gameOverText;
    public Button restartButton;

    private void Start()
    {
        restartButton.onClick.AddListener(RestartGame);    
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("UI_Scene");
    }
}
