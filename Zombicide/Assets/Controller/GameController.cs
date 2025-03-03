using Model;
using Network;
using Persistence;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private GameModel gameModel;
    private GameObject MapPrefab;
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
        gameModel = new GameModel();
        Debug.Log("GameModel létrehozva a következõ pályával: " + 0);

        gameModel.LoadGame(mapID);
        Debug.Log(gameModel.Board.Buildings.Count);
        Debug.Log(gameModel.Board.Streets.Count);
        Debug.Log(gameModel.Board.Tiles.Count);
        GenerateBoard(mapID);
    }
    public void GenerateBoard(int mapID)
    {
        MapPrefab = Resources.Load<GameObject>($"Prefabs/Missions/Map_{mapID}");
        GameObject.Instantiate(MapPrefab);

    }
    private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName == "InGameScene") // Csak az InGameScene-nél lépünk be
        {
            Debug.Log("GameController inicializálása...");

            int selectedMapID = NetworkManagerController.Instance.SelectedMapID;
            Initialize(selectedMapID);
        }
    }
}
