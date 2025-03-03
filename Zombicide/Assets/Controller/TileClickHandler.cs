using UnityEngine;

public class TileClickHandler : MonoBehaviour
{
    private void OnMouseDown()
    {
        Debug.Log($"Tile clicked: {gameObject.name}");
    }
}
