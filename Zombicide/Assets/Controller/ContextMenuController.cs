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
    private List<GameObject> currentMenus=new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OpenMenu(Vector3 position, List<string> options, System.Action<string> onOptionSelected)
    {
        //// Ha már van egy nyitott menü, elõször azt töröljük
        //if (currentMenus.Count > 0)
        //{
        //    var menu = currentMenus[0];
        //    currentMenus.RemoveAt(0);
        //    Destroy(menu);
        //}

        GameController.Instance.EnableBoardInteraction(false);
        // Menü létrehozása
        GameObject currentMenu = Instantiate(contextMenuPrefab, gameUI.transform);
        currentMenu.transform.localPosition = position;
        currentMenus.Add(currentMenu);

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
        if (currentMenus.Count > 0)
        {
            var menu = currentMenus[0];
            currentMenus.RemoveAt(0);
            Destroy(menu);
            GameController.Instance.EnableBoardInteraction(currentMenus.Count==0);
        }
    }
}

