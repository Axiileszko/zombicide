using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class HoverClickHandlerForItem : MonoBehaviour
{
    [SerializeField] private GameObject item;
    private Vector3 originalPositionItem;
    void Start()
    {
        originalPositionItem = item.transform.position;
    }
    void OnMouseEnter()
    {
        item.transform.localPosition = new Vector3(item.transform.localPosition.x, item.transform.localPosition.y + 60f, item.transform.localPosition.z);
    }
    void OnMouseExit()
    {
        item.transform.position = originalPositionItem;
    }

    void OnMouseDown()
    {
        Debug.Log($"{gameObject.name} itemre kattintottál!");
    }
}
