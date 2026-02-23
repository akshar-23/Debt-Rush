using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public static class UIBinder
{
    public static void BindToPlayer(
        MultiplayerEventSystem eventSystem,
        InputSystemUIInputModule uiModule,
        PlayerInput player,
        GameObject playerRoot,
        GameObject firstSelected)
    {
        if (eventSystem == null || uiModule == null || player == null || playerRoot == null)
            return;

        eventSystem.playerRoot = playerRoot;
        eventSystem.firstSelectedGameObject = firstSelected;
        eventSystem.SetSelectedGameObject(firstSelected);

        uiModule.actionsAsset = player.actions;

        uiModule.move = InputActionReference.Create(player.actions["Navigate"]);
        uiModule.submit = InputActionReference.Create(player.actions["Submit"]);
        uiModule.cancel = InputActionReference.Create(player.actions["Cancel"]);
        uiModule.point = InputActionReference.Create(player.actions["Point"]);
        uiModule.leftClick = InputActionReference.Create(player.actions["Click"]);
    }
}
