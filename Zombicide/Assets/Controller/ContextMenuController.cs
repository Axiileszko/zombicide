using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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

    public void OpenMenu(Vector2 position, List<string> options, System.Action<string> onOptionSelected)
    {
        GameController.Instance.EnableBoardInteraction(false);

        GameObject currentMenu = Instantiate(contextMenuPrefab, gameUI.transform);
        currentMenu.transform.localPosition = position;

        float scaleFactor = 3.2f;
        float menuHeight = (float)(options.Count*3.7)+2f;
        float menuWidth = 7.5f;
        float menuTop = position.y;
        float menuLeft = position.x;
        float screenHeight = Screen.height;
        float screenWidth = Screen.width;

        float differenceH = (225 - menuHeight*scaleFactor) - (Math.Abs(menuTop) + menuHeight*scaleFactor);
        float differenceW = (400 - menuWidth*scaleFactor) - (Math.Abs(menuLeft) + menuWidth*scaleFactor);
        if (differenceH < 0 && menuTop<0)
        {
            position.y +=Math.Abs(differenceH)+ options.Count + 2f;
            currentMenu.transform.localPosition = position;
        }
        if (menuLeft>280)
        {
            position.x -= Math.Abs(differenceW) + 7.5f*8;
            currentMenu.transform.localPosition = position;
        }

        currentMenus.Add(currentMenu);

        foreach (var option in options)
        {
            GameObject button = Instantiate(contextMenuButtonPrefab, currentMenu.transform);
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

