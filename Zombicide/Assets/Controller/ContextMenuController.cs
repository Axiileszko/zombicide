using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenuController : MonoBehaviour
{
    public static ContextMenuController Instance;

    [SerializeField] private GameObject contextMenuPrefab;
    [SerializeField] private GameObject contextMenuButtonPrefab;
    [SerializeField] private GameObject gameUI;
    private GameObject currentMenu;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OpenMenu(Vector3 position, List<string> options, System.Action<string> onOptionSelected)
    {
        // Ha már van egy nyitott menü, elõször azt töröljük
        if (currentMenu != null) Destroy(currentMenu);

        // Menü létrehozása
        currentMenu = Instantiate(contextMenuPrefab, gameUI.transform);
        Debug.Log("currentmenu isntantiate után: "+currentMenu.name);
        currentMenu.transform.localPosition = position;

        VerticalLayoutGroup layout = null;
        foreach (Transform child in currentMenu.transform)
        {
            layout=child.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
                break;
        }
        // Menü opciók dinamikus hozzáadása
        foreach (var option in options)
        {
            GameObject button = Instantiate(contextMenuButtonPrefab, layout.transform);
            button.GetComponentInChildren<TMP_Text>().text = option;
            button.GetComponent<Button>().onClick.AddListener(() => {
                onOptionSelected(option);
                CloseMenu();
            });
        }
    }

    public void CloseMenu()
    {
        if (currentMenu != null) Destroy(currentMenu);
    }
}

