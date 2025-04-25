using Persistence;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Network;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Unity.Netcode;
using System.Collections;
using View;

public class MenuController : MonoBehaviour
{
    #region Fields
    private List<MapData> maps;
    private List<CharacterData> characters;
    private Dictionary<ulong, GameObject> lobbyUIEntries= new Dictionary<ulong, GameObject>();
    [SerializeField] GameObject networkManagerPrefab;
    #endregion
    #region Properties
    public static MenuController Instance { get; private set; }
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
    private void Start()
    {
        StartCoroutine(EnsureNetworkManagerInitialized());
    }
    private IEnumerator EnsureNetworkManagerInitialized()
    {
        yield return null;

        if (NetworkManager.Singleton == null && networkManagerPrefab != null)
        {
            Instantiate(networkManagerPrefab);
        }
    }
    #region Switching Between Canvases
    public void ShowHostGame()
    {
        MenuView.Instance.ShowHostGame();

        maps = FileManager.Instance.LoadMaps();
        characters = FileManager.Instance.LoadCharacters();

        MenuView.Instance.PopulateMapDropdown(maps);
        MenuView.Instance.PopulateCharacterDropdown(characters);
        MenuView.Instance.PopulatePlayerCountDropdown();
    }
    public void ShowJoinGame()
    {
        characters = FileManager.Instance.LoadCharacters();
        MenuView.Instance.ShowJoinGame();
    }
    #endregion
    public void UpdateMapDetails(int index)
    {
        MenuView.Instance.UpdateMapDetailsOnUI(maps[index]);
    }
    public void UpdateCharacterDropdown(string[] characters)
    {
        List<string> characterNames = new List<string>(characters);
        characterNames.Sort();
        this.characters=this.characters.Where(c => characterNames.Contains(c.name)).ToList();
        MenuView.Instance.UpdateCharacterDropdownOnUI(characters);
    }
    public void UpdateCharacterDetails(int index)
    {
        characters=characters.OrderBy(c => c.name).ToList();
        MenuView.Instance.UpdateCharacterDetailsOnUI(characters[index]);
    }
    public void UpdateLobbyDisplay(Dictionary<ulong, string> selectedCharacters)
    {
        List<ulong> extistingPlayers = new List<ulong>(lobbyUIEntries.Keys);

        foreach (var entry in selectedCharacters)
        {
            if (lobbyUIEntries.ContainsKey(entry.Key))
            {
                MenuView.Instance.UpdateLobbyDisplayOnUI(entry.Key, entry.Value, lobbyUIEntries[entry.Key]);
                extistingPlayers.Remove(entry.Key);
            }
            else
            {
                GameObject playerEntry= MenuView.Instance.CreateLobbyEntry(entry.Key, entry.Value);
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
        int playerCount = MenuView.Instance.SelectedPlayerCount;
        int selectedMap = maps[MenuView.Instance.MapDropDownValue].id;
        string selectedCharacter = characters[MenuView.Instance.CharacterDropDownValue].name;

        List<string> availableCharacters = characters.Select(x => x.name).ToList();
        availableCharacters.Remove(selectedCharacter);

        NetworkManagerController.Instance.StartHost(playerCount, availableCharacters, selectedCharacter, selectedMap);

        MenuView.Instance.ShowLobbyMenu();
    }
    public void OnJoinButtonPressed()
    {
        string selectedCharacter = MenuView.Instance.UpdateJoin();
        NetworkManagerController.Instance.SubscribeClient();
        NetworkManagerController.Instance.SelectCharacterServerRpc(NetworkManager.Singleton.LocalClientId, selectedCharacter);
        MenuView.Instance.ShowLobbyMenu();
    }
    public void OnCancelLobbyButtonPressed()
    {
        NetworkManagerController.Instance.LobbyCancel();
    }
    public void OnCancelButtonPressed()
    {
        MenuView.Instance.ShowMainMenu();
    }
    public void OnCancelJoinButtonPressed()
    {
        NetworkManagerController.Instance.JoinCancel();
        MenuView.Instance.ShowMainMenu();
    }
    #endregion
    #endregion
}
