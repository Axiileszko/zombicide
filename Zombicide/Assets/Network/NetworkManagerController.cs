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
    public class NetworkManagerController:NetworkBehaviour
    {
        private List<string> availableCharacters = new List<string>();
        private Dictionary<ulong, string> selectedCharacters = new Dictionary<ulong, string>();
        private int expectedPlayerCount;
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



    }
}
