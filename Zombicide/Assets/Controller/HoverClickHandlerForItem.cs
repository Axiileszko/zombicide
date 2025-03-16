using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class HoverClickHandlerForItem : MonoBehaviour
{
    [SerializeField] private GameObject item;
    private Vector3 originalPositionItem;
    public static bool IsHovering = false;
    void Start()
    {
        originalPositionItem = item.transform.position;
    }

    void OnMouseEnter()
    {
        originalPositionItem = item.transform.position;
        IsHovering = true;
        item.transform.localPosition = new Vector3(item.transform.localPosition.x, item.transform.localPosition.y + 60f, item.transform.localPosition.z);
    }
    void OnMouseExit()
    {
        item.transform.position = originalPositionItem;
        IsHovering = false;
    }
}
