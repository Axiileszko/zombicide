using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TileClickHandler : MonoBehaviour
{
    private void OnMouseDown()
    {
        Debug.Log($"Tile clicked: {gameObject.name}");
        //Vector3 menuPosition = Camera.main.WorldToScreenPoint(transform.position);
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
        Debug.Log("actions:"+string.Join(", ", availableActions));
        ContextMenuController.Instance.OpenMenu(anchoredPosition, availableActions, OnOptionSelected);
    }
    private void OnOptionSelected(string option)
    {
        Debug.Log($"Játékos választotta: {option}");
        // Ide jön a konkrét akció végrehajtása a játékban
    }
}
