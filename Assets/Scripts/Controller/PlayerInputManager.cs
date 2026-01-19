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



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        //return;

        if (Keyboard.current == null) return;

        if(!wasdJoined && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            var player = PlayerInput.Instantiate(player1Prefab, controlScheme: "WASD", pairWithDevice: Keyboard.current);
        
            if(spawnPoints.Length > 0)
            {
                player.transform.position = spawnPoints[0].position;
            
            }

            GameManager.Instance.players[0] = player.gameObject.GetComponent<PlayerController>();

            if (gamesceneManager != null)
            {
                gamesceneManager.AddPlayer(player.gameObject.GetComponent<PlayerController>(), 0);
                gamesceneManager.CopyInventory(0);
            }

            wasdJoined = true;
        }

        if (!arrowsJoined && Keyboard.current.rightCtrlKey.wasPressedThisFrame)
        {
            var player = PlayerInput.Instantiate(player2Prefab, controlScheme: "Arrows", pairWithDevice: Keyboard.current);

            if (spawnPoints.Length > 0)
            {
                player.transform.position = spawnPoints[1].position;
            }

            GameManager.Instance.players[1] = player.gameObject.GetComponent<PlayerController>();

            if (gamesceneManager != null)
            {
                gamesceneManager.AddPlayer(player.gameObject.GetComponent<PlayerController>(), 1);
                gamesceneManager.CopyInventory(1);

            }

            arrowsJoined = true;
        }

        foreach(var gamePad in Gamepad.all)
        {
            if (gamePad.buttonSouth.wasPressedThisFrame)
            {
                PlayerInput.Instantiate(player1Prefab, controlScheme: "Gamepad", pairWithDevice: gamePad);
            }
        }

    }

    public void InstantiateCharacter(string scheme, int playerIndex)
    {
        if (Keyboard.current == null) return;

        GameObject prefab = playerIndex == 0 ? player1Prefab : player2Prefab ;
        var player = PlayerInput.Instantiate(prefab, controlScheme: scheme, pairWithDevice: Keyboard.current);

        if (spawnPoints.Length > 0)
        {
            player.transform.position = spawnPoints[playerIndex].position;
        }

        if (gamesceneManager != null)
        {
            gamesceneManager.AddPlayer(player.gameObject.GetComponent<PlayerController>(), playerIndex);
        }
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
}
