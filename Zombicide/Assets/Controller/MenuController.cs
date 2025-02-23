using Persistence;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Network;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Unity.Netcode;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject hostGameCanvas;
    [SerializeField] private GameObject joinGameCanvas;
    [SerializeField] private GameObject lobbyCanvas;

    [SerializeField] private TMP_Dropdown mapDropdown;
    [SerializeField] private Image mapImage;
    [SerializeField] private TMP_Text mapObjectives;
    [SerializeField] private TMP_Text mapDifficulty;
    [SerializeField] private TMP_Text mapRules;
    [SerializeField] private TMP_Dropdown characterDropdown;
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Dropdown playerCount;
    private List<MapData> maps;
    private List<CharacterData> characters;


    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private TMP_Text characterLabel;
    [SerializeField] private Button codeButton;
    [SerializeField] private Image characterImageForClient;
    [SerializeField] private TMP_Dropdown characterDropdownForClient;
    [SerializeField] private Button joinButton;
    //private List<string> availableCharacters = new List<string>(); // A választható karakterek
    //private Dictionary<ulong, string> selectedCharacters = new Dictionary<ulong, string>(); // A kiválasztott karakterek
    //public List<string> AvailableCharacters { get { return availableCharacters; } }

    public static MenuController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ezt akkor használd, ha a scene váltás miatt veszhet el
        }
        else
        {
            Destroy(gameObject); // Ha több példány jön létre, töröljük a másodikat
        }
    }
    public void ShowHostGame()
    {
        mainMenuCanvas.SetActive(false);
        joinGameCanvas.SetActive(false);
        lobbyCanvas.SetActive(false);
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
        lobbyCanvas.SetActive(false);
        joinGameCanvas.SetActive(true);

        characterDropdown.gameObject.SetActive(false);
        characterLabel.gameObject.SetActive(false);
        joinButton.enabled = false;
    }
    public void ShowMainMenu()
    {
        mainMenuCanvas.SetActive(true);
        hostGameCanvas.SetActive(false);
        lobbyCanvas.SetActive(false);
        joinGameCanvas.SetActive(false);
    }
    public void ShowLobbyMenu()
    {
        mainMenuCanvas.SetActive(false);
        hostGameCanvas.SetActive(false);
        lobbyCanvas.SetActive(true);
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
    public void UpdateCharacterDropdown(string[] characters)
    {
        characterLabel.gameObject.SetActive(true);
        joinButton.enabled = true;
        characterDropdownForClient.ClearOptions();
        List<string> characterNames = new List<string>(characters);
        characterDropdownForClient.gameObject.SetActive(true);
        characterDropdownForClient.AddOptions(characterNames);
        characterDropdownForClient.onValueChanged.AddListener(delegate { UpdateCharacterDetails(characterDropdownForClient.value); });

        UpdateCharacterDetails(0);
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
        //probléma: ez a characters lista üres a kliensnek
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
    public int GetSelectedPlayerCount()
    {
        return playerCount.value + 2;
    }
    public void OnOkButtonPressed()
    {
        int playerCount = GetSelectedPlayerCount();
        string selectedMap = maps[mapDropdown.value].name;
        string selectedCharacter = characters[characterDropdown.value].name;

        List<string> availableCharacters = characters.Select(x => x.name).ToList();
        availableCharacters.Remove(selectedCharacter);
        // A host karakterét eltároljuk
        //selectedCharacters[NetworkManager.Singleton.LocalClientId] = selectedCharacter;

        NetworkManagerController.Instance.StartHost(playerCount, availableCharacters);

        //Debug.Log($"Host létrehozva! Pálya: {selectedMap}, Játékosok száma: {playerCount}, Karakter: {selectedCharacter}");
        ShowLobbyMenu();
    }
    public void OnJoinButtonPressed()
    {
        string selectedCharacter = characterDropdown.options[characterDropdown.value].text;
        NetworkManagerController.Instance.SelectCharacterServerRpc(NetworkManager.Singleton.LocalClientId, selectedCharacter);
    }
    public void OnCancelButtonPressed()
    {
        ShowMainMenu();
    }
}
