using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    void Update()
    {
        float ScrollWheelChange = Input.GetAxis("Mouse ScrollWheel");
        if (ScrollWheelChange != 0)
        {                                            
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
            Camera.main.transform.position = new Vector3(CamX + X, CamY + Y, CamZ + Z);
        }
    }
}
