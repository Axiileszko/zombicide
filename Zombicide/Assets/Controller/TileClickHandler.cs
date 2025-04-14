using Network;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TileClickHandler : MonoBehaviour
{
    private void OnMouseDown()
    {
        GameObject canvas = GameObject.FindGameObjectWithTag("GameUI");
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 anchoredPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            Camera.main,
            out anchoredPosition
        );
        List<string> availableActions = GameController.Instance.GetAvailableActionsOnTile(gameObject.name);
        availableActions.Add("Skip");
        availableActions.Add("Cancel");
        ContextMenuController.Instance.OpenMenu(anchoredPosition, availableActions, OnOptionSelected);
    }
    private void OnOptionSelected(string option)
    {
        switch (option)
        {
            case "Cancel": ContextMenuController.Instance.CloseMenu(); return;
            case "Rearrange Items": GameController.Instance.OpenInventory(null,false); return;
            case "Open Door": GameController.Instance.EnableDoors(true); GameController.Instance.EnableBoardInteraction(false); return;
            case "Attack":
                GameObject canvas = GameObject.FindGameObjectWithTag("GameUI");
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                Vector2 anchoredPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    Input.mousePosition,
                    Camera.main,
                    out anchoredPosition
                );
                ContextMenuController.Instance.OpenMenu(anchoredPosition, GameController.Instance.GetAvailableAttacks(gameObject.name.Substring(8)), OnAttackOptionSelected); return;
            default:
                break;
        }

        ulong localPlayerId = NetworkManager.Singleton.LocalClientId;
        NetworkManagerController.Instance.RequestActionServerRpc(localPlayerId, option, gameObject.name);
    }
    private void OnAttackOptionSelected(string option)
    {
        List<string> wOptions = new List<string>();
        GameController.Instance.AttackFlag = option;
        if(option == "Range")
            wOptions=GameController.Instance.GetAvailableWeapons(false, gameObject.name.Substring(8));
        else
            wOptions = GameController.Instance.GetAvailableWeapons(true, gameObject.name.Substring(8));

        GameObject canvas = GameObject.FindGameObjectWithTag("GameUI");
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 anchoredPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            Camera.main,
            out anchoredPosition
        );
        ContextMenuController.Instance.OpenMenu(anchoredPosition, wOptions, OnWeaponOptionSelected); return;
    }

    private void OnWeaponOptionSelected(string option)
    {
        if (option == "Right Hand")
            GameController.Instance.StartAttack(gameObject.name,true);
        else
            GameController.Instance.StartAttack(gameObject.name,false);
    }
}
