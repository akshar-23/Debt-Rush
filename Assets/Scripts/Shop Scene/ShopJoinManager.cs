using UnityEngine;
using UnityEngine.InputSystem;

public class ShopJoinManager : MonoBehaviour
{
    public static ShopJoinManager Instance { get; private set; }

    public bool player1Joined { get; private set; } = false;
    public bool player2Joined { get; private set; } = false;

    public System.Action OnPlayer1Joined;
    public System.Action OnPlayer2Joined;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (player1Joined && player2Joined) return;

        if (Keyboard.current != null)
        {
            if (!player1Joined && Keyboard.current.spaceKey.wasPressedThisFrame)
                JoinPlayer(1, "WASD", null);

            if (!player2Joined && Keyboard.current.rightCtrlKey.wasPressedThisFrame)
                JoinPlayer(2, "Arrows", null);
        }

        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            var gp = Gamepad.all[i];
            if (!gp.buttonSouth.wasPressedThisFrame) continue;

            if (i == 0 && !player1Joined)
                JoinPlayer(1, "GamePad", gp);
            else if (i == 1 && !player2Joined)
                JoinPlayer(2, "GamePad", gp);
            else if (Gamepad.all.Count == 1 && player1Joined && !player2Joined)
                JoinPlayer(2, "GamePad", gp);
        }
    }

    void JoinPlayer(int playerNumber, string scheme, Gamepad gamepad)
    {
        int idx = playerNumber - 1;
        GameManager.Instance.playerSchemes[idx] = scheme;
        GameManager.Instance.playerGamepads[idx] = gamepad;

        if (playerNumber == 1)
        {
            player1Joined = true;
            OnPlayer1Joined?.Invoke();
        }
        else
        {
            player2Joined = true;
            OnPlayer2Joined?.Invoke();
        }
    }

    public void ResetJoin()
    {
        player1Joined = false;
        player2Joined = false;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerSchemes = new string[2];
            GameManager.Instance.playerGamepads = new Gamepad[2];
        }
    }
}
