using System.Linq;
using TMPro;
using UnityEngine;

public class GameplaySceneManager : MonoBehaviour
{
    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private float _timer;
    [SerializeField] private Character[] _players;
    [SerializeField] private Character[] _enemies;
    [SerializeField] private TextMeshProUGUI _enemiesText;


    void Awake()
    {
        if (_players[0] != null)
        {
            _players[0].GetComponent<PlayerController>().CopyInventory((System.Collections.Generic.List<GameManager.ShopItem>)GameManager.Instance.GetInventory(1));
        }
        if (_players[1] != null)
        {
            _players[1].GetComponent<PlayerController>().CopyInventory((System.Collections.Generic.List<GameManager.ShopItem>)GameManager.Instance.GetInventory(2));
        }
        GameManager.Instance.gameOverUI = _gameOverUI;
        GameManager.Instance.timerText = _timerText;
        GameManager.Instance.checkConditions = true;
        GameManager.Instance.SetTimer(_timer);
        GameManager.Instance.players = _players;
        GameManager.Instance.enemies = _enemies;
    }

    void FixedUpdate()
    {
        _enemiesText.text = "Enemies: " + _enemies.Count();
    }
}
