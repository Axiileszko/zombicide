using UnityEngine;

public class CameraDrag : MonoBehaviour
{
    private Vector3 dragOrigin;
    private float dragSpeed = 20f;
    private Vector2 panLimitX = new Vector2(-20f, 20f);
    private Vector2 panLimitZ = new Vector2(-40f, 0f);
    private Vector3 resetCameraPosition;
    private bool isDragging = false;

    void Start()
    {
        resetCameraPosition = Camera.main.transform.position;
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
            isDragging = true;
        }
        if (Input.GetMouseButton(1) && isDragging)
        {
            Vector3 difference = dragOrigin - Input.mousePosition;
            dragOrigin = Input.mousePosition;

            Vector3 move = new Vector3(difference.x * dragSpeed * Time.deltaTime, 0, difference.y * dragSpeed * Time.deltaTime);
            MoveCamera(move);
        }
        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }
    }
    void HandleResetCamera()
    {
        if (Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.Space))
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

}
