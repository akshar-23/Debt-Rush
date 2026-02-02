using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;
    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private GameplaySceneManager gamesceneManager;

    private bool wasdJoined = false;
    private bool arrowsJoined = false;
    private bool gamepadJoined = false;

    private const int MaxNumberPlayers = 2;
    private int currentNumberPlayers;

    void Start()
    {
        currentNumberPlayers = 0;
    }

    void Update()
    {
        if (Keyboard.current == null || currentNumberPlayers >= 2) return;

        if(!wasdJoined && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            InstantiateCharacter("WASD");
            wasdJoined = true;
        }

        if (!arrowsJoined && Keyboard.current.rightCtrlKey.wasPressedThisFrame)
        {
            InstantiateCharacter("Arrows");
            arrowsJoined = true;
        }

        foreach(var gamePad in Gamepad.all)
        {
            if (gamePad.buttonSouth.wasPressedThisFrame)
            {
                InstantiateCharacter("GamePad", gamePad);
                if (currentNumberPlayers == 2) gamepadJoined = true;
            }
        }
    }

    /// <summary>
    /// Spawns a player character with the specified control scheme.
    /// Player assignment is based on control scheme rather than join order.
    /// </summary>
    public void InstantiateCharacter(string scheme, Gamepad gamePad = null)
    {
        // Determine player index and prefab based on control scheme
        int playerIndex;
        GameObject prefabToSpawn;
        
        if (scheme == "WASD")
        {
            playerIndex = 0;
            prefabToSpawn = player1Prefab;
        }
        else if (scheme == "Arrows")
        {
            playerIndex = 1;
            prefabToSpawn = player2Prefab;
        }
        else // Gamepad
        {
            // Assign gamepad to first available player slot
            if (!wasdJoined)
            {
                playerIndex = 0;
                prefabToSpawn = player1Prefab;
                wasdJoined = true;
            }
            else if (!arrowsJoined)
            {
                playerIndex = 1;
                prefabToSpawn = player2Prefab;
                arrowsJoined = true;
            }
            else
            {
                Debug.LogWarning("Both players already joined!");
                return;
            }
        }

        // Retrieve spawn transform for the specified player index
        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;
        
        if (spawnPoints != null && playerIndex < spawnPoints.Length && spawnPoints[playerIndex] != null)
        {
            spawnPosition = spawnPoints[playerIndex].position;
            spawnRotation = spawnPoints[playerIndex].rotation;
        }
        else
        {
            Debug.LogWarning($"Spawn point for player {playerIndex} is missing! Using default position.");
        }

        // Instantiate player with assigned control scheme
        var player = PlayerInput.Instantiate(
            prefabToSpawn,
            controlScheme: scheme, 
            pairWithDevice: gamePad != null ? gamePad : Keyboard.current
        );

        player.transform.position = spawnPosition;
        player.transform.rotation = spawnRotation;

        // Temporarily disable CharacterController to ensure position is applied correctly
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.transform.position = spawnPosition;
            cc.enabled = true;
        }

        // Configure player controller
        PlayerController pc = player.gameObject.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.playerNumber = playerIndex + 1;
        }

        // Register player with game systems
        if (GameManager.Instance != null && GameManager.Instance.players != null)
        {
            GameManager.Instance.players[playerIndex] = pc;
        }

        if (gamesceneManager != null)
        {
            gamesceneManager.AddPlayer(pc, playerIndex);
            gamesceneManager.CopyInventory(playerIndex);
        }

        // Assign camera tracking
        GameObject cameraManager = GameObject.Find("CameraManager");
        if (cameraManager != null)
        {
            cameraManager.GetComponent<CameraManager>().AssignTransformPosition(player.transform, playerIndex);
        }
        
        currentNumberPlayers++;
    }

    public void InstantiateCharacters()
    {
        if (Keyboard.current == null) return;

        if (!wasdJoined)
        {
            var player = PlayerInput.Instantiate(player1Prefab, controlScheme: "WASD", pairWithDevice: Keyboard.current);
            
            if (spawnPoints.Length > 0)
            {
                player.transform.position = spawnPoints[0].position;
                
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false;
                    player.transform.position = spawnPoints[0].position;
                    cc.enabled = true;
                }
            }

            if (gamesceneManager != null)
            {
                gamesceneManager.AddPlayer(player.gameObject.GetComponent<PlayerController>(), 0);
            }

            wasdJoined = true;
        }

        if (!arrowsJoined)
        {
            var player = PlayerInput.Instantiate(player2Prefab, controlScheme: "Arrows", pairWithDevice: Keyboard.current);

            if (spawnPoints.Length > 0)
            {
                player.transform.position = spawnPoints[1].position;
                
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false;
                    player.transform.position = spawnPoints[1].position;
                    cc.enabled = true;
                }
            }

            if (gamesceneManager != null)
            {
                gamesceneManager.AddPlayer(player.gameObject.GetComponent<PlayerController>(), 1);
            }

            arrowsJoined = true;
        }
    }

    public void SetGameSceneManager(GameplaySceneManager _gamesceneManager)
    {
        gamesceneManager = _gamesceneManager;
    }

    private GameObject GetPlayerPrefab(int playerIndex) 
    {
        if(playerIndex == 0) 
        {
            return player1Prefab;
        }
        else if (playerIndex == 1)
        {
            return player2Prefab;
        }
        else
        {
            return null;
        }
    }
}
