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

    public void InstantiateCharacter(string scheme, Gamepad gamePad = null)
    {
        // Determine player index based on control scheme
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

        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;
        
        if (spawnPoints != null && playerIndex < spawnPoints.Length && spawnPoints[playerIndex] != null)
        {
            spawnPosition = spawnPoints[playerIndex].position;
            spawnRotation = spawnPoints[playerIndex].rotation;
        }

        // Instantiate player
        var player = PlayerInput.Instantiate(
            prefabToSpawn,
            controlScheme: scheme, 
            pairWithDevice: gamePad != null ? gamePad : Keyboard.current
        );

        player.transform.position = spawnPosition;
        player.transform.rotation = spawnRotation;

        // Disable/enable CharacterController to ensure position sticks
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.transform.position = spawnPosition;
            cc.enabled = true;
        }

        // Get PlayerController component
        PlayerController pc = player.gameObject.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.playerNumber = playerIndex + 1;
            pc.hudref = gamesceneManager.GetHUD();
        }

        // Register with GameManager
        if (GameManager.Instance != null && GameManager.Instance.players != null)
        {
            GameManager.Instance.players[playerIndex] = pc;
        }

        // Copy inventory from GameManager to this player
        var inventory = new System.Collections.Generic.List<ShopItem>(GameManager.Instance.GetInventory(playerIndex + 1));
        pc.CopyInventory(inventory);

        // Add to scene manager
        if (gamesceneManager != null)
        {
            gamesceneManager.AddPlayer(pc, playerIndex);
        }

        // Assign camera
        GameObject cameraManager = GameObject.Find("CameraManager");
        if (cameraManager != null)
        {
            cameraManager.GetComponent<CameraManager>().AssignTransformPosition(player.transform, playerIndex);
        }
        
        currentNumberPlayers++;
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
