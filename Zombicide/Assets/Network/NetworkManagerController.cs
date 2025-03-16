using Model;
using Model.Characters.Survivors;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

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
        ZombieSpawn
    }
    public class NetworkManagerController:NetworkBehaviour
    {
        private List<string> availableCharacters = new List<string>();
        private Dictionary<ulong, string> selectedCharacters = new Dictionary<ulong, string>();
        private int expectedPlayerCount;
        private bool hasSentPlayerSelections = false;  // Flag a dupla hívás ellen
        public int SelectedMapID { get; private set; }
        public static NetworkManagerController Instance { get; private set; }

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

        public void StartHost(int playerCount, List<string> characters, string selectedCharacter, int selectedMap)
        {
            SelectedMapID = selectedMap;
            expectedPlayerCount = playerCount;
            selectedCharacters[NetworkManager.Singleton.LocalClientId] = selectedCharacter;
            availableCharacters = new List<string>(characters);
            MenuController.Instance.UpdateLobbyDisplay(selectedCharacters);
        }

        // **Kliens kér egy karakterlistát a hosttól**
        [ServerRpc(RequireOwnership = false)]
        public void RequestCharacterListServerRpc(ulong clientId)
        {
            string characterListString=string.Join(",", availableCharacters);

            // Ha a host hívja meg ezt, visszaküldi az elérhető karaktereket a kért kliensnek
            SendCharacterListClientRpc(characterListString, clientId);
        }

        // **A host elküldi az elérhető karaktereket egy kliensnek**
        [ClientRpc]
        private void SendCharacterListClientRpc(string characterList, ulong targetClientId)
        {
            if (NetworkManager.Singleton.LocalClientId == targetClientId)
            {
                // Szétválasztjuk a stringet egy tömbbé
                string[] characters = characterList.Split(',');

                // A kliens frissíti a karakterválasztóját
                MenuController.Instance.UpdateCharacterDropdown(characters);
            }
        }
        [ServerRpc(RequireOwnership = false)]
        public void SelectCharacterServerRpc(ulong clientId, string character)
        {
            if (!availableCharacters.Contains(character))
            {
                return; // Ha valaki már kiválasztotta ezt a karaktert, nem engedjük
            }

            availableCharacters.Remove(character);
            selectedCharacters[clientId] = character;

            // Ha minden játékos csatlakozott, indítsuk a játékot!
            if (selectedCharacters.Count == expectedPlayerCount)
            {
                StartGameClientRpc();
            }

            // Frissítjük az összes kliens karakterválasztóját
            string characterListString = string.Join(",", availableCharacters);
            UpdateAllClientsCharacterDropdownClientRpc(characterListString);

            // Frissítjük az összes kliens lobby képernyőjét
            string selectedCharactersString = string.Join(";", selectedCharacters.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
            UpdateLobbyClientRpc(selectedCharactersString);
        }

        [ClientRpc]
        private void UpdateAllClientsCharacterDropdownClientRpc(string characterList)
        {
            string[] characters = characterList.Split(',');
            MenuController.Instance.UpdateCharacterDropdown(characters);
        }

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

            // A MenuController frissíti a lobby megjelenítését
            MenuController.Instance.UpdateLobbyDisplay(selectedCharacters);
        }
        public void LobbyCancel()
        {
            ulong localClientId = NetworkManager.Singleton.LocalClientId;

            // Ha a host nyomta meg a gombot
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("A host megszakította a lobbit. Mindenki lecsatlakozik...");

                // A host leállítja a szervert
                NetworkManager.Singleton.Shutdown();
            }
            else
            {
                Debug.Log($"Kliens ({localClientId}) kilépett a lobbiból.");

                // Kliens kilép, és elküldi a szervernek
                RequestLeaveLobbyServerRpc(localClientId);

                // A kliens lecsatlakozik
                NetworkManager.Singleton.Shutdown();
            }

            // Visszatérés a főmenübe
            MenuController.Instance.ShowMainMenu();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestLeaveLobbyServerRpc(ulong clientId)
        {
            if (selectedCharacters.ContainsKey(clientId))
            {
                // A karaktert visszaadjuk az elérhető listába
                string character = selectedCharacters[clientId];
                availableCharacters.Add(character);

                // Eltávolítjuk a kliens adatait
                selectedCharacters.Remove(clientId);

                Debug.Log($"Kliens ({clientId}) kilépett. Karakter ({character}) visszaadva.");
            }

            ulong[] clientIds = selectedCharacters.Keys.ToArray();
            string characterNamesCSV = string.Join(",", selectedCharacters.Values); // String listát alakítunk ki

            UpdateLobbyDisplayClientRpc(clientIds, characterNamesCSV);
        }

        [ClientRpc]
        private void UpdateLobbyDisplayClientRpc(ulong[] clientIds, string characterNamesCSV)
        {
            Dictionary<ulong, string> updatedCharacters = new Dictionary<ulong, string>();

            string[] characterNames = characterNamesCSV.Split(','); // Szétbontás tömbbé

            for (int i = 0; i < clientIds.Length; i++)
            {
                updatedCharacters[clientIds[i]] = characterNames[i];
            }

            // A kliens oldalon frissítjük a lobby UI-t
            MenuController.Instance.UpdateLobbyDisplay(updatedCharacters);
        }
        public void SubscribeClient()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        }
        private void OnClientDisconnect(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log("A kapcsolat megszakadt. Visszatérés a főmenübe...");
                MenuController.Instance.ShowMainMenu();
            }
        }

        [ClientRpc]
        private void StartGameClientRpc()
        {
            Debug.Log("Minden játékos csatlakozott! Játék indítása...");

            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("InGameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendPlayerSelectionsServerRpc()
        {
            if (hasSentPlayerSelections)
            {
                return;
            }

            hasSentPlayerSelections = true; // Flag beállítása
            if (!NetworkManager.Singleton.IsHost) return;

            // A karakterválasztások átalakítása listába
            List<ulong> clientIDs = new List<ulong>(selectedCharacters.Keys);
            List<string> characterNames = new List<string>(selectedCharacters.Values);

            // String tömbök sorosítása
            string clientIDsSerialized = string.Join(",", clientIDs);
            string characterNamesSerialized = string.Join(",", characterNames);

            // Küldés a klienseknek
            ReceivePlayerSelectionsClientRpc(clientIDsSerialized, characterNamesSerialized);
        }

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
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendMessageToClientsServerRpc(MessageType type, string data)
        {
            ReceiveMessageClientRpc(type, data);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestActionServerRpc(ulong playerId, string actionName, string objectName)
        {
            Debug.Log($"[SERVER] Player {playerId} requested action: {actionName} on tile {objectName}");
            ApplyActionClientRpc(playerId, actionName, objectName);
        }

        [ClientRpc]
        public void ApplyActionClientRpc(ulong playerId, string actionName, string objectName)
        {
            Debug.Log($"[CLIENT] Player {playerId} executed action: {actionName} on tile {objectName}");

            // Minden kliens végrehajtja az akciót
            GameController.Instance.ApplyActionLocally(playerId,actionName, objectName);
        }
    }
}
