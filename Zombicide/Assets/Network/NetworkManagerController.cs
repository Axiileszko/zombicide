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

        public void StartHost(int playerCount, List<string> characters, string selectedCharacter)
        {
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

    }
}
