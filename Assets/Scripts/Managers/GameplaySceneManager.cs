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


    void Awake()
    {
        // Do we need this block of code? 
        /*
        GameObject cManager = GameObject.Find("PlayerInputManager");

        if(cManager != null)
        {
            cManager.GetComponent<PlayerInputManager>().SetGameSceneManager(this);
            //cManager.GetComponent<PlayerInputManager>().InstantiateCharacters();
            //cManager.GetComponent<PlayerInputManager>().InstantiateCharacter("WASD", 0);
            //cManager.GetComponent<PlayerInputManager>().InstantiateCharacter("Arrows", 1);
        }
        
        GameObject cameraManager = GameObject.Find("CameraManager");
        if (cameraManager != null)
        {
            cameraManager.GetComponent<CameraManager>().AssignTransformPosition(_players[0].transform, 0);
            cameraManager.GetComponent<CameraManager>().AssignTransformPosition(_players[1].transform, 1);
        }*/


        if (_players[0] != null)
        {
            _players[0].GetComponent<PlayerController>().CopyInventory((System.Collections.Generic.List<ShopItem>)GameManager.Instance.GetInventory(1));
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
        
        if (player != null && AtleastOnePlayerIsAlive())
        {
            _respawnRoutines[player] = StartCoroutine(RespawnCoroutine(player));
        }
        else
        {
            StopAllCoroutines();
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

        Debug.LogWarning("Player Respawning in " + respawnDelay + " seconds...");

        yield return new WaitForSeconds(respawnDelay);

        Debug.LogWarning("Respawning player!");

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
        _players[playerIndex].GetComponent<PlayerController>().CopyInventory((System.Collections.Generic.List<ShopItem>)GameManager.Instance.GetInventory(playerIndex + 1));
    }
}
