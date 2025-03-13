using Network;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TileClickHandler : MonoBehaviour
{
    private void OnMouseDown()
    {
        Debug.Log($"Tile clicked: {gameObject.name}");

        GameObject canvas = GameObject.FindGameObjectWithTag("GameUI");
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 anchoredPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            Camera.main,
            out anchoredPosition
        );
        anchoredPosition.x += 107;
        anchoredPosition.y -= 140;
        List<string> availableActions = GameController.Instance.GetAvailableActionsOnTile(gameObject.name);
        if (availableActions.Count == 0) return;
        availableActions.Add("Skip");
        availableActions.Add("Cancel");
        Debug.Log("actions:"+string.Join(", ", availableActions));
        ContextMenuController.Instance.OpenMenu(anchoredPosition, availableActions, OnOptionSelected);
    }
    private void OnOptionSelected(string option)
    {
        Debug.Log($"J�t�kos v�lasztotta: {option}");

        // Ide j�n a konkr�t akci� v�grehajt�sa a j�t�kban
        switch (option)
        {
            case "Cancel": ContextMenuController.Instance.CloseMenu(); return;
            case "Rearrange Items": GameController.Instance.OpenInventory(null); return;
            case "Open Door": GameController.Instance.EnableDoors(true); return;
            default:
                break;
        }

        ulong localPlayerId = NetworkManager.Singleton.LocalClientId;
        NetworkManagerController.Instance.RequestActionServerRpc(localPlayerId, option, gameObject.name);
    }

}
