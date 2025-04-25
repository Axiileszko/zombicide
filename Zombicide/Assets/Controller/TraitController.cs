using System.Collections.Generic;
using System.Drawing;
using TMPro;
using View;
using UnityEngine;
using UnityEngine.UI;

public class TraitController : MonoBehaviour
{
    public static TraitController Instance;

    [SerializeField] private GameObject traitPrefab;
    [SerializeField] private GameObject traitMenuPrefab;
    [SerializeField] private GameObject gameUI;

    private GameObject currentMenu;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public void OpenMenu(int level, List<string> options, System.Action<int,int> onOptionSelected)
    {
        InGameView.Instance.EnableBoardInteraction(false);
        currentMenu = Instantiate(traitMenuPrefab, gameUI.transform);

        VerticalLayoutGroup layout = null;
        foreach (Transform child in currentMenu.transform)
        {
            layout = child.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
                break;
        }
        foreach (var option in options)
        {
            GameObject button = Instantiate(traitPrefab, layout.transform);
            switch (level)
            {
                case 1:
                    button.GetComponentInChildren<TMP_Text>().color = UnityEngine.Color.yellow;
                    SpriteState spriteState = button.GetComponent<Button>().spriteState;
                    spriteState.highlightedSprite= Resources.Load<Sprite>("Menu/yellow_hover");
                    button.GetComponent<Button>().spriteState= spriteState;
                    break;
                case 2:
                    button.GetComponentInChildren<TMP_Text>().color = new UnityEngine.Color(1f, 0.52f, 0.07f, 1f);
                    spriteState = button.GetComponent<Button>().spriteState;
                    spriteState.highlightedSprite = Resources.Load<Sprite>("Menu/orange_hover");
                    button.GetComponent<Button>().spriteState = spriteState;
                    break;
                case 3:
                    button.GetComponentInChildren<TMP_Text>().color = UnityEngine.Color.red;
                    spriteState = button.GetComponent<Button>().spriteState;
                    spriteState.highlightedSprite = Resources.Load<Sprite>("Menu/red_hover");
                    button.GetComponent<Button>().spriteState = spriteState;
                    break;
            }
            button.GetComponentInChildren<TMP_Text>().text = option;
            button.GetComponent<Button>().onClick.AddListener(() => {
                onOptionSelected(level, options.IndexOf(option));
                CloseMenu();
            });
        }
    }
    public void CloseMenu()
    {
        if (currentMenu != null)
        {
            Destroy(currentMenu);
            currentMenu = null;
        }
    }
}
