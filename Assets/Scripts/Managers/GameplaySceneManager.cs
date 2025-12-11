using System.Linq;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameplaySceneManager : MonoBehaviour
{
    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private Character[] _players;
    [SerializeField] private Character[] _enemies;
    [SerializeField] private TextMeshProUGUI _enemiesText;
    private float respawnDelay = 5f;
    [SerializeField] private Transform respawnPoint;
    public static event System.Action<int, Transform> OnPlayerRespawn;



    void Awake()
    {
        if (_players[0] != null)
        {
            //_players[0].GetComponent<PlayerController>().CopyInventory((System.Collections.Generic.List<ShopItem>)GameManager.Instance.GetInventory(1));
        }
        if (_players[1] != null)
        {
            _players[1].GetComponent<PlayerController>().CopyInventory((System.Collections.Generic.List<ShopItem>)GameManager.Instance.GetInventory(2));
        }
        GameManager.Instance.gameOverUI = _gameOverUI;
        GameManager.Instance.checkConditions = true;
        GameManager.Instance.players = _players;
        GameManager.Instance.enemies = _enemies;
    }

    void FixedUpdate()
    {
        _enemiesText.text = "Enemies: " + _enemies.Count();
    }

    private void OnEnable()
    {
        PlayerController.OnPlayerDied += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerDied -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath(int _id)
    {
        Character player = _players.FirstOrDefault(p => p.id == _id);
        if (player != null)
        {
            StartCoroutine(RespawnCoroutine(player));
        }
    }

    private IEnumerator RespawnCoroutine(Character player)
    {
        player.gameObject.GetComponent<CharacterController>().enabled = false;

        Debug.Log("Player Respawning in " + respawnDelay + " seconds...");

        yield return new WaitForSeconds(respawnDelay);

        Debug.Log("Respawning player!");

        player.transform.position = respawnPoint.position;
        player.transform.rotation = respawnPoint.rotation;

        player.gameObject.GetComponent<CharacterController>().enabled = true;
        player.Reset();
        player.gameObject.SetActive(true);
        OnPlayerRespawn?.Invoke(player.id, player.transform);
    }
}
