using TMPro;
using UnityEngine;

public class GameplaySceneManager : MonoBehaviour
{
    [SerializeField] private PlayerController _player1;
    [SerializeField] private PlayerController _player2;

    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private float _timer;
 

    void Awake()
    {
        if(_player1 != null)
        {
            _player1.CopyInventory((System.Collections.Generic.List<GameManager.ShopItem>)GameManager.Instance.GetInventory(1));
        }
        if (_player2 != null)
        {
            _player2.CopyInventory((System.Collections.Generic.List<GameManager.ShopItem>)GameManager.Instance.GetInventory(2));
        }
        GameManager.Instance.gameOverUI = _gameOverUI;
        GameManager.Instance.timerText = _timerText;
        GameManager.Instance.checkConditions = true;
        GameManager.Instance.SetTimer(_timer);
    }
}
