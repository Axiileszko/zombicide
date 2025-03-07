using UnityEngine;

public class HoverClickHandlerForPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    private Vector3 originalPositionPanel;
    void Start()
    {
        originalPositionPanel = panel.transform.position;
    }

    void OnMouseEnter()
    {
        panel.transform.localPosition = new Vector3(panel.transform.localPosition.x, panel.transform.localPosition.y+220f, panel.transform.localPosition.z);
    }

    void OnMouseExit()
    {
        panel.transform.position = originalPositionPanel;
    }

    void OnMouseDown()
    {
        Debug.Log($"panelre kattintottál!");
    }
}
