using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private List<GameObject> colliders = new List<GameObject>();
    private List<HoverClickHandlerForItem> itemHoverScripts=new List<HoverClickHandlerForItem>();
    private float timer = 0f;
    private float minZoom = 20f;
    private float maxZoom = 40f;
    public static HoverClickHandlerForPanel PanelHoverScript;
    private void Start()
    {
        foreach (var item in colliders)
        {
            itemHoverScripts.Add(item.GetComponent<HoverClickHandlerForItem>());
        }
    }
    void Update()
    {
        if (CameraDrag.IsDragging) return;
        float ScrollWheelChange = Input.GetAxis("Mouse ScrollWheel");
        if (ScrollWheelChange != 0 && !HoverClickHandlerForItem.IsHovering && !HoverClickHandlerForPanel.IsHovering)
        {
            ToggleHoverScripts(false);
            float R = ScrollWheelChange * 15;                                   
            float PosX = Camera.main.transform.eulerAngles.x + 90;              
            float PosY = -1 * (Camera.main.transform.eulerAngles.y - 90);       
            PosX = PosX / 180 * Mathf.PI;
            PosY = PosY / 180 * Mathf.PI;            
            float X = R * Mathf.Sin(PosX) * Mathf.Cos(PosY);
            float Z = R * Mathf.Sin(PosX) * Mathf.Sin(PosY);
            float Y = R * Mathf.Cos(PosX);
            float CamX = Camera.main.transform.position.x;
            float CamY = Camera.main.transform.position.y;
            float CamZ = Camera.main.transform.position.z;
            //Camera.main.transform.position = new Vector3(CamX + X, CamY + Y, CamZ + Z);
            Vector3 newPosition = Camera.main.transform.position + new Vector3(X, Y, Z);

            // Ellenõrizzük, hogy az új pozíció a minimum és maximum között van-e
            float distance = Vector3.Distance(newPosition, Vector3.zero);
            if (distance >= minZoom && distance <= maxZoom)
            {
                Camera.main.transform.position = newPosition;
            }
        }
        else 
        {
            timer += Time.deltaTime;
            if (timer > 10)
            {
                timer = 0f;
                ToggleHoverScripts(true); 
            }
        }
    }
    private void ToggleHoverScripts(bool state)
    {
        foreach (var script in itemHoverScripts)
        {
            script.enabled = state; // Engedélyezzük vagy tiltjuk a hover scripteket
        }
        if(PanelHoverScript != null)
            PanelHoverScript.enabled = state;
    }
}
