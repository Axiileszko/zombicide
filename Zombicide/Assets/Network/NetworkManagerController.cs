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

        public void StartHost(int playerCount, List<string> characters)
        {
            availableCharacters = new List<string>(characters); // Tároljuk a karakterlistát
            //NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        //public void OnClientConnected(ulong clientId)
        //{
        //    string characterListString = string.Join(",", availableCharacters);
        //    SendCharacterListClientRpc(characterListString, clientId);
        //}

        // **Kliens kér egy karakterlistát a hosttól**
        [ServerRpc(RequireOwnership = false)]
        public void RequestCharacterListServerRpc(ulong clientId)
        {
            string characterListString=string.Join(",", availableCharacters);
            Debug.Log(characterListString);//test

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
        }

        [ClientRpc]
        private void UpdateAllClientsCharacterDropdownClientRpc(string characterList)
        {
            string[] characters = characterList.Split(',');
            MenuController.Instance.UpdateCharacterDropdown(characters);
        }
    }
}
