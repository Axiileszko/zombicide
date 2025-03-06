using UnityEngine;

public class HoverClickHandlerForItem : MonoBehaviour
{
    private float hoverHeight = 0.5f;
    private Vector3 originalPosition;
    void Start()
    {
        originalPosition = transform.position;
    }

    void OnMouseEnter()
    {
        transform.position = originalPosition + Vector3.up * hoverHeight;
    }

    void OnMouseExit()
    {
        transform.position = originalPosition;
    }

    void OnMouseDown()
    {
        Debug.Log("Objektumra kattintottál!");
    }
}
