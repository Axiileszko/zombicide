using Persistence;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Network;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MenuController : MonoBehaviour
{
    public GameObject mainMenuCanvas;
    public GameObject hostGameCanvas;
    public GameObject joinGameCanvas;

    public TMP_Dropdown mapDropdown;
    public Image mapImage;
    public TMP_Text mapObjectives;
    public TMP_Text mapDifficulty;
    public TMP_Text mapRules;
    public TMP_Dropdown characterDropdown;
    public Image characterImage;
    public TMP_Dropdown playerCount;
    private List<MapData> maps;
    private List<CharacterData> characters;


    public TMP_InputField codeInputField;
    public TMP_Text errorText;
    public TMP_Text characterLabel;
    public Button codeButton;
    public Image characterImageForClient;
    public TMP_Dropdown characterDropdownForClient;
    public Button joinButton;
    private List<string> availableCharacters = new List<string>(); // Ezt a host adja meg majd!
    private string roomCode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowHostGame()
    {
        mainMenuCanvas.SetActive(false);
        joinGameCanvas.SetActive(false);
        hostGameCanvas.SetActive(true);

        maps = FileManager.Instance.LoadMaps();
        characters = FileManager.Instance.LoadCharacters();

        PopulateMapDropdown();
        PopulateCharacterDropdown();
        PopulatePlayerCountDropdown();
    }

    public void ShowJoinGame()
    {
        mainMenuCanvas.SetActive(false);
        hostGameCanvas.SetActive(false);
        joinGameCanvas.SetActive(true);

        errorText.gameObject.SetActive(false);
        characterDropdown.gameObject.SetActive(false);
        characterLabel.gameObject.SetActive(false);
        joinButton.enabled = false;

        NetworkManagerController.Instance.ConnectClient();
    }
    public void ShowMainMenu()
    {
        mainMenuCanvas.SetActive(true);
        hostGameCanvas.SetActive(false);
        joinGameCanvas.SetActive(false);
    }
    public void Quit()
    {
        Application.Quit();
    }

    void PopulateMapDropdown()
    {
        mapDropdown.ClearOptions();
        List<string> mapNames = new List<string>();

        foreach (var map in maps)
        {
            mapNames.Add(map.name);
        }

        mapDropdown.AddOptions(mapNames);
        mapDropdown.onValueChanged.AddListener(delegate { UpdateMapDetails(mapDropdown.value); });

        UpdateMapDetails(0); // Alapértelmezett kiválasztott pálya
    }

    void UpdateMapDetails(int index)
    {
        mapImage.sprite = Resources.Load<Sprite>("Maps/" + maps[index].image);
        mapObjectives.text = maps[index].objectives;
        mapDifficulty.text = maps[index].difficulty;
        mapRules.text = maps[index].rules;
    }

    void PopulateCharacterDropdown()
    {
        characterDropdown.ClearOptions();
        List<string> characterNames = new List<string>();

        foreach (var character in characters)
        {
            characterNames.Add(character.name);
        }

        characterDropdown.AddOptions(characterNames);
        characterDropdown.onValueChanged.AddListener(delegate { UpdateCharacterDetails(characterDropdown.value); });

        UpdateCharacterDetails(0); // Alapértelmezett kiválasztott karakter
    }

    void UpdateCharacterDetails(int index)
    {
        if(hostGameCanvas.activeInHierarchy)
            characterImage.sprite = Resources.Load<Sprite>("Characters/" + characters[index].image);
        else
            characterImageForClient.sprite = Resources.Load<Sprite>("Characters/" + characters[index].image);
    }
    void PopulatePlayerCountDropdown()
    {
        playerCount.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 2; i <= 12; i++)
        {
            options.Add(i.ToString());
        }

        playerCount.AddOptions(options);
    }
    void PopulateCharacterDropdownForClient()
    {
        List<string> availableCharacters = NetworkManagerController.Instance.AvailableCharacters;

        characterDropdown.ClearOptions();
        characterDropdown.AddOptions(availableCharacters);
        characterDropdown.onValueChanged.AddListener(delegate { UpdateCharacterDetails(characterDropdown.value); });

        UpdateCharacterDetails(0);
    }

    public int GetSelectedPlayerCount()
    {
        return playerCount.value + 2;
    }
    public void OnOkButtonPressed()
    {
        int playerCount = GetSelectedPlayerCount();
        string selectedMap = maps[mapDropdown.value].name;
        string selectedCharacter = characters[characterDropdown.value].name;

        Debug.Log($"Host létrehozva! Pálya: {selectedMap}, Játékosok száma: {playerCount}, Karakter: {selectedCharacter}");
        List<string> avaliableCharacters=characters.Where(x => x.name != selectedCharacter).Select(x=>x.name).ToList();
        NetworkManagerController.Instance.StartHost(playerCount,avaliableCharacters);

        // Megjelenítjük a csatlakozási kódot az UI-n (ha lesz rá UI elem)
        Debug.Log("Csatlakozási kód: " + NetworkManagerController.Instance.GetSessionCode());
    }
    public void OnCodeButtonPressed()
    {
        if (!NetworkManagerController.Instance.CheckCode(codeInputField.text,out string error))
        {
            errorText.gameObject.SetActive(true);
            errorText.text = error;
            return;
        }
        errorText.gameObject.SetActive(false);
        characterDropdown.gameObject.SetActive(true);
        characterLabel.gameObject.SetActive(true);
        joinButton.enabled = true;

        PopulateCharacterDropdownForClient();
    }
    public void OnJoinButtonPressed()
    {
        string selectedCharacter = characterDropdown.options[characterDropdown.value].text;

        if (!NetworkManagerController.Instance.SelectCharacter(selectedCharacter))
        {
            errorText.gameObject.SetActive(true);
            errorText.text = "Ez a karakter már foglalt!";
            return;
        }

        // Ha sikeres, csatlakozunk a váróterembe
        NetworkManagerController.Instance.JoinGame(selectedCharacter);

        //elküldjük a váróterembe
    }
    public void OnCancelButtonPressed()
    {
        ShowMainMenu();
    }
}
