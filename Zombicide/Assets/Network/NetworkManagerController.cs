using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public enum MessageType
    {
        PlayerOrder,
        TurnStart,
        TurnEnd,
        GenericWeapon,
        FinishedRound,
        PimpWeapon,
        ItemsChanged,
        Search,
        Attack,
        ZombieSpawn,
        ZombieSpawnInBuilding,
        TraitUpgrade,
        GameEnded,
        SurvivorDied,
        PlayerLeft,
        PlayerDisconnected
    }
    public class NetworkManagerController:NetworkBehaviour
    {
        #region Fields
        private List<string> availableCharacters = new List<string>();
        private Dictionary<ulong, string> selectedCharacters = new Dictionary<ulong, string>();
        private int expectedPlayerCount;
        private bool hasSentPlayerSelections = false;
        #endregion
        #region Properties
        public int SelectedMapID { get; private set; }
        public static NetworkManagerController Instance { get; private set; }
        #endregion
        #region Methods
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        /// <summary>
        /// Host started a play session.
        /// </summary>
        /// <param name="playerCount">how many players are gonna play</param>
        /// <param name="characters">available characters</param>
        /// <param name="selectedCharacter">selected character by the host</param>
        /// <param name="selectedMap">selected map by the host</param>
        public void StartHost(int playerCount, List<string> characters, string selectedCharacter, int selectedMap)
        {
            SubscribeClient();
            hasSentPlayerSelections = false;
            SelectedMapID = selectedMap;
            expectedPlayerCount = playerCount;
            selectedCharacters.Clear();
            selectedCharacters[NetworkManager.Singleton.LocalClientId] = selectedCharacter;
            availableCharacters = new List<string>(characters);
            MenuController.Instance.UpdateLobbyDisplay(selectedCharacters);
        }
        /// <summary>
        /// Handler of a player leaving the lobby.
        /// </summary>
        public void LobbyCancel()
        {
            ulong localClientId = NetworkManager.Singleton.LocalClientId;

            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("A host megszakította a lobbit. Mindenki lecsatlakozik...");

                NetworkManager.Singleton.Shutdown();
            }
            else
            {
                Debug.Log($"Kliens ({localClientId}) kilépett a lobbiból.");

                RequestLeaveLobbyServerRpc(localClientId);

                NetworkManager.Singleton.Shutdown();
            }

            MenuController.Instance.ShowMainMenu();
        }
        /// <summary>
        /// Subcribing to the clients' disconnected event.
        /// </summary>
        public void SubscribeClient()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        #region ServerRCP Methods
        /// <summary>
        /// Client sends request to host to leave the lobby.
        /// </summary>
        /// <param name="clientId">ID of the client who wants to leave</param>
        [ServerRpc(RequireOwnership = false)]
        public void RequestLeaveLobbyServerRpc(ulong clientId)
        {
            if (selectedCharacters.ContainsKey(clientId))
            {
                string character = selectedCharacters[clientId];
                availableCharacters.Add(character);

                selectedCharacters.Remove(clientId);

                Debug.Log($"Kliens ({clientId}) kilépett. Karakter ({character}) visszaadva.");
            }

            ulong[] clientIds = selectedCharacters.Keys.ToArray();
            string characterNamesCSV = string.Join(",", selectedCharacters.Values);

            UpdateLobbyDisplayClientRpc(clientIds, characterNamesCSV);
        }
        /// <summary>
        /// The cilent asks for the character list from the host.
        /// </summary>
        /// <param name="clientId">ID of the client</param>
        [ServerRpc(RequireOwnership = false)]
        public void RequestCharacterListServerRpc(ulong clientId)
        {
            string characterListString=string.Join(",", availableCharacters);

            SendCharacterListClientRpc(characterListString, clientId);
        }
        /// <summary>
        /// A client chose a character and joined the game.
        /// </summary>
        /// <param name="clientId">ID of the client</param>
        /// <param name="character">chosen character</param>
        [ServerRpc(RequireOwnership = false)]
        public void SelectCharacterServerRpc(ulong clientId, string character)
        {
            if (!availableCharacters.Contains(character))
            {
                return;
            }

            availableCharacters.Remove(character);
            selectedCharacters[clientId] = character;

            if (selectedCharacters.Count == expectedPlayerCount)
            {
                if(NetworkManager.Singleton.IsHost)
                    UpdateSelectedMapIDClientRpc(SelectedMapID.ToString());
                StartGameClientRpc();
            }

            string characterListString = string.Join(",", availableCharacters);
            UpdateAllClientsCharacterDropdownClientRpc(characterListString);

            string selectedCharactersString = string.Join(";", selectedCharacters.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
            UpdateLobbyClientRpc(selectedCharactersString);
        }
        /// <summary>
        /// Sending clients the list of players and their selected characters.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SendPlayerSelectionsServerRpc()
        {
            if (hasSentPlayerSelections)
            {
                return;
            }

            hasSentPlayerSelections = true;
            if (!NetworkManager.Singleton.IsHost) return;

            List<ulong> clientIDs = new List<ulong>(selectedCharacters.Keys);
            List<string> characterNames = new List<string>(selectedCharacters.Values);

            string clientIDsSerialized = string.Join(",", clientIDs);
            string characterNamesSerialized = string.Join(",", characterNames);

            ReceivePlayerSelectionsClientRpc(clientIDsSerialized, characterNamesSerialized);
        }
        /// <summary>
        /// Sending the in-game message to all clients. 
        /// </summary>
        /// <param name="type">type of message</param>
        /// <param name="data">string containing the data</param>
        [ServerRpc(RequireOwnership = false)]
        public void SendMessageToClientsServerRpc(MessageType type, string data)
        {
            ReceiveMessageClientRpc(type, data);
        }
        /// <summary>
        /// Host sends the in-game action to all clients.
        /// </summary>
        /// <param name="playerId">ID of the player doing the action</param>
        /// <param name="actionName">name of the action</param>
        /// <param name="objectName">name of the object used in the action</param>
        [ServerRpc(RequireOwnership = false)]
        public void RequestActionServerRpc(ulong playerId, string actionName, string objectName)
        {
            Debug.Log($"[SERVER] Player {playerId} requested action: {actionName} on tile {objectName}");
            ApplyActionClientRpc(playerId, actionName, objectName);
        }
        #endregion
        #region ClientRPC Methods
        /// <summary>
        /// The host sends the list of available characters to the clients.
        /// </summary>
        /// <param name="characterList">List of characters</param>
        /// <param name="targetClientId">ID of the client</param>
        [ClientRpc]
        private void SendCharacterListClientRpc(string characterList, ulong targetClientId)
        {
            if (NetworkManager.Singleton.LocalClientId == targetClientId)
            {
                string[] characters = characterList.Split(',');

                MenuController.Instance.UpdateCharacterDropdown(characters);
            }
        }
        /// <summary>
        /// The client updates the selected map ID.
        /// </summary>
        /// <param name="mapId">ID of the map</param>
        [ClientRpc]
        private void UpdateSelectedMapIDClientRpc(string mapId)
        {
            SelectedMapID = int.Parse(mapId);
        }
        /// <summary>
        /// Updating the list of available characters for all clients  .
        /// </summary>
        /// <param name="characterList">updated list of characters</param>
        [ClientRpc]
        private void UpdateAllClientsCharacterDropdownClientRpc(string characterList)
        {
            string[] characters = characterList.Split(',');
            MenuController.Instance.UpdateCharacterDropdown(characters);
        }
        /// <summary>
        /// Updating the connected players in the lobby when a new client joins.
        /// </summary>
        /// <param name="selectedCharactersString">string containing the connected players</param>
        [ClientRpc]
        private void UpdateLobbyClientRpc(string selectedCharactersString)
        {
            Dictionary<ulong, string> selectedCharacters = new Dictionary<ulong, string>();

            string[] pairs = selectedCharactersString.Split(';');
            foreach (string pair in pairs)
            {
                string[] keyValue = pair.Split(':');
                if (keyValue.Length == 2 && ulong.TryParse(keyValue[0], out ulong clientId))
                {
                    selectedCharacters[clientId] = keyValue[1];
                }
            }

            MenuController.Instance.UpdateLobbyDisplay(selectedCharacters);
        }
        /// <summary>
        /// Updating the connected players in the lobby when a client leaves.
        /// </summary>
        /// <param name="clientIds">ID of the client leaving</param>
        /// <param name="characterNamesCSV">Remaining players</param>
        [ClientRpc]
        private void UpdateLobbyDisplayClientRpc(ulong[] clientIds, string characterNamesCSV)
        {
            Dictionary<ulong, string> updatedCharacters = new Dictionary<ulong, string>();

            string[] characterNames = characterNamesCSV.Split(',');

            for (int i = 0; i < clientIds.Length; i++)
            {
                updatedCharacters[clientIds[i]] = characterNames[i];
            }

            MenuController.Instance.UpdateLobbyDisplay(updatedCharacters);
        }
        /// <summary>
        /// Starting the game for every client.
        /// </summary>
        [ClientRpc]
        private void StartGameClientRpc()
        {
            Debug.Log("Minden játékos csatlakozott! Játék indítása...");

            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("InGameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
        /// <summary>
        /// Clients get the list of players and their selected characters.
        /// </summary>
        /// <param name="clientIDsSerialized">IDs of the clients</param>
        /// <param name="characterNamesSerialized">names of characters chosen by clients</param>
        [ClientRpc]
        private void ReceivePlayerSelectionsClientRpc(string clientIDsSerialized, string characterNamesSerialized)
        {
            // Stringek visszaalakítása tömbökké
            string[] clientIDsArray = clientIDsSerialized.Split(',');
            string[] characterNamesArray = characterNamesSerialized.Split(',');

            Dictionary<ulong, string> receivedSelections = new Dictionary<ulong, string>();

            for (int i = 0; i < clientIDsArray.Length; i++)
            {
                ulong clientID = ulong.Parse(clientIDsArray[i]);
                string characterName = characterNamesArray[i];
                receivedSelections[clientID] = characterName;
            }

            // Átadjuk a GameControllernek
            GameController.Instance.SetPlayerSelections(receivedSelections);
        }
        /// <summary>
        /// Clients recieve the in-game message from the host.
        /// </summary>
        /// <param name="type">type of message</param>
        /// <param name="data">string containing the data</param>
        [ClientRpc]
        public void ReceiveMessageClientRpc(MessageType type, string data)
        {
            switch (type)
            {
                case MessageType.PlayerOrder:
                    GameController.Instance.ReceivePlayerOrder(data);
                    break;
                case MessageType.TurnStart:
                    GameController.Instance.ReceiveTurnStart(ulong.Parse(data));
                    break;
                case MessageType.GenericWeapon:
                    GameController.Instance.ReceiveGenericWeapons(data);
                    break;
                case MessageType.FinishedRound:
                    GameController.Instance.PlayerFinishedRound(data);
                    break;
                case MessageType.PimpWeapon:
                    GameController.Instance.ReceivePimpWeapon(data);
                    break;
                case MessageType.ItemsChanged:
                    GameController.Instance.ReceiveItemsChanged(data);
                    break;
                case MessageType.Search:
                    GameController.Instance.ReceiveSearch(data);
                    break;
                case MessageType.Attack:
                    GameController.Instance.ReceiveAttack(data);
                    break;
                case MessageType.ZombieSpawn:
                    GameController.Instance.ReceiveZombieSpawns(data);
                    break;
                case MessageType.ZombieSpawnInBuilding:
                    GameController.Instance.ReceiveZombieSpawnsInBuilding(data);
                    break;
                case MessageType.TraitUpgrade:
                    GameController.Instance.ReceiveTraitUpgrade(data);
                    break;
                case MessageType.GameEnded:
                    NetworkManager.Singleton.Shutdown();
                    SceneManager.LoadScene("MenuScene");
                    break;
                case MessageType.SurvivorDied:
                    GameController.Instance.RemovePlayer(data);
                    break;
                case MessageType.PlayerLeft:
                    GameController.Instance.PlayerLeft(data);
                    break;
                case MessageType.PlayerDisconnected:
                    GameController.Instance.PlayerDisconnected(data);
                    break;
            }
        }
        /// <summary>
        /// Client applies the recieved action locally.
        /// </summary>
        /// <param name="playerId">ID of the player doing the action</param>
        /// <param name="actionName">name of the action</param>
        /// <param name="objectName">name of the object used in the action</param>
        [ClientRpc]
        public void ApplyActionClientRpc(ulong playerId, string actionName, string objectName)
        {
            Debug.Log($"[CLIENT] Player {playerId} executed action: {actionName} on tile {objectName}");

            GameController.Instance.ApplyActionLocally(playerId,actionName, objectName);
        }
        #endregion
        #region Event Handlers
        /// <summary>
        /// Eventhandler of a client disconnecting.
        /// </summary>
        /// <param name="clientId">ID of the client who disconnected</param>
        private void OnClientDisconnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                if (!NetworkManager.Singleton.IsHost)
                {
                    NetworkManager.Singleton.Shutdown();
                    SceneManager.LoadScene("MenuScene");
                }
            }
            else
            {
                if (NetworkManager.Singleton.IsHost)
                {
                    SendMessageToClientsServerRpc(MessageType.PlayerDisconnected, clientId.ToString());
                }
            }
        }
        #endregion
        #endregion
    }
}
