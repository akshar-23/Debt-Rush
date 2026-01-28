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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentNumberPlayers = 0;
    }

    // Update is called once per frame
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
        var player = PlayerInput.Instantiate(GetPlayerPrefab(currentNumberPlayers), controlScheme: scheme, pairWithDevice: gamePad != null ? gamePad : Keyboard.current);

        if (spawnPoints != null && currentNumberPlayers < spawnPoints.Length)
        {
            player.transform.position = spawnPoints[currentNumberPlayers].position;
        }

        GameManager.Instance.players[currentNumberPlayers] = player.gameObject.GetComponent<PlayerController>();

        if (gamesceneManager != null)
        {
            gamesceneManager.AddPlayer(player.gameObject.GetComponent<PlayerController>(), currentNumberPlayers);
            gamesceneManager.CopyInventory(currentNumberPlayers);
        }

        GameObject cameraManager = GameObject.Find("CameraManager");
        if (cameraManager != null)
        {
            cameraManager.GetComponent<CameraManager>().AssignTransformPosition(player.transform, currentNumberPlayers);
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
