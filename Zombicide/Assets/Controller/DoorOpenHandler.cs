using Network;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DoorOpenHandler : MonoBehaviour
{
    private void OnMouseDown()
    {
        Debug.Log($"Door clicked: {gameObject.name}");

        GameObject canvas = GameObject.FindGameObjectWithTag("GameUI");
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 anchoredPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            Camera.main,
            out anchoredPosition
        );
        List<string> availableActions = GameController.Instance.GetAvailableDoorOpeners();
        if (availableActions.Count == 0) return;
        availableActions.Add("Cancel");
        ContextMenuController.Instance.OpenMenu(anchoredPosition, availableActions, OnOptionSelected);
    }

    private void OnOptionSelected(string option)
    {
        Debug.Log($"Játékos választotta: {option}");
        if(option=="Cancel")
            ContextMenuController.Instance.CloseMenu();
        else
            NetworkManagerController.Instance.RequestActionServerRpc(NetworkManager.Singleton.LocalClientId, "Open Door "+option, gameObject.name);
    }
}
