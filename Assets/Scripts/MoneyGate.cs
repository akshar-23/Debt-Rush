using UnityEngine;

public class MoneyGate : MonoBehaviour
{
    [Header("Settings")]
    public int costToOpen = 500;
    public GameObject invisibleWall;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TryToOpenGate(other.gameObject);
        }
    }

    private void TryToOpenGate(GameObject playerObj)
    {

        if (MoneyManager.Instance.GetMoneyAmount() > 0)
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
