using System.Collections.Generic;
using UnityEngine;

public class CameraDrag : MonoBehaviour
{
    private Vector3 dragOrigin;
    private float dragSpeed = 10f;
    private float timer = 0f;
    private Vector2 panLimitX = new Vector2(-20f, 20f);
    private Vector2 panLimitZ = new Vector2(-40f, 0f);
    private Vector3 resetCameraPosition;
    public static HoverClickHandlerForPanel PanelHoverScript;
    [SerializeField] private List<GameObject> colliders = new List<GameObject>();
    private List<HoverClickHandlerForItem> itemHoverScripts = new List<HoverClickHandlerForItem>();
    public static bool IsDragging { get; private set; } = false;
    void Start()
    {
        resetCameraPosition = Camera.main.transform.position;
        foreach (var item in colliders)
        {
            itemHoverScripts.Add(item.GetComponent<HoverClickHandlerForItem>());
        }
    }
    void Update()
    {
        HandleMouseDrag();
        HandleResetCamera();
    }
    void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = Input.mousePosition;
            IsDragging = true;
        }
        if (Input.GetMouseButton(1) && IsDragging && !HoverClickHandlerForItem.IsHovering && !HoverClickHandlerForPanel.IsHovering)
        {
            ToggleHoverScripts(false);
            Vector3 difference = dragOrigin - Input.mousePosition;
            dragOrigin = Input.mousePosition;

            Vector3 move = new Vector3(difference.x * dragSpeed * Time.deltaTime, 0, difference.y * dragSpeed * Time.deltaTime);
            MoveCamera(move);
        }
        if (Input.GetMouseButtonUp(1))
        {
            IsDragging = false;
            timer += Time.deltaTime;
            if (timer > 10)
            {
                timer = 0f;
                ToggleHoverScripts(true);
            }
        }
    }
    void HandleResetCamera()
    {
        if ((Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.Space)) && !HoverClickHandlerForItem.IsHovering && !HoverClickHandlerForPanel.IsHovering)
        {
            Camera.main.transform.position = resetCameraPosition;
        }
    }
    void MoveCamera(Vector3 move)
    {
        Vector3 newPosition = Camera.main.transform.position + move;
        newPosition.x = Mathf.Clamp(newPosition.x, panLimitX.x, panLimitX.y);
        newPosition.z = Mathf.Clamp(newPosition.z, panLimitZ.x, panLimitZ.y);
        Camera.main.transform.position = newPosition;
    }
    private void ToggleHoverScripts(bool state)
    {
        foreach (var script in itemHoverScripts)
        {
            script.enabled = state; // Engedélyezzük vagy tiltjuk a hover scripteket
        }
        if (PanelHoverScript != null)
            PanelHoverScript.enabled = state;
    }
}
