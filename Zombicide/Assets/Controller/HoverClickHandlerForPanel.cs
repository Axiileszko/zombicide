using UnityEngine;

public class HoverClickHandlerForPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    private Vector3 originalPositionPanel;
    public static bool IsHovering = false;
    void Start()
    {
        originalPositionPanel = panel.transform.position;
    }

    void OnMouseEnter()
    {
        originalPositionPanel = panel.transform.position;
        IsHovering = true;
        panel.transform.localPosition = new Vector3(panel.transform.localPosition.x, panel.transform.localPosition.y+120f, panel.transform.localPosition.z);
    }

    void OnMouseExit()
    {
        IsHovering=false;
        panel.transform.position = originalPositionPanel;
    }

}
