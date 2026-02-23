using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerUIBinder : MonoBehaviour
{
    [HideInInspector] public MultiplayerEventSystem eventSystem;
    [HideInInspector] public Canvas playerCanvas;
    [HideInInspector] public GameObject firstSelected;

    private PlayerInput _pi;

    void Awake()
    {
        _pi = GetComponent<PlayerInput>();
    }

    public void Bind()
    {
        if (eventSystem == null || playerCanvas == null || _pi == null) return;

        // Limit UI to this player's canvas
        eventSystem.playerRoot = playerCanvas.gameObject;
        eventSystem.firstSelectedGameObject = firstSelected;
        eventSystem.SetSelectedGameObject(firstSelected);

        // Configure this EventSystem's UI module to use this player's actions
        var ui = eventSystem.GetComponent<InputSystemUIInputModule>();
        ui.actionsAsset = _pi.actions;
        ui.move = InputActionReference.Create(_pi.actions["Navigate"]);
        ui.submit = InputActionReference.Create(_pi.actions["Submit"]);
        ui.cancel = InputActionReference.Create(_pi.actions["Cancel"]);
        ui.point = InputActionReference.Create(_pi.actions["Point"]);
        ui.leftClick = InputActionReference.Create(_pi.actions["Click"]);
    }
}
