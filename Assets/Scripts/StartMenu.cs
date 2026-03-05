using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    public Button[] menuButtons;

    private int currentIndex = 0;

    void Start()
    {
        UpdateSelection();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            currentIndex++;
            if (currentIndex >= menuButtons.Length)
                currentIndex = 0;

            UpdateSelection();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = menuButtons.Length - 1;

            UpdateSelection();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            menuButtons[currentIndex].onClick.Invoke();
        }
    }

    void UpdateSelection()
    {
        EventSystem.current.SetSelectedGameObject(menuButtons[currentIndex].gameObject);
    }

    public void LoadShopScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
