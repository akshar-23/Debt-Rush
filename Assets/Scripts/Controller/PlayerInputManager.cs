using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerInputManager : MonoBehaviour
{
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header ("UI")]
    [SerializeField] public MultiplayerEventSystem eventSystemP1;
    [SerializeField] public Canvas canvasP1;
    [SerializeField] public GameObject p1FirstButton;

    [SerializeField] public MultiplayerEventSystem eventSystemP2;
    [SerializeField] public Canvas canvasP2;
    [SerializeField] public GameObject p2FirstButton;

    [SerializeField] private GameplaySceneManager gamesceneManager;

    private bool wasdJoined = false;
    private bool arrowsJoined = false;
    private bool gamepadJoined = false;

    private const int MaxNumberPlayers = 2;
    private int currentNumberPlayers;

    void Start()
    {
        currentNumberPlayers = 0;

        // If players already joined in the shop scene, auto-instantiate both
        if (GameManager.Instance != null &&
            !string.IsNullOrEmpty(GameManager.Instance.playerSchemes[0]) &&
            !string.IsNullOrEmpty(GameManager.Instance.playerSchemes[1]))
        {
            AutoInstantiateFromShop();
        }
    }

    void AutoInstantiateFromShop()
    {
        for (int i = 0; i < 2; i++)
        {
            string scheme = GameManager.Instance.playerSchemes[i];
            Gamepad gp    = GameManager.Instance.playerGamepads[i];
            InstantiateCharacter(scheme, gp, forcedPlayerIndex: i);
        }
    }

    void Update()
    {
        // Skip join detection if both players already joined in shop
        if (GameManager.Instance != null &&
            !string.IsNullOrEmpty(GameManager.Instance.playerSchemes[0]) &&
            !string.IsNullOrEmpty(GameManager.Instance.playerSchemes[1]))
            return;

        if (currentNumberPlayers >= 2) return;

        if (Keyboard.current != null)
        {
            // Space = WASD player join (matches Submit binding in action map)
            if (!wasdJoined && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                wasdJoined = true;
                InstantiateCharacter("WASD");
            }

            // RightCtrl = Arrows player join (matches Submit binding in action map)
            if (!arrowsJoined && Keyboard.current.rightCtrlKey.wasPressedThisFrame)
            {
                arrowsJoined = true;
                InstantiateCharacter("Arrows");
            }
        }

        // Gamepad: first unclaimed south press joins
        foreach (var gamePad in Gamepad.all)
        {
            if (gamePad.buttonSouth.wasPressedThisFrame && !gamepadJoined)
            {
                gamepadJoined = true;
                InstantiateCharacter("GamePad", gamePad);
                break;
            }
        }
    }

    public void InstantiateCharacter(string scheme, Gamepad gamePad = null, int forcedPlayerIndex = -1)
    {
        // Determine player index based on control scheme
        int playerIndex;
        GameObject prefabToSpawn;

        if (forcedPlayerIndex >= 0)
        {
            // Index was determined at join time (e.g. from shop scene)
            playerIndex = forcedPlayerIndex;
            prefabToSpawn = playerIndex == 0 ? player1Prefab : player2Prefab;
        }
        else if (scheme == "WASD")
        {
            playerIndex = 0;
            prefabToSpawn = player1Prefab;
        }
        else if (scheme == "Arrows")
        {
            playerIndex = 1;
            prefabToSpawn = player2Prefab;
        }
        else // Gamepad - live join, determine by available slot
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

        var player = PlayerInput.Instantiate(
            prefabToSpawn,
            controlScheme: scheme,
            pairWithDevices: gamePad != null ? gamePad : Keyboard.current
        );

        int idx = playerIndex; // authoritative index

        // Store join data in GameManager so shop scene can read it without players existing
        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerSchemes[idx] = scheme;
            GameManager.Instance.playerGamepads[idx] = gamePad;
        }

        // Use idx for spawn, cameras, arrays, HUD, etc.
        Vector3 spawnPosition = (spawnPoints != null && idx < spawnPoints.Length && spawnPoints[idx] != null)
            ? spawnPoints[idx].position
            : Vector3.zero;
        Quaternion spawnRotation = (spawnPoints != null && idx < spawnPoints.Length && spawnPoints[idx] != null)
            ? spawnPoints[idx].rotation
            : Quaternion.identity;

        player.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.transform.position = spawnPosition;
            cc.enabled = true;
        }

        var pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.playerNumber = idx + 1;
            pc.hudref = gamesceneManager.GetHUD();
        }

        if (GameManager.Instance != null && GameManager.Instance.players != null)
            GameManager.Instance.players[idx] = pc;

        var inventory = new List<ShopItem>(GameManager.Instance.GetInventory(idx + 1));
        pc.CopyInventory(inventory);

        if (gamesceneManager != null)
        {
            gamesceneManager.AddPlayer(pc, idx);
            gamesceneManager.AssignArrows();
        }

        var cameraManager = GameObject.Find("CameraManager");
        if (cameraManager != null)
            cameraManager.GetComponent<CameraManager>().AssignTransformPosition(player.transform, idx);

        var binder = player.GetComponent<PlayerUIBinder>();
        if (binder != null)
        {
            // Decide which UI stack this PlayerInput uses (by playerIndex or by scheme)
            if (idx == 0)
            {
                binder.eventSystem = eventSystemP1;
                binder.playerCanvas = canvasP1;
                binder.firstSelected = p1FirstButton;
            }
            else if (idx == 1)
            {
                binder.eventSystem = eventSystemP2;
                binder.playerCanvas = canvasP2;
                binder.firstSelected = p2FirstButton;
            }

            binder.Bind();
        }

        // Clear any stale submit press from the join button
        if (pc != null) pc.RegisterSubmitPressed();

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
