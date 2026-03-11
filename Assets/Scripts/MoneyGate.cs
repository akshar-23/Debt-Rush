using UnityEngine;

public class MoneyGate : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How much money is needed to open the gate. Set low for playtesting.")]
    public int costToOpen = 100;
    public bool bTakeAllTheMoney = false;
    public GameObject invisibleWall;

    [Header("Level Passed")]
    [Tooltip("Drag the LevelPassed canvas here")]
    public GameObject levelPassedCanvas;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TryToOpenGate();
        }
    }

    public void TryToOpenGate()
    {
        if (bTakeAllTheMoney)
        {
            MoneyManager.Instance.ResetMoneyAmount();
            OpenGate();
            return;
        }

        if (MoneyManager.Instance.GetMoneyAmount() >= costToOpen)
        {
            MoneyManager.Instance.SubtractMoney(costToOpen);
            OpenGate();
        }
        else
        {
            Debug.Log("Not enough money! You need " + costToOpen + ", you have " + MoneyManager.Instance.GetMoneyAmount());
        }
    }

    private void OpenGate()
    {
        if (invisibleWall != null)
        {
            invisibleWall.SetActive(false);
            Debug.Log("Gate Unlocked!");
        }

        // Show level passed screen
        if (levelPassedCanvas != null)
        {
            levelPassedCanvas.SetActive(true);
        }

        gameObject.SetActive(false);
    }
}
