using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

namespace View
{
    public class MenuView:MonoBehaviour
    {
        #region Fields
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
        public static MenuView Instance { get; private set; }
        public int SelectedPlayerCount { get { return playerCount.value + 2; } }
        public int MapDropDownValue { get { return mapDropdown.value; } }
        public int CharacterDropDownValue { get { return characterDropdown.value; } }
        #endregion
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
        }
        public void ShowJoinGame()
        {
            mainMenuCanvas.SetActive(false);
            hostGameCanvas.SetActive(false);
            lobbyCanvas.SetActive(false);
            joinGameCanvas.SetActive(true);

            characterDropdownForClient.gameObject.SetActive(false);
            characterLabel.gameObject.SetActive(false);
            characterImageForClient.gameObject.SetActive(false);
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
        public void PopulateMapDropdown(List<MapData> maps)
        {
            mapDropdown.ClearOptions();
            List<string> mapNames = new List<string>();

            foreach (var map in maps)
            {
                mapNames.Add(map.name);
            }
            mapNames.Sort();

            mapDropdown.AddOptions(mapNames);
            mapDropdown.onValueChanged.AddListener(delegate { MenuController.Instance.UpdateMapDetails(mapDropdown.value); });

            MenuController.Instance.UpdateMapDetails(0);
        }
        public void PopulateCharacterDropdown(List<CharacterData> characters)
        {
            characterDropdown.ClearOptions();
            List<string> characterNames = new List<string>();

            foreach (var character in characters)
            {
                characterNames.Add(character.name);
            }
            characterNames.Sort();

            characterDropdown.AddOptions(characterNames);
            characterDropdown.onValueChanged.AddListener(delegate { MenuController.Instance.UpdateCharacterDetails(characterDropdown.value); });

            MenuController.Instance.UpdateCharacterDetails(0);
        }
        public void UpdateMapDetailsOnUI(MapData map)
        {
            mapImage.sprite = Resources.Load<Sprite>("Maps/" + map.image);
            mapObjectives.text = map.objectives;
            mapDifficulty.text = map.difficulty;
            mapRules.text = map.rules;
        }
        public void UpdateCharacterDropdownOnUI(string[] characters)
        {
            characterLabel.gameObject.SetActive(true);
            characterImageForClient.gameObject.SetActive(true);
            joinButton.enabled = true;
            characterDropdownForClient.ClearOptions();
            List<string> characterNames = new List<string>(characters);
            characterNames.Sort();
            characterDropdownForClient.gameObject.SetActive(true);
            characterDropdownForClient.AddOptions(characterNames);
            characterDropdownForClient.onValueChanged.AddListener(delegate { MenuController.Instance.UpdateCharacterDetails(characterDropdownForClient.value); });

            MenuController.Instance.UpdateCharacterDetails(0);
        }
        public void UpdateCharacterDetailsOnUI(CharacterData character)
        {
            if (hostGameCanvas.activeInHierarchy)
                characterImage.sprite = Resources.Load<Sprite>("Characters/" + character.image);
            else
                characterImageForClient.sprite = Resources.Load<Sprite>("Characters/" + character.image);
        }
        public void UpdateLobbyDisplayOnUI(ulong clientId, string name, GameObject entry)
        {
            entry.GetComponentInChildren<TMP_Text>().text = $"Player {clientId}: {name}";
        }
        public GameObject CreateLobbyEntry(ulong clientId, string name)
        {
            GameObject playerEntry = Instantiate(lobbyPlayerPrefab, lobbyPlayerListContainer.transform);
            TMP_Text playerText = playerEntry.GetComponent<TMP_Text>();
            playerText.text = $"Player {clientId}: {name}";
            playerText.enabled = true;
            return playerEntry;
        }
        public string UpdateJoin()
        {
            characterDropdownForClient.gameObject.SetActive(false);
            characterLabel.gameObject.SetActive(false);
            characterImageForClient.gameObject.SetActive(false);
            joinButton.enabled = false;
            string selectedCharacter = characterDropdownForClient.options[characterDropdownForClient.value].text;
            return selectedCharacter;
        }
        
    }
}
