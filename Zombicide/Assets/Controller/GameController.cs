using Model;
using Network;
using Persistence;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private GameModel gameModel;
    public GameObject tilePrefab; // A TileDisplay Prefab
    public Transform boardParent; // Ide ker�lnek a p�lyar�szek
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
        Debug.Log("GameModel l�trehozva a k�vetkez� p�ly�val: " + 0);

        gameModel.LoadGame(mapID);
        Debug.Log(gameModel.Board.Buildings.Count);
        Debug.Log(gameModel.Board.Streets.Count);
        Debug.Log(gameModel.Board.Tiles.Count);
        GenerateBoard(mapID);
    }
    public void GenerateBoard(int mapID)
    {

        MapVisualLoader loader = new MapVisualLoader();
        List<TileVisualData> tiles = loader.LoadVisualMap(mapID);

        foreach (var tileData in tiles)
        {
            // Bet�ltj�k a megfelel� k�pet
            Sprite tileSprite = Resources.Load<Sprite>("Tiles/" + tileData.tileName);
            if (tileSprite == null)
            {
                Debug.LogError("Nem tal�lhat� sprite: " + tileData.tileName);
                continue;
            }

            // Prefab p�ld�nyos�t�sa
            GameObject newTile = Instantiate(tilePrefab, boardParent);
            TileDisplay tileDisplay = newTile.GetComponent<TileDisplay>();

            // Be�ll�tjuk a poz�ci�t �s forgat�st
            tileDisplay.Setup(tileSprite, tileData.position, tileData.rotation);
        }
    }
    private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName == "InGameScene") // Csak az InGameScene-n�l l�p�nk be
        {
            Debug.Log("GameController inicializ�l�sa...");

            int selectedMapID = NetworkManagerController.Instance.SelectedMapID;
            Initialize(selectedMapID);
        }
    }
}
