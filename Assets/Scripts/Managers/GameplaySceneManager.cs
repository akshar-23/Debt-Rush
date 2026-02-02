using System.Linq;
using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class GameplaySceneManager : MonoBehaviour
{
    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private Character[] _players;
    [SerializeField] private Character[] _enemies;
    [SerializeField] private TextMeshProUGUI _enemiesText;
    private float respawnDelay = 5f;
    [SerializeField] private Transform respawnPoint;
    public static event System.Action<int, Transform> OnPlayerRespawn;
    private Dictionary<Character, Coroutine> _respawnRoutines = new Dictionary<Character, Coroutine>();

    [Header("Inventory")]
    [SerializeField] private List<ShopItem> inventoryP1 = new List<ShopItem>();
    [SerializeField] private List<ShopItem> inventoryP2 = new List<ShopItem>();

    void Awake()
    {
        // Add items from inventory lists to GameManager
        foreach (var item in inventoryP1)
        {
            if (item != null)
            {
                GameManager.Instance.AddToInventory(1, item);
            }
        }

        foreach (var item in inventoryP2)
        {
            if (item != null)
            {
                GameManager.Instance.AddToInventory(2, item);
            }
        }

        // Update lists to show what GameManager actually has (including shop items)
        inventoryP1 = new List<ShopItem>(GameManager.Instance.GetInventory(1));
        inventoryP2 = new List<ShopItem>(GameManager.Instance.GetInventory(2));

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
        Character.OnPlayerDied += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        Character.OnPlayerDied -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath(int _id)
    {
        Character player = _players.FirstOrDefault(p => p.id == _id);
        
        if (player != null && AtleastOnePlayerIsAlive())
        {
            _respawnRoutines[player] = StartCoroutine(RespawnCoroutine(player));
        }
        else
        {
            foreach (var routine in _respawnRoutines.Values)
            {
                if (routine != null)
                    StopCoroutine(routine);
            }
            _respawnRoutines.Clear();
        }
    }

    private bool AtleastOnePlayerIsAlive()
    {
        return _players.Any(p => !p.isDead);
    }

    private IEnumerator RespawnCoroutine(Character player)
    {
        player.gameObject.GetComponent<CharacterController>().enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        player.transform.position = respawnPoint.position;
        player.transform.rotation = respawnPoint.rotation;

        player.gameObject.GetComponent<CharacterController>().enabled = true;
        player.Reset();
        player.gameObject.SetActive(true);
        OnPlayerRespawn?.Invoke(player.id, player.transform);
    }

    public void AddPlayer(Character player, int pos)
    {
        _players[pos] = player;
    }

    public void CopyInventory(int playerIndex)
    {
        _players[playerIndex].GetComponent<PlayerController>().CopyInventory(
            new List<ShopItem>(GameManager.Instance.GetInventory(playerIndex + 1))
        );
    }
}
