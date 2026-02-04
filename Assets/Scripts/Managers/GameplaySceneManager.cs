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
    
    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 15f;
    [SerializeField] private Transform[] respawnPoints;
    
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
        PlayerController pc = player.GetComponent<PlayerController>();
        CharacterController cc = player.GetComponent<CharacterController>();
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        
        // Find and hide health bar
        Transform healthBarCanvas = player.transform.Find("HealthBarCanvas");
        
        // Disable movement and rendering, but keep GameObject active
        if (cc != null)
        {
            cc.enabled = false;
        }
        
        if (pc != null)
        {
            pc.SetCanPlayerMove(false);
            pc.SetCanPlayerAct(false);
        }
        
        // Hide player visually
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }
        
        // Hide health bar
        if (healthBarCanvas != null)
        {
            healthBarCanvas.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(respawnDelay);

        // Determine respawn position
        int playerIndex = player.id;
        Transform respawnTransform = GetRespawnPoint(playerIndex);

        if (respawnTransform != null)
        {
            player.transform.position = respawnTransform.position;
            player.transform.rotation = respawnTransform.rotation;
        }

        // Reset player state
        player.Reset();
        
        // Re-enable CharacterController with position fix
        if (cc != null)
        {
            cc.enabled = false;
            player.transform.position = respawnTransform.position;
            cc.enabled = true;
        }
        
        // Re-enable movement
        if (pc != null)
        {
            pc.SetCanPlayerMove(true);
            pc.SetCanPlayerAct(true);
        }
        
        // Show player visually
        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }
        
        // Show health bar
        if (healthBarCanvas != null)
        {
            healthBarCanvas.gameObject.SetActive(true);
        }
        
        OnPlayerRespawn?.Invoke(player.id, player.transform);
    }

    /// <summary>
    /// Gets the respawn point for a specific player
    /// </summary>
    private Transform GetRespawnPoint(int playerId)
    {
        if (respawnPoints != null && playerId < respawnPoints.Length && respawnPoints[playerId] != null)
        {
            return respawnPoints[playerId];
        }
        
        Debug.LogWarning($"Respawn point for player {playerId} not found!");
        return transform;
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
