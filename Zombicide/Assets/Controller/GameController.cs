using Model;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private GameModel gameModel;
    public static GameController Instance { get; private set; }

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
    public void Initialize()
    {
        gameModel = new GameModel();
        Debug.Log("GameModel létrehozva a következõ pályával: " + 0);

        gameModel.LoadGame(0);
        Debug.Log(gameModel.Board.Buildings.Count);
        Debug.Log(gameModel.Board.Streets.Count);
        Debug.Log(gameModel.Board.Tiles.Count);
    }
}
