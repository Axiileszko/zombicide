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
        public static NetworkManagerController Instance { get; private set; }

        private string sessionCode;
        private int maxPlayers;
        private List<string> availableCharacters = new List<string>();
        public List<string> AvailableCharacters {  get { return availableCharacters; } }

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
            maxPlayers = playerCount;
            sessionCode = GenerateSessionCode();
            availableCharacters = characters;

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.StartHost();

            Debug.Log($"Szerver elindítva! Csatlakozási kód: {sessionCode}");
        }
        public void ConnectClient()
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.StartClient();
            }
        }
        public bool CheckCode(string inputCode, out string error)
        {
            if (inputCode != sessionCode)
            {
                error = "Érvénytelen kód!" +" "+sessionCode;
                return false;
            }

            error = "";
            return true;
        }
        private string GenerateSessionCode()
        {
            return UnityEngine.Random.Range(1000, 9999).ToString();
        }
        public void JoinGame(string selectedCharacter)
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.StartClient();
            }

            Debug.Log($"Csatlakozás a szerverhez... Kiválasztott karakter: {selectedCharacter}");
        }
        public bool SelectCharacter(string character)
        {
            if (!availableCharacters.Contains(character))
            {
                return false;
            }

            availableCharacters.Remove(character);
            return true;
        }

        private void OnClientConnected(ulong clientId)
        {
            int connectedClients = NetworkManager.Singleton.ConnectedClientsList.Count;

            if (connectedClients > maxPlayers)
            {
                Debug.Log($"Túl sok játékos! Kiléptetjük a klienst: {clientId}");
                NetworkManager.Singleton.DisconnectClient(clientId);
            }
            else
            {
                Debug.Log($"Új játékos csatlakozott! (ID: {clientId})");
            }
        }
        public string GetSessionCode()
        {
            return sessionCode;
        }
    }
}
