using Model;
using Model.Characters.Survivors;
using Network;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private GameModel gameModel;
    private GameObject MapPrefab;
    private int selectedMapID;
    private Dictionary<ulong, string> playerSelections;
    private GameObject charImagePrefab;
    private HorizontalLayoutGroup charListContainer;
    public static GameController Instance { get; private set; }

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
        }
    }

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
    public void Initialize(int mapID)
    {
        Debug.Log("Lefut az initialize");
        gameModel = new GameModel();
        gameModel.StartGame(playerSelections.Values.ToList(),mapID);
        GenerateBoard(mapID);
        if (NetworkManager.Singleton.LocalClientId==0)
        {
            gameModel.DecidePlayerOrder(null);
            // Küldjük a sorrendet a klienseknek
            string serializedOrder = string.Join(',', gameModel.PlayerOrder.Select(x => x.Name));
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.PlayerOrder, serializedOrder);

            StartNextTurn();
        }
    }
    private void StartNextTurn()
    {
        ShowPlayerOrder();
        gameModel.CurrentPlayer.StartedRound = true;

        // A hálózati managernek szólunk
        NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.TurnStart, GetClientIdByCharacter(gameModel.CurrentPlayer.Name).ToString());
    }

    private void ShowPlayerOrder()
    {
        if(charImagePrefab==null || charListContainer == null)
        {
            charImagePrefab = Resources.Load<GameObject>($"Prefabs/CharImagePrefab");
            charListContainer = GameObject.FindFirstObjectByType<HorizontalLayoutGroup>();
        }
        // Töröljük az elõzõ listát
        foreach (Transform child in charListContainer.transform)
        {
            Destroy(child.gameObject);
        }
  
        foreach (var item in gameModel.PlayerOrder)
        {
            GameObject playerEntry = Instantiate(charImagePrefab, charListContainer.transform);
            playerEntry.GetComponent<Image>().sprite= Resources.Load<Sprite>($"Characters/{item.Name.ToLower().Replace(" ",string.Empty)+"_head"}");
            Outline outline = playerEntry.GetComponent<Outline>();
            outline.enabled = gameModel.CurrentPlayer==item;
        }
        GameObject zombieEntry = Instantiate(charImagePrefab, charListContainer.transform);
        zombieEntry.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Characters/zombie_head");
        Outline zoutline = zombieEntry.GetComponent<Outline>();
        zoutline.enabled = false;
        //majd állitsuk be a zombik outlinejat is rendesen
    }

    public void ReceivePlayerOrder(string serializedOrder)
    {
        List<string> orderedPlayers = serializedOrder.Split(',').ToList();
        gameModel.DecidePlayerOrder(orderedPlayers);
    }
    public void SetPlayerSelections(Dictionary<ulong, string> selections)
    {
        Debug.Log("Lefut a SetPlayerSelections");
        playerSelections = selections;
        Initialize(selectedMapID);
    }
    public void GenerateBoard(int mapID)
    {
        MapPrefab = Resources.Load<GameObject>($"Prefabs/Missions/Map_{mapID}");
        GameObject.Instantiate(MapPrefab);

    }
    private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Debug.Log("Lefut az OnSceneLoaded");
        if (sceneName == "InGameScene")
        {
            selectedMapID = NetworkManagerController.Instance.SelectedMapID;
            NetworkManagerController.Instance.SendPlayerSelectionsServerRpc();
        }
    }
    public ulong? GetClientIdByCharacter(string characterName)
    {
        foreach (var entry in playerSelections)
        {
            if (entry.Value == characterName)
            {
                return entry.Key; // Megtaláltuk a karaktert, visszaadjuk a kliens ID-t
            }
        }
        return null; // Ha nem található a karakter, visszatérünk null-lal
    }
    public void ReceiveTurnStart(ulong playerID)
    {
        UpdateBoardForActivePlayer(playerID);
    }

    private void UpdateBoardForActivePlayer(ulong playerID)
    {
        // Itt kapcsoljuk ki/be az interakciót
        bool isActive = playerID == NetworkManager.Singleton.LocalClientId;
        Debug.Log($"Host-e: {NetworkManager.Singleton.IsHost} current player: {gameModel.CurrentPlayer} isActive {isActive}");
        EnableBoardInteraction(isActive);
    }
    public void EnableBoardInteraction(bool enable)
    {
        //SubTile objektumok collidereinek engedélyezése/tiltása
        foreach (Transform child in GameObject.FindWithTag("MapPrefab").transform)
        {
            if (child.name.StartsWith("SubTile_")) // Csak a kattintható részeket érinti
            {
                var collider = child.GetComponent<BoxCollider>();
                if (collider != null)
                    collider.enabled = enable;
            }

            //3D objektumok kezelése: ha van benne collider, akkor engedélyezzük/tiltjuk
            foreach (Transform subChild in child)
            {
                var collider = subChild.GetComponent<BoxCollider>();
                if (collider != null)
                    collider.enabled = enable;
            }
        }
    }

    public void EndTurn()
    {
        if (NetworkManager.Singleton.LocalClientId != GetClientIdByCharacter(gameModel.CurrentPlayer.Name))
            return; // Csak az aktív játékos hívhatja meg

        gameModel.CurrentPlayer.FinishedRound = true;

        // Ha mindenki végzett, jönnek a zombik
        if (gameModel.PlayerOrder.All(p => p.FinishedRound))
        {
            gameModel.EndRound();
        }
        else
        {
            StartNextTurn();
        }
    }
}
