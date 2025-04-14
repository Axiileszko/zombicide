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
    #region Fields
    #region Data
    private List<MapData> maps;
    private List<CharacterData> characters;
    private Dictionary<ulong, GameObject> lobbyUIEntries= new Dictionary<ulong, GameObject>();
    #endregion
    #region Canvases
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject hostGameCanvas;
    [SerializeField] private GameObject joinGameCanvas;
    [SerializeField] private GameObject lobbyCanvas;
    #endregion
    #region Host
    [SerializeField] private TMP_Dropdown mapDropdown;
    [SerializeField] private Image mapImage;
    [SerializeField] private TMP_Text mapObjectives;
    [SerializeField] private TMP_Text mapDifficulty;
    [SerializeField] private TMP_Text mapRules;
    [SerializeField] private TMP_Dropdown characterDropdown;
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Dropdown playerCount;
    #endregion
    #region Join
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private TMP_Text characterLabel;
    [SerializeField] private Button codeButton;
    [SerializeField] private Image characterImageForClient;
    [SerializeField] private TMP_Dropdown characterDropdownForClient;
    [SerializeField] private Button joinButton;
    #endregion
    #region Lobby
    [SerializeField] private GameObject lobbyPlayerPrefab;
    [SerializeField] private VerticalLayoutGroup lobbyPlayerListContainer;
    #endregion
    #endregion
    #region Properties
    public static MenuController Instance { get; private set; }
    public int SelectedPlayerCount { get { return playerCount.value + 2; } }
    #endregion
    #region Methods
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #region Switching Between Canvases
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

        characters = FileManager.Instance.LoadCharacters();
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
    #endregion
    public void PopulateMapDropdown()
    {
        mapDropdown.ClearOptions();
        List<string> mapNames = new List<string>();

        foreach (var map in maps)
        {
            mapNames.Add(map.name);
        }
        mapNames.Sort();

        mapDropdown.AddOptions(mapNames);
        mapDropdown.onValueChanged.AddListener(delegate { UpdateMapDetails(mapDropdown.value); });

        UpdateMapDetails(0); // Alapértelmezett kiválasztott pálya
    }
    public void PopulatePlayerCountDropdown()
    {
        playerCount.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 2; i <= 6; i++)
        {
            options.Add(i.ToString());
        }

        playerCount.AddOptions(options);
    }
    public void PopulateCharacterDropdown()
    {
        characterDropdown.ClearOptions();
        List<string> characterNames = new List<string>();

        foreach (var character in characters)
        {
            characterNames.Add(character.name);
        }
        characterNames.Sort();

        characterDropdown.AddOptions(characterNames);
        characterDropdown.onValueChanged.AddListener(delegate { UpdateCharacterDetails(characterDropdown.value); });

        UpdateCharacterDetails(0); // Alapértelmezett kiválasztott karakter
    }
    public void UpdateMapDetails(int index)
    {
        mapImage.sprite = Resources.Load<Sprite>("Maps/" + maps[index].image);
        mapObjectives.text = maps[index].objectives;
        mapDifficulty.text = maps[index].difficulty;
        mapRules.text = maps[index].rules;
    }
    public void UpdateCharacterDropdown(string[] characters)
    {
        characterLabel.gameObject.SetActive(true);
        characterImageForClient.gameObject.SetActive(true);
        joinButton.enabled = true;
        characterDropdownForClient.ClearOptions();
        List<string> characterNames = new List<string>(characters);
        characterNames.Sort();
        this.characters=this.characters.Where(c => characterNames.Contains(c.name)).ToList();
        characterDropdownForClient.gameObject.SetActive(true);
        characterDropdownForClient.AddOptions(characterNames);
        characterDropdownForClient.onValueChanged.AddListener(delegate { UpdateCharacterDetails(characterDropdownForClient.value); });

        UpdateCharacterDetails(0);
    }
    public void UpdateCharacterDetails(int index)
    {
        characters=characters.OrderBy(c => c.name).ToList();
        if(hostGameCanvas.activeInHierarchy)
            characterImage.sprite = Resources.Load<Sprite>("Characters/" + characters[index].image);
        else
            characterImageForClient.sprite = Resources.Load<Sprite>("Characters/" + characters[index].image);
    }
    public void UpdateLobbyDisplay(Dictionary<ulong, string> selectedCharacters)
    {
        List<ulong> extistingPlayers = new List<ulong>(lobbyUIEntries.Keys);

        foreach (var entry in selectedCharacters)
        {
            if (lobbyUIEntries.ContainsKey(entry.Key))
            {
                lobbyUIEntries[entry.Key].GetComponentInChildren<TMP_Text>().text = $"Player {entry.Key}: {entry.Value}";
                extistingPlayers.Remove(entry.Key);
            }
            else
            {
                GameObject playerEntry = Instantiate(lobbyPlayerPrefab, lobbyPlayerListContainer.transform);
                TMP_Text playerText = playerEntry.GetComponent<TMP_Text>();
                playerText.text = $"Player {entry.Key}: {entry.Value}";
                playerText.enabled = true;
                lobbyUIEntries.Add(entry.Key, playerEntry);
            }
        }
        foreach(var clientID in extistingPlayers)
        {
            Destroy(lobbyUIEntries[clientID]);
            lobbyUIEntries.Remove(clientID);
        }
    }
    #region Event handlers
    public void Quit()
    {
        Application.Quit();
    }
    public void OnOkButtonPressed()
    {
        int playerCount = SelectedPlayerCount;
        int selectedMap = maps[mapDropdown.value].id;
        string selectedCharacter = characters[characterDropdown.value].name;

        List<string> availableCharacters = characters.Select(x => x.name).ToList();
        availableCharacters.Remove(selectedCharacter);

        NetworkManagerController.Instance.StartHost(playerCount, availableCharacters, selectedCharacter, selectedMap);

        ShowLobbyMenu();
    }
    public void OnJoinButtonPressed()
    {
        characterDropdownForClient.gameObject.SetActive(false);
        characterLabel.gameObject.SetActive(false);
        characterImageForClient.gameObject.SetActive(false);
        joinButton.enabled = false;
        string selectedCharacter = characterDropdownForClient.options[characterDropdownForClient.value].text;
        NetworkManagerController.Instance.SubscribeClient();
        NetworkManagerController.Instance.SelectCharacterServerRpc(NetworkManager.Singleton.LocalClientId, selectedCharacter);
        ShowLobbyMenu();
    }
    public void OnCancelLobbyButtonPressed()
    {
        NetworkManagerController.Instance.LobbyCancel();
    }
    public void OnCancelButtonPressed()
    {
        ShowMainMenu();
    }
    #endregion
    #endregion
}
