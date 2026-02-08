using UnityEngine;

public class MoneyGate : MonoBehaviour
{
    [Header("Settings")]
    public int costToOpen = 500;
    public bool bTakeAllTheMoney = false;
    public GameObject invisibleWall;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TryToOpenGate();
        }
    }

    //private void TryToOpenGate(GameObject playerObj)
    public void TryToOpenGate()
    {
        if (bTakeAllTheMoney)
        {
            MoneyManager.Instance.ResetMoneyAmount();
            OpenGate();
        }

        if (MoneyManager.Instance.GetMoneyAmount() > costToOpen)
        {
            MoneyManager.Instance.SubtractMoney(MoneyManager.Instance.GetMoneyAmount());
            OpenGate();
        }
        else
        {
            Debug.Log("Not enough money! You need " + costToOpen);
        }
    }

    private void OpenGate()
    {
        if (invisibleWall != null)
        {
            invisibleWall.SetActive(false);
            Debug.Log("Gate Unlocked!");
        }

        gameObject.SetActive(false);
    }
}
