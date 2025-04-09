using Model;
using Model.Board;
using Model.Characters.Survivors;
using Model.Characters.Zombies;
using Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using DG.Tweening;
using Assets.Controller;

public class GameController : MonoBehaviour
{
    #nullable enable
    #region Fields
    private GameModel? gameModel;
    private Survivor? survivor;
    private GameObject? MapPrefab;
    [SerializeField] private GameObject? cameraDrag;
    [SerializeField] private DiceRoller? diceRoller;
    [SerializeField] private GameObject? rightHand;
    [SerializeField] private GameObject? leftHand;
    [SerializeField] private List<GameObject> backPack=new List<GameObject>();
    private GameObject? aPointsText;
    private GameObject? healthText;
    private GameObject? usedActionsText;
    private GameObject? freeActionsLayoutGroup;
    private int selectedMapID;
    private Dictionary<ulong, string> playerSelections=new Dictionary<ulong, string>();
    private Dictionary<string, GameObject> playerPrefabs=new Dictionary<string, GameObject>();
    private GameObject? charImagePrefab;
    private HorizontalLayoutGroup? charListContainer;
    private GameObject? inventoryForS;
    private GameObject? sniperMenuForS;
    private bool isMenuOpen=false;
    private bool isSurvivorDead=false;
    private Dictionary<int, GameObject> zombieCanvases = new Dictionary<int, GameObject>();
    #endregion
    #region Properties
    public string? AttackFlag {  get; set; }
    public static GameController? Instance { get; private set; }
    #endregion
    #region Methods
    /// <summary>
    /// When the object is created, we subscribe and unsubscribe to the scene loading event.
    /// </summary>
    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
        }
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// Instantiate the model and load all UI elements to start the game.
    /// </summary>
    /// <param name="mapID">ID of the map selected by the host</param>
    private void Initialize(int mapID)
    {
        gameModel = new GameModel();
        gameModel.StartGame(playerSelections.Values.ToList(),mapID);
        gameModel.GameEnded += GameModel_GameEnded;
        GenerateBoard(mapID);
        GeneratePlayersOnBoard();
        survivor = gameModel.GetSurvivorByName(playerSelections[NetworkManager.Singleton.LocalClientId]);
        survivor.SetReference(gameModel);
        if (NetworkManager.Singleton.IsHost)
        {
            gameModel.DecidePlayerOrder();

            // The host sends the predetermined player order
            string serializedOrder = string.Join(',', gameModel.PlayerOrder.Select(x => x.Name));
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.PlayerOrder, serializedOrder);

            // The host sends the starting weapons to the players
            List<Weapon> genericW=gameModel.GenerateGenericWeapons();
            string serializedGenericW = string.Join(",", genericW.Select(x => x.Name));
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.GenericWeapon, serializedGenericW);
        }
        ShowPlayerUI();
        if(NetworkManager.Singleton.IsHost)
            StartNextTurn();
        // Subscribe to the death event of all characters in the game
        foreach (var item in playerSelections.Values)
        {
            SurvivorFactory.GetSurvivorByName(item).SurvivorDied += GameController_SurvivorDied;
        }
    }
    #region Query Methods
    /// <summary>
    /// Returns the actions that the player can perform on the given tile.
    /// </summary>
    /// <param name="tileName">Name of the tile object the player clicked</param>
    /// <returns>String list of the actions</returns>
    public List<string> GetAvailableActionsOnTile(string tileName)
    {
        SurvivorFactory.GetSurvivorByName(playerSelections[NetworkManager.Singleton.LocalClientId]).SetActions(gameModel!.Board.GetTileByID(int.Parse(tileName.Substring(8))));
        return SurvivorFactory.GetSurvivorByName(playerSelections[NetworkManager.Singleton.LocalClientId]).Actions.Keys.ToList();
    }
    /// <summary>
    /// Returns the items the player is holding that can be used to open doors.
    /// </summary>
    /// <returns>String list of the items</returns>
    public List<string> GetAvailableDoorOpeners()
    {
        List<string> result = new List<string>();
        if (survivor!.RightHand != null && survivor.RightHand is Weapon weapon && weapon.CanOpenDoors) result.Add("Right Hand");
        if (survivor.LeftHand != null && survivor.LeftHand is Weapon weapon2 && weapon2.CanOpenDoors) result.Add("Left Hand");
        return result;
    }
    /// <summary>
    /// Returns the attacks the player can perform on the given tile.
    /// </summary>
    /// <param name="tileID">ID of the tile the player clicked</param>
    /// <returns>String list of the attacks</returns>
    public List<string> GetAvailableAttacks(string tileID)
    {
        List<string> options = survivor!.GetAvailableAttacksOnTile(gameModel!.Board.GetTileByID(int.Parse(tileID)));
        if (options.Contains("Melee") && survivor.CurrentTile.Id != int.Parse(tileID))
            options.Remove("Melee");
        return options;
    }
    /// <summary>
    /// Returns the weapons in the player's hands that match the given parameters.
    /// </summary>
    /// <param name="isMelee">Is the attack melee or range</param>
    /// <param name="tileID">ID of the tile the player clicked</param>
    /// <returns>String list of the weapons</returns>
    public List<string> GetAvailableWeapons(bool isMelee, string tileID)
    {
        List<string> result = new List<string>();
        if (isMelee)
        {
            if(survivor!.LeftHand != null && survivor.LeftHand is Weapon weapon && weapon.CanBeUsedAsMelee)
                result.Add("Left Hand");
            if (survivor.RightHand != null && survivor.RightHand is Weapon weapon2 && weapon2.CanBeUsedAsMelee)
                result.Add("Right Hand");
        }
        else
        {
            int distance = 1;
            if (survivor!.CurrentTile.Type == TileType.STREET && gameModel!.Board.GetTileByID(int.Parse(tileID)).Type==TileType.STREET)
            {
                var street = gameModel.Board.GetStreetByTiles(survivor.CurrentTile.Id, int.Parse(tileID));
                distance = Math.Abs(street.Tiles.IndexOf(survivor.CurrentTile.Id) - street.Tiles.IndexOf(int.Parse(tileID)));
            }
            if (survivor.LeftHand != null && survivor.LeftHand is Weapon weapon && distance <= weapon.Range)
                result.Add("Left Hand");
            if (survivor.RightHand != null && survivor.RightHand is Weapon weapon2 && distance <= weapon2.Range)
                result.Add("Right Hand");
        }
        return result;
    }
    #endregion
    #region UI Methods
    /// <summary>
    /// Animates the door gameobject before destroying it.
    /// </summary>
    /// <param name="door">Door that was clicked</param>
    private void DestroyDoorWithTween(GameObject door)
    {
        door.transform.DORotate(new Vector3(0, 90, 0), 1f, RotateMode.WorldAxisAdd)
            .OnComplete(() => Destroy(door));
    }
    /// <summary>
    /// Destroyes the given object with animation
    /// </summary>
    /// <param name="obj">Gameobject that will be destroyed</param>
    public void DestroyWithTween(GameObject obj)
    {
        obj.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).OnComplete(() => Destroy(obj));
    }
    /// <summary>
    /// Loads the map prefab corresponding to the given ID.
    /// </summary>
    /// <param name="mapID">ID of the map that needs to be loaded</param>
    private void GenerateBoard(int mapID)
    {
        MapPrefab = Resources.Load<GameObject>($"Prefabs/Missions/Map_{mapID}");
        GameObject.Instantiate(MapPrefab);
    }
    /// <summary>
    /// Displays the player order for the current round.
    /// </summary>
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
        GameObject zombieEntry = Instantiate(charImagePrefab, charListContainer.transform);
        zombieEntry.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Characters/zombie_head");
        Outline zoutline = zombieEntry.GetComponent<Outline>();
        zoutline.enabled = gameModel!.IsPlayerRoundOver;
        //majd állitsuk be a zombik outlinejat is rendesen

        List<Survivor> reversed = gameModel.PlayerOrder.ToList();
        reversed.Reverse();
        foreach (var item in reversed)
        {
            GameObject playerEntry = Instantiate(charImagePrefab, charListContainer.transform);
            playerEntry.GetComponent<Image>().sprite= Resources.Load<Sprite>($"Characters/{item.Name.ToLower().Replace(" ",string.Empty)+"_head"}");
            Outline outline = playerEntry.GetComponent<Outline>();
            outline.enabled = gameModel.CurrentPlayer==item;
        }
        
    }
    /// <summary>
    /// Displays the panel associated with the selected character.
    /// </summary>
    private void ShowPlayerUI()
    {
        string name = playerSelections[NetworkManager.Singleton.LocalClientId];
        GameObject ui = GameObject.FindWithTag("GameUI");
        GameObject panelPrefab = Resources.Load<GameObject>($"Prefabs/Players/PlayerPanel_{name.Replace(" ", string.Empty)}");
        foreach(Transform child in panelPrefab.transform)
        {
            var component=child.GetComponent<HoverClickHandlerForPanel>();
            if (component != null)
            {
                CameraZoom.PanelHoverScript=component;
                CameraDrag.PanelHoverScript =component;
            }
        }
        GameObject player = Instantiate(panelPrefab,ui.transform);
        foreach (Transform child in player.transform)
        {
            if (child.name == "Data")
            {
                foreach (Transform subChild in child.transform)
                {
                    if (subChild.name == "Health")
                        healthText = subChild.gameObject;
                    else if (subChild.name == "Points")
                        aPointsText = subChild.gameObject;
                }
            }
            if (child.name =="UsedActions")
                usedActionsText = child.gameObject;
            if(child.name == "FreeActions")
                freeActionsLayoutGroup = child.gameObject;
        }
        UpdatePlayerStats();
    }
    /// <summary>
    /// Generates the players' figurines on the starting tile.
    /// </summary>
    private void GeneratePlayersOnBoard()
    {
        Transform tile = GameObject.FindWithTag("MapPrefab").transform.Find($"SubTile_{gameModel!.StartTile.Id}");
        BoxCollider collider = tile.GetComponent<BoxCollider>();
        float startX = collider.transform.position.x - 2f;
        float startZ = collider.transform.position.z + 0.5f;
        float startY = 2f;
        Vector3 newPosition = new Vector3();
        newPosition.x = startX; newPosition.y = startY; newPosition.z = startZ;
        int multiply = 1;

        foreach (var item in playerSelections.Values)
        {
            GameObject playerPrefab = Resources.Load<GameObject>($"Prefabs/Players/{item.Replace(" ",string.Empty)}");
            GameObject player=Instantiate(playerPrefab);
            playerPrefabs.Add(item.Replace(" ", string.Empty), player);
            player.transform.position = newPosition;
            if (multiply < 3)
            {
                newPosition.x = startX + (multiply * 1.5f);
                newPosition.z = startZ;
            }
            else if(multiply<5)
            {
                newPosition.x = startX + ((multiply - 3) * 1.5f);
                newPosition.z = startZ - 0.7f;
            }
            else
            {
                newPosition.x = startX + ((multiply - 5) * 1.5f);
                newPosition.z = startZ - (2 * 0.7f);
            }
            multiply++;
        }
    }
    /// <summary>
    /// Moves the given player's figurine to the specified tile.
    /// </summary>
    /// <param name="tileID">ID of the tile the object should be put to</param>
    /// <param name="player">Object that needs to be moved</param>
    private void MovePlayerToTile(int tileID, GameObject player)
    {
        RearrangePlayersOnTile(tileID, player.gameObject.name.Replace("(Clone)",""));
        Transform tile = GameObject.FindWithTag("MapPrefab").transform.Find($"SubTile_{tileID}");
        BoxCollider collider = tile.GetComponent<BoxCollider>();
        float startX=collider.transform.position.x-2f;
        float startZ=collider.transform.position.z + 0.5f;
        float startY = 2f; //ez mindig marad
        int playerCount = gameModel!.NumberOfPlayersOnTile(tileID)-1;
        Vector3 newPosition = player.transform.position;

        if (playerCount < 3)
        {
            newPosition.x = startX + (playerCount * 1.5f);
            newPosition.z = startZ;
        }
        else if (playerCount < 5)
        {
            newPosition.x = startX + ((playerCount - 3) * 1.5f);
            newPosition.z = startZ - 0.7f;
        }
        else
        {
            newPosition.x = startX + ((playerCount - 5) * 1.5f);
            newPosition.z = startZ - (2 * 0.7f);
        }
        newPosition.y = startY;
        player.transform.DOMove(newPosition, 0.5f).SetEase(Ease.OutQuad);
    }
    /// <summary>
    /// Rearranges the players that are already on the tile
    /// </summary>
    /// <param name="tileID">ID of the tile</param>
    /// <param name="steppingPlayer">Player who wants to move</param>
    private void RearrangePlayersOnTile(int tileID, string steppingPlayer)
    {
        List<string> list = gameModel!.GetSurvivorsOnTile(gameModel.Board.GetTileByID(tileID)).Select(x=>x.Name).ToList();
        list.Remove(steppingPlayer);
        if (list.Count == 0) return;
        Transform tile = GameObject.FindWithTag("MapPrefab").transform.Find($"SubTile_{tileID}");
        BoxCollider collider = tile.GetComponent<BoxCollider>();
        float startX = collider.transform.position.x - 2f;
        float startZ = collider.transform.position.z + 0.5f;
        float startY = 2f;
        Vector3 newPosition = new Vector3();
        newPosition.x = startX; newPosition.y = startY; newPosition.z = startZ;
        int multiply = 1;
        foreach (string s in list)
        {
            GameObject player = playerPrefabs[s.Replace(" ", string.Empty)];
            player.transform.DOMove(newPosition, 0.5f).SetEase(Ease.OutQuad);
            if (multiply < 3)
            {
                newPosition.x = startX + (multiply * 1.5f);
                newPosition.z = startZ;
            }
            else if (multiply < 5)
            {
                newPosition.x = startX + ((multiply - 3) * 1.5f);
                newPosition.z = startZ - 0.7f;
            }
            else
            {
                newPosition.x = startX + ((multiply - 5) * 1.5f);
                newPosition.z = startZ - (2 * 0.7f);
            }
            multiply++;
        }
    }
    /// <summary>
    /// Updates the number and type of zombies on the given tile.
    /// </summary>
    /// <param name="tileID">ID of the tile the zombies should be updated on</param>
    private void UpdateZombieCanvasOnTile(int tileID)
    {
        if(zombieCanvases.ContainsKey(tileID))
        {
            Destroy(zombieCanvases[tileID]);
            zombieCanvases.Remove(tileID);
        }
        List<Zombie> zombies = gameModel!.GetZombiesInPriorityOrderOnTile(gameModel.Board.GetTileByID(tileID));
        if(zombies!=null && zombies.Count > 0)
        {
            GameObject zombieCanvasPrefab = Resources.Load<GameObject>("Prefabs/ZombieCanvas");
            Transform tile = GameObject.FindWithTag("MapPrefab").transform.Find($"SubTile_{tileID}");
            BoxCollider collider = tile.GetComponent<BoxCollider>();
            float startX = collider.transform.position.x - 0.8f;
            float startZ = collider.transform.position.z-1f;
            float startY = 1.8f;
            Vector3 newPosition = new Vector3();
            newPosition.x = startX; newPosition.y = startY; newPosition.z = startZ;
            GameObject zc = Instantiate(zombieCanvasPrefab);
            zc.transform.position = newPosition;
            Transform panel=zc.transform.GetChild(0);
            GameObject zombiePrefab = Resources.Load<GameObject>("Prefabs/Zombie");
            foreach (string zombie in zombies.Select(x=>x.GetType().Name).Distinct())
            {
                int amount = zombies.Count(x => x.GetType().Name == zombie);
                GameObject newZombie = Instantiate(zombiePrefab, panel.transform);
                newZombie.GetComponent<Image>().sprite= Resources.Load<Sprite>($"Characters/Zombies/{zombie}");
                newZombie.transform.GetChild(0).GetComponent<TMP_Text>().text = amount.ToString();
            }
            zombieCanvases.Add(tileID, zc);
        }
    }
    /// <summary>
    /// Updates the player's data on the panel.
    /// </summary>
    private void UpdatePlayerStats()
    {
        healthText!.GetComponent<TMP_Text>().text = survivor!.HP.ToString();
        aPointsText!.GetComponent<TMP_Text>().text=survivor.APoints.ToString();
        usedActionsText!.GetComponent<TMP_Text>().text=survivor.UsedAction.ToString();
        foreach (Transform child in freeActionsLayoutGroup!.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (var item in survivor.FreeActions)
        {
            var prefab= Resources.Load<GameObject>($"Prefabs/Players/FreeActionLabelPrefab");
            var newAction = Instantiate(prefab, freeActionsLayoutGroup.transform);
            newAction.GetComponent<TMP_Text>().text = item.Key;
        }
    }
    /// <summary>
    /// Updates the map interaction lock for the current player.
    /// </summary>
    /// <param name="playerID">ID of the player</param>
    private void UpdateBoardForActivePlayer(ulong playerID)
    {
        bool isActive = playerID == NetworkManager.Singleton.LocalClientId;
        EnableBoardInteraction(isActive);
    }
    /// <summary>
    /// Updates the player's inventory.
    /// </summary>
    private void UpdateItemSlots()
    {
        if (survivor!.LeftHand == null)
            leftHand!.GetComponent<Image>().sprite= Resources.Load<Sprite>("Objects/card_lefthand");
        else
            leftHand!.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{survivor.LeftHand.Name.ToString().ToLower()}");
        if (survivor.RightHand == null)
            rightHand!.GetComponent<Image>().sprite = Resources.Load<Sprite>("Objects/card_righthand");
        else
            rightHand!.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{survivor.RightHand.Name.ToString().ToLower()}");
        for (int i = 0; i < 3; i++)
        {
            if(survivor.BackPack.Count > i)
            {
                if (survivor.BackPack[i] == null)
                    backPack[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("Objects/card_backpack");
                else
                    backPack[i].GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{survivor.BackPack[i].Name.ToString().ToLower()}");
            }
        }
    }
    /// <summary>
    /// Locks or unlocks map interaction based on the parameter.
    /// </summary>
    /// <param name="enable">If true then enable otherwise disable</param>
    public void EnableBoardInteraction(bool enable)
    {
        if (enable && isMenuOpen) return; 
        //SubTile objektumok collidereinek engedélyezése/tiltása
        foreach (Transform child in GameObject.FindWithTag("MapPrefab").transform)
        {
            if (child.name.StartsWith("SubTile_")) // Csak a kattintható részeket érinti
            {
                var collider = child.GetComponent<BoxCollider>();
                if (collider != null)
                    collider.enabled = enable;
            }   
        }
    }
    /// <summary>
    /// Enables or disables clicking on doors on the map based on the parameter.
    /// </summary>
    /// <param name="enable">If true then enable otherwise disable</param>
    public void EnableDoors(bool enable)
    {
        isMenuOpen = true;
        foreach (Transform child in GameObject.FindWithTag("MapPrefab").transform)
        {
            if (child.name=="Doors")
            {
                foreach (Transform subChild in child.transform)
                {
                    if (int.Parse(subChild.name.Substring(5).Split('_')[0])==survivor!.CurrentTile.Id || int.Parse(subChild.name.Substring(5).Split('_')[1]) == survivor.CurrentTile.Id)
                    {
                        var collider = subChild.GetChild(0).GetComponent<BoxCollider>();
                        if (collider != null)
                            collider.enabled = enable;
                    }
                }
            }
        }
    }
    /// <summary>
    /// Handles the character of the player who has left the game.
    /// </summary>
    /// <param name="playerName">Name of the player who left the game</param>
    public void PlayerLeft(string playerName)
    {
        if(!SurvivorFactory.GetSurvivorByName(playerName).IsDead)
            SurvivorFactory.GetSurvivorByName(playerName).IsDead=true;
        RemovePlayer(playerName);
    }
    /// <summary>
    /// Handles the character of the player who disconnected from the game.
    /// </summary>
    /// <param name="clientID">ID of the player who disconnected</param>
    public void PlayerDisconnected(string clientID)
    {
        string playerName = playerSelections[ulong.Parse(clientID)];
        if (!SurvivorFactory.GetSurvivorByName(playerName).IsDead)
            SurvivorFactory.GetSurvivorByName(playerName).IsDead = true;
        RemovePlayer(playerName);
    }
    /// <summary>
    /// Deletes the figurine of the given player.
    /// </summary>
    /// <param name="playerName">Name of the player who should be removed</param>
    public void RemovePlayer(string playerName)
    {
        if (!playerPrefabs.ContainsKey(playerName.Replace(" ", string.Empty)))
            return;
        GameObject player = playerPrefabs[playerName.Replace(" ", string.Empty)];
        playerPrefabs.Remove(playerName.Replace(" ", string.Empty));
        Destroy(player);
        if (survivor!.Name == playerName)
            EnableBoardInteraction(false);
        if(gameModel!.CurrentPlayer==survivor && survivor.Name==playerName && !survivor.LeftExit)
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.FinishedRound, survivor.Name);
    }
    /// <summary>
    /// Displays the priority selection window.
    /// </summary>
    /// <param name="data">Contains the list of zombies</param>
    private void OpenPriorityMenu(string data)
    {
        List<Zombie> zombies = gameModel!.GetZombiesInPriorityOrderOnTile(gameModel.Board.GetTileByID(int.Parse(data.Split(':')[0])));
        isMenuOpen = true;
        cameraDrag!.SetActive(false);
        GameObject gameUI = GameObject.FindGameObjectWithTag("GameUI");
        GameObject sniperPrefab = Resources.Load<GameObject>($"Prefabs/SniperMenu");
        GameObject sniper = Instantiate(sniperPrefab, gameUI.transform);
        EnableBoardInteraction(false);
        List<string> zombieTypeList = zombies.Select(x=>x.GetType().Name).Distinct().ToList();
        foreach (Transform child in sniper.transform)
        {
            if (child.name.StartsWith("Zombie"))
            {
                int i = 0;
                foreach (Transform subChild in child.transform)
                {
                    if (i >=zombieTypeList.Count)
                        break;
                    if (zombieTypeList[i] != null)
                    {
                        GameObject itemPrefab = Resources.Load<GameObject>($"Prefabs/Inventory/Item");
                        GameObject item = Instantiate(itemPrefab, subChild.transform);
                        if (zombieTypeList[i].StartsWith("Hobo")|| zombieTypeList[i].StartsWith("Abo") || zombieTypeList[i].StartsWith("Pat"))
                            item.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Menu/AbominationSlot");
                        else
                            item.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Menu/{zombieTypeList[i]}Slot");
                    }
                    i++;
                }
            }
            else if (child.name.StartsWith("Ok"))
            {
                child.GetComponent<Button>().onClick.AddListener(() => {
                    OnOkPriorityMenuClicked(data);
                });
            }
        }
        sniperMenuForS = sniper;
    }
    /// <summary>
    /// Displays the inventory window.
    /// </summary>
    /// <param name="additionalItems">New items the player got</param>
    /// <param name="isSearch">Was the method called from a search</param>
    public void OpenInventory(List<Item>? additionalItems, bool isSearch)
    {
        isMenuOpen = true;
        cameraDrag!.SetActive(false);
        GameObject gameUI = GameObject.FindGameObjectWithTag("GameUI");
        GameObject inventoryPrefab = Resources.Load<GameObject>($"Prefabs/Inventory/Inventory");
        GameObject inventory = Instantiate(inventoryPrefab, gameUI.transform);
        EnableBoardInteraction(false);
        foreach (Transform child in inventory.transform)
        {
            if (child.name.StartsWith("Hand"))
            {
                foreach (Transform subChild in child.transform)
                {
                    if (subChild.name.Substring(14) == "Left" && survivor!.LeftHand != null)
                    {
                        GameObject itemPrefab = Resources.Load<GameObject>($"Prefabs/Inventory/Item");
                        GameObject item = Instantiate(itemPrefab, subChild.transform);
                        item.GetComponent<Image>().sprite= Resources.Load<Sprite>($"Items/{survivor.LeftHand.Name.ToString().ToLower()}");
                    }
                    else if(subChild.name.Substring(14) == "Right" && survivor!.RightHand != null)
                    {
                        GameObject itemPrefab = Resources.Load<GameObject>($"Prefabs/Inventory/Item");
                        GameObject item = Instantiate(itemPrefab, subChild.transform);
                        item.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{survivor.RightHand.Name.ToString().ToLower()}");
                    }
                }
            }
            else if (child.name.StartsWith("Back"))
            {
                int i = 0;
                foreach (Transform subChild in child.transform)
                {
                    if (i >= survivor!.BackPack.Count)
                        break;
                    if (survivor.BackPack[i] != null)
                    {
                        GameObject itemPrefab = Resources.Load<GameObject>($"Prefabs/Inventory/Item");
                        GameObject item = Instantiate(itemPrefab, subChild.transform);
                        item.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{survivor.BackPack[i].Name.ToString().ToLower()}");
                    }
                    i++;
                }
            }
            else if(child.name.StartsWith("Throw") && additionalItems != null && additionalItems.Count > 0)
            {
                int i = 0;
                foreach (Transform subChild in child.transform)
                {
                    if (i >= additionalItems.Count)
                        break;
                    if (additionalItems[i] != null)
                    {
                        GameObject itemPrefab = Resources.Load<GameObject>($"Prefabs/Inventory/Item");
                        GameObject item = Instantiate(itemPrefab, subChild.transform);
                        item.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{additionalItems[i].Name.ToString().ToLower()}");
                    }
                    i++;
                }
            }
            else if (child.name.StartsWith("Ok"))
            {
                child.GetComponent<Button>().onClick.AddListener(() => {
                    OnOkInventoryClicked(isSearch);
                });
            }
        }
        inventoryForS=inventory;
    }
    #endregion
    #region NetworkManager Methods
    /// <summary>
    /// Returns the client ID associated with the given character.
    /// </summary>
    /// <param name="characterName">Name of the character</param>
    /// <returns>Client ID</returns>
    private ulong? GetClientIdByCharacter(string characterName)
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
    /// <summary>
    /// The client sets up the characters in the game and assigns their corresponding client IDs.
    /// </summary>
    /// <param name="selections">Character names and their client IDs</param>
    public void SetPlayerSelections(Dictionary<ulong, string> selections)
    {
        playerSelections = selections;
        Initialize(selectedMapID);
    }
    /// <summary>
    /// Sets and displays the given player order.
    /// </summary>
    /// <param name="serializedOrder">Player order</param>
    public void ReceivePlayerOrder(string serializedOrder)
    {
        List<string> orderedPlayers = serializedOrder.Split(',').ToList();
        gameModel!.SetPlayerOrder(orderedPlayers);
        ShowPlayerOrder();
    }
    /// <summary>
    /// Starts the turn for the specified player.
    /// </summary>
    /// <param name="playerID">ID of the player</param>
    public void ReceiveTurnStart(ulong playerID)
    {
        UpdateBoardForActivePlayer(playerID);
        if (gameModel!.CurrentPlayer == survivor)
        {
            survivor!.SetFreeActions();
            UpdatePlayerStats();
        }
    }
    /// <summary>
    /// Assigns the given starting weapons to the players.
    /// </summary>
    /// <param name="data">Contains the list of weapons</param>
    public void ReceiveGenericWeapons(string data)
    {
        List<ItemName> weapons=data.Split(',').Select(x=>(ItemName)Enum.Parse(typeof(ItemName), x.Replace("ItemName.",""),true)).ToList();
        gameModel!.GiveSurvivorsGenericWeapon(weapons);
        UpdateItemSlots();
    }
    /// <summary>
    /// Grants the specified trait to the given player.
    /// </summary>
    /// <param name="data">Contains the name of the player and the trait</param>
    public void ReceiveTraitUpgrade(string data)
    {
        string[] strings = data.Split(';');
        Survivor s = SurvivorFactory.GetSurvivorByName(strings[0]);
        s.UpgradeTo(int.Parse(strings[1]), int.Parse(strings[2])+1);
        if (s == survivor)
            UpdatePlayerStats();
        EnableBoardInteraction(survivor == gameModel!.CurrentPlayer);
    }
    /// <summary>
    /// Rearranges the received items for the player given.
    /// </summary>
    /// <param name="data">Contains the list of items</param>
    public void ReceiveItemsChanged(string data)
    {
        string[] strings = data.Split(':');
        Survivor s = SurvivorFactory.GetSurvivorByName(strings[1]);
        string[] lists= strings[0].Split(";");//left;right;backpack;throwaway
        string leftH = lists[0];
        string rightH = lists[1];
        List<string> backpack = lists[2].Split(",").ToList();
        List<Item> backpackItem = new List<Item>();
        List<string> throwAway = lists[3].Split(",").ToList();
        List<Item> throwAwayItem = new List<Item>();
        string isSearch = lists[4];
        if (rightH == string.Empty)
            s.PutIntoHand(true, null);
        else
            s.PutIntoHand(true, ItemFactory.GetItemByName((ItemName)Enum.Parse(typeof(ItemName), rightH, true)));
        if (leftH == string.Empty)
            s.PutIntoHand(false, null);
        else
            s.PutIntoHand(false, ItemFactory.GetItemByName((ItemName)Enum.Parse(typeof(ItemName), leftH, true)));
        foreach (var item in backpack)
        {
            if (item != string.Empty)
                backpackItem.Add(ItemFactory.GetItemByName((ItemName)Enum.Parse(typeof(ItemName), item, true)));
        }
        foreach (var item in throwAway)
        {
            if (item != string.Empty)
                throwAwayItem.Add(ItemFactory.GetItemByName((ItemName)Enum.Parse(typeof(ItemName), item, true)));
        }
        s.PutIntoBackpack(backpackItem);
        foreach (var item in throwAwayItem) { s.ThrowAway(item); }
        ProgressBar.UpdateFill(survivor!.APoints);
        UpdateItemSlots();
        UpdatePlayerStats();
        if (survivor.Name == s.Name && isSearch=="False")
            IncreaseUsedActions("Rearrange Items", s, null);
        int level = s.CanUpgradeTo();
        if (level > 0 && s==survivor)
        {
            if(s.FinishedRound) s.FinishedRound = false;
            isMenuOpen = true;
            TraitController.Instance.OpenMenu(level, survivor.GetTraitUpgrades(level), OnTraitSelected);
        }
        if (survivor.FinishedRound && survivor == gameModel!.CurrentPlayer)
        {
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.FinishedRound, s.Name);
        }
    }
    /// <summary>
    /// Executes the specified attack.
    /// </summary>
    /// <param name="data">Contains the specifications of the attack</param>
    public void ReceiveAttack(string data)
    {
        //tileid:isrighthand:ismelee:prioritylist(,):survivorname:results(,)
        string[] strings = data.Split(':');
        Survivor s = SurvivorFactory.GetSurvivorByName(strings[4]);
        Weapon? weapon = null;
        if (strings[1] == "True")
            weapon = (Weapon)s.RightHand!;
        else
            weapon=(Weapon)s.LeftHand!;
        List<int> throws=new List<int>();
        if (weapon.Type != WeaponType.BOMB)
        {
            foreach (var item in strings[5].Split(',').ToList())
                throws.Add(int.Parse(item));
        }
        else
        {
            Transform tile = GameObject.FindWithTag("MapPrefab").transform.Find($"SubTile_{strings[0]}");
            BoxCollider collider = tile.GetComponent<BoxCollider>();
            float startX = collider.transform.position.x - 0.8f;
            float startZ = collider.transform.position.z - 1f;
            float startY = 1.8f;
            Vector3 newPosition = new Vector3();
            newPosition.x = startX; newPosition.y = startY; newPosition.z = startZ;
            AnimationController.Instance.ShowPopupSkull(newPosition);
        }
        int zombieCount = gameModel!.GetZombiesInPriorityOrderOnTile(gameModel.Board.GetTileByID(int.Parse(strings[0]))).Count;
        s.Attack(gameModel.Board.GetTileByID(int.Parse(strings[0])), weapon, bool.Parse(strings[2]), throws, strings[3].Split(',').ToList());
        int newZombieCount = gameModel.GetZombiesInPriorityOrderOnTile(gameModel.Board.GetTileByID(int.Parse(strings[0]))).Count;
        if (newZombieCount < zombieCount)
        {
            Transform tile = GameObject.FindWithTag("MapPrefab").transform.Find($"SubTile_{strings[0]}");
            BoxCollider collider = tile.GetComponent<BoxCollider>();
            float startX = collider.transform.position.x - 0.8f;
            float startZ = collider.transform.position.z - 1f;
            float startY = 1.8f;
            Vector3 newPosition = new Vector3();
            newPosition.x = startX; newPosition.y = startY; newPosition.z = startZ;
            AnimationController.Instance.ShowPopupSkull(newPosition);
        }
        UpdateZombieCanvasOnTile(int.Parse(strings[0]));
        if (survivor!.Name == s.Name)
            IncreaseUsedActions("Attack", s, strings[2]);
        EnableBoardInteraction(survivor == gameModel.CurrentPlayer);
        ProgressBar.UpdateFill(survivor.APoints);
        UpdatePlayerStats();
        UpdateItemSlots();
        if (survivor.FinishedRound && survivor == gameModel.CurrentPlayer)
        {
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.FinishedRound, s.Name);
        }
    }
    /// <summary>
    /// Spawns the given zombies on the specified tiles.
    /// </summary>
    /// <param name="data">Contains the list of zombies</param>
    public void ReceiveZombieSpawns(string data)
    {
        if(gameModel!.GameOver) return;
        string[] strings = data.Split(";");
        List<(ZombieType, int, int, int, int, bool)> spawns = new List<(ZombieType, int, int, int, int, bool)>();
        foreach (string s in strings)
        {
            string[] spawnData = s.Split(",");
            ZombieType type = (ZombieType)Enum.Parse(typeof(ZombieType), spawnData[0], true);
            spawns.Add((type, int.Parse(spawnData[1]), int.Parse(spawnData[2]), int.Parse(spawnData[3]), int.Parse(spawnData[4]), bool.Parse(spawnData[5])));
        }
        gameModel.SpawnZombies(spawns);
        foreach (var item in gameModel.Board.Tiles)
        {
            UpdateZombieCanvasOnTile(item.Id);
        }

        gameModel.IsPlayerRoundOver = false;
        NewRoundForPlayers();
        gameModel.ShiftPlayerOrder();
        StartNextTurn();
    }
    /// <summary>
    /// Spawns the given zombies inside the specified building.
    /// </summary>
    /// <param name="data">Contains the list of zombies and the building</param>
    public void ReceiveZombieSpawnsInBuilding(string data)
    {
        string[] strings = data.Split(":");
        List<(ZombieType, int, int, int, int, bool)> spawns = new List<(ZombieType, int, int, int, int, bool)>();
        foreach (string s in strings[0].Split(';'))
        {
            string[] spawnData = s.Split(",");
            ZombieType type = (ZombieType)Enum.Parse(typeof(ZombieType), spawnData[0], true);
            spawns.Add((type, int.Parse(spawnData[1]), int.Parse(spawnData[2]), int.Parse(spawnData[3]), int.Parse(spawnData[4]), bool.Parse(spawnData[5])));
        }
        string[] rooms = strings[1].Split(";");
        for (int i = 0; i<rooms.Length;i++)
        {
            gameModel!.SpawnZombiesOnTile(spawns[i], gameModel.Board.GetTileByID(int.Parse(rooms[i])));
        }
        foreach (var item in gameModel!.Board.Tiles)
        {
            UpdateZombieCanvasOnTile(item.Id);
        }
    }
    /// <summary>
    /// Opens the inventory for given player with the recieved items or spawns walker zombies.
    /// </summary>
    /// <param name="data">Contains list of items and number of zombies that should be spawned</param>
    public void ReceiveSearch(string data)
    {
        string[] strings = data.Split(';');
        Survivor s = SurvivorFactory.GetSurvivorByName(strings[2]);
        s.SearchedAlready = true;
        int zombieAmount = int.Parse(strings[0]);
        List<string> items = strings[1].Split(",").ToList();
        if(zombieAmount > 0)
            gameModel!.SpawnZombie(ZombieType.WALKER, zombieAmount, s.CurrentTile, false);
        if (survivor == s)
        {
            List<Item> realItems = new List<Item>();
            foreach (var item in items)
            {
                if (item != string.Empty)
                    realItems.Add(ItemFactory.GetItemByName((ItemName)Enum.Parse(typeof(ItemName), item, true)));
            }
            if (realItems.Count > 0)
                OpenInventory(realItems,true);
            if (survivor.Name == s.Name)
                IncreaseUsedActions("Search", s, null);
        }
        UpdateZombieCanvasOnTile(s.CurrentTile.Id);
    }
    /// <summary>
    /// The given player picks up the specified pimp weapon.
    /// </summary>
    /// <param name="data">Contains the name of the player and name of the weapon</param>
    public void ReceivePimpWeapon(string data)
    {
        string[] strings=data.Split(':');
        PimpWeapon pimp = (PimpWeapon)ItemFactory.GetItemByName((ItemName)Enum.Parse(typeof(ItemName), strings[0].Replace("ItemName.", ""), true));
        Survivor s = SurvivorFactory.GetSurvivorByName(strings[1]);
        PickUpPimpWLocally(s,pimp);
    }
    /// <summary>
    /// Ends the turn for the given player and starts the next player's turn.
    /// </summary>
    /// <param name="data">Name of the player that finished</param>
    public void PlayerFinishedRound(string data)
    {
        SurvivorFactory.GetSurvivorByName(data).FinishedRound=true;//beállitani a használt képességeket falsera
        gameModel!.NextPlayer();
        if(gameModel.IsPlayerRoundOver)
        {
            ShowPlayerOrder();
            ZombieRound();
        }
        else
            StartNextTurn();
    }
    #endregion
    #region Game Methods
    /// <summary>
    /// Starts the next player's turn.
    /// </summary>
    private void StartNextTurn()
    {
        gameModel!.CurrentPlayer!.StartedRound = true;
        gameModel.CurrentPlayer.SetFreeActions();
        ShowPlayerOrder();
        if(gameModel.CurrentPlayer==survivor)
            UpdatePlayerStats();

        NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.TurnStart, GetClientIdByCharacter(gameModel.CurrentPlayer.Name).ToString());
    }
    /// <summary>
    /// Opens the given door with the given weapon.
    /// </summary>
    /// <param name="objectName">Name of the door object</param>
    /// <param name="weaponOption">Name of the weapon</param>
    /// <param name="s">Player that opens the door</param>
    private void OpenDoor(string objectName, string weaponOption, Survivor s)
    {
        GameObject? door = null;
        foreach (Transform child in GameObject.FindWithTag("MapPrefab").transform)
        {
            if (child.name == "Doors")
            {
                foreach (Transform subChild in child.transform)
                {
                    if (subChild.name == objectName)
                    {
                        door = subChild.gameObject; break;
                    }
                }
                break;
            }
        }
        TileConnection connection = s.CurrentTile.GetTileConnectionById(int.Parse(door!.gameObject.name.Substring(5).Split('_')[0]), int.Parse(door.gameObject.name.Substring(5).Split('_')[1]));
        if (weaponOption == "Right Hand")
            s.CurrentTile.OpenDoor(connection, (Weapon)s.RightHand!);
        else
            s.CurrentTile.OpenDoor(connection, (Weapon)s.LeftHand!);
        DestroyDoorWithTween(door);
        EnableDoors(false);
        isMenuOpen = false;
        EnableBoardInteraction(survivor == gameModel!.CurrentPlayer);
        if (NetworkManager.Singleton.IsHost)
        {
            if (!gameModel.BuildingOpened(connection))
                return;
            Building building = gameModel.Board.GetBuildingByTile(connection.Destination.Id);
            List<string> spawns = new List<string>();
            var darkRooms = building.Rooms.Where(x => x.Type == TileType.DARKROOM).Select(x=>x.Id).ToList();
            for (int i = 0; i < darkRooms.Count; i++)
            {
                (ZombieType, int, int, int, int, bool) option = gameModel.ChooseZombieSpawnOption();
                spawns.Add($"{option.Item1},{option.Item2},{option.Item3},{option.Item4},{option.Item5},{option.Item6}");
            }
            string data = $"{string.Join(';', spawns)}:{string.Join(';',darkRooms)}";
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.ZombieSpawnInBuilding, data);
        }
    }
    /// <summary>
    /// The given player deducts the used action.
    /// </summary>
    /// <param name="action">The action used</param>
    /// <param name="s">Player</param>
    /// <param name="isMelee">True - Melee attack, False - Range attack, null - not attack</param>
    private void IncreaseUsedActions(string action, Survivor s, string? isMelee)
    {
        gameModel!.CheckWin();
        s.OnUsedAction(action, isMelee);
        UpdatePlayerStats();
        int level = s.CanUpgradeTo();
        if (level > 0)
        {
            if(s.FinishedRound) s.FinishedRound = false;
            isMenuOpen = true;
            TraitController.Instance.OpenMenu(level, survivor!.GetTraitUpgrades(level), OnTraitSelected);
        }
    }
    /// <summary>
    /// The client executes the given action locally with the given player.
    /// </summary>
    /// <param name="playerID">ID of the player</param>
    /// <param name="actionName">Name of the action</param>
    /// <param name="objectName">Name of the tile object</param>
    public void ApplyActionLocally(ulong playerID, string actionName, string objectName)
    {
        Survivor s = SurvivorFactory.GetSurvivorByName(playerSelections[playerID]);
        switch (actionName)
        {
            case "Open Door Right Hand":
                OpenDoor(objectName, "Right Hand", s);
                if (survivor!.Name == s.Name)
                    IncreaseUsedActions("Open Door", s, null);
                EnableBoardInteraction(gameModel!.CurrentPlayer == survivor);
                break;
            case "Open Door Left Hand":
                OpenDoor(objectName, "Left Hand", s);
                if (survivor!.Name == s.Name)
                    IncreaseUsedActions("Open Door", s, null);
                EnableBoardInteraction(gameModel!.CurrentPlayer == survivor);
                break;
            case "Search":
                SearchOnTile(s);
                break;
            case "Skip":
                s.Skip();
                break;
            case "Move":
                s.Move(gameModel!.Board.GetTileByID(int.Parse(objectName.Substring(8))));
                MovePlayerToTile(int.Parse(objectName.Substring(8)), playerPrefabs[s.Name.Replace(" ",string.Empty)]);
                if (survivor!.Name == s.Name)
                    IncreaseUsedActions("Move", s, null);
                break;
            case "Slippery Move":
                s.SlipperyMove(gameModel!.Board.GetTileByID(int.Parse(objectName.Substring(8))));
                MovePlayerToTile(int.Parse(objectName.Substring(8)), playerPrefabs[s.Name.Replace(" ", string.Empty)]);
                if (survivor!.Name == s.Name)
                    IncreaseUsedActions("Slippery Move", s, null);
                break;
            case "Sprint Move":
                s.SprintMove(gameModel!.Board.GetTileByID(int.Parse(objectName.Substring(8))));
                MovePlayerToTile(int.Parse(objectName.Substring(8)), playerPrefabs[s.Name.Replace(" ", string.Empty)]);
                if (survivor!.Name == s.Name)
                    IncreaseUsedActions("Sprint Move", s, null);
                break;
            case "Jump":
                s.Jump(gameModel!.Board.GetTileByID(int.Parse(objectName.Substring(8))));
                MovePlayerToTile(int.Parse(objectName.Substring(8)), playerPrefabs[s.Name.Replace(" ", string.Empty)]);
                if (survivor!.Name == s.Name)
                    IncreaseUsedActions("Jump", s, null);
                break;
            case "Charge":
                s.Charge(gameModel!.Board.GetTileByID(int.Parse(objectName.Substring(8))));
                MovePlayerToTile(int.Parse(objectName.Substring(8)), playerPrefabs[s.Name.Replace(" ", string.Empty)]);
                if (survivor!.Name == s.Name)
                    IncreaseUsedActions("Charge", s, null);
                break;
            case "Pick Up Pimp Weapon":
                PickUpPimpW(s);
                if (survivor!.Name == s.Name)
                    IncreaseUsedActions("Pick Up Pimp Weapon", s, null);
                break;
            case "Pick Up Objective":
                PickUpObjective(s);
                if (survivor!.Name == s.Name)
                    IncreaseUsedActions("Pick Up Objective", s, null);
                break;
            case "Leave Through Exit":
                s.LeaveThroughExit();
                RemovePlayer(s.Name);
                break;
            default:
                Debug.LogWarning("Unknown action: " + actionName);
                break;
        }
        if (survivor == s)
            UpdatePlayerStats();
        if (survivor!.FinishedRound && survivor==gameModel!.CurrentPlayer)
        {
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.FinishedRound, s.Name);
        }
    }
    /// <summary>
    /// Initiates the appropriate dice roll for the given attack.
    /// </summary>
    /// <param name="objectName">Name of the tile object</param>
    /// <param name="isRightHand">True - player used right hand for the attack, False - player used left hand for the attack</param>
    public void StartAttack(string objectName, bool isRightHand)
    {
        bool isMelee = AttackFlag == "Melee";
        Weapon? weapon = null;
        if (isRightHand)
            weapon = (Weapon)survivor!.RightHand!;
        else
            weapon = (Weapon)survivor!.LeftHand!;

        string data = $"{objectName.Substring(8)}:{isRightHand}:{isMelee}";

        if (weapon.Type == WeaponType.BOMB)
        {
            data += $"::{survivor.Name}";
            OnOkRollDiceClicked(data, new List<int>());
            return;
        }

        if (isMelee)
        {
            OpenPriorityMenu(data);
            return;
        }
        else if((!isMelee && survivor.Traits.Contains(Trait.SNIPER))||((!isMelee)&& (weapon.Name==ItemName.SNIPERRIFLE || weapon.Name==ItemName.MILITARYSNIPERRIFLE)))
        {
            OpenPriorityMenu(data);
            return;
        }
        else 
        { 
            data += $"::{survivor.Name}";
            RollDice(data);
        }
    }
    /// <summary>
    /// Performs the dice roll.
    /// </summary>
    /// <param name="data">Contains the name of the player who attacked and the name of the weapon used</param>
    private void RollDice(string data)
    {
        string[] strings = data.Split(':');
        Survivor s = SurvivorFactory.GetSurvivorByName(strings[4]);
        bool isDual=s.LeftHand !=null && s.RightHand != null&&s.LeftHand.Name==s.RightHand.Name;
        Weapon? weapon = null;
        if (strings[1] == "True")
            weapon = (Weapon)s.RightHand!;
        else
            weapon = (Weapon)s.LeftHand!;
        int diceAmount=weapon.DiceAmount;

        if (s.Traits.Contains(Trait.AMBIDEXTROUS) || (weapon.IsDual && isDual))
            diceAmount *= 2;

        if (strings[2]=="True" && weapon.Name==ItemName.GUNBLADE)
            diceAmount *=2;
        else if(strings[2] == "True" && weapon.Name == ItemName.MASSHOTGUN)
            diceAmount -=1;

        // Dice trait
        if (s.Traits.Contains(Trait.P1DC))
            diceAmount++;
        if (s.Traits.Contains(Trait.P1DM) && strings[2]=="True")
            diceAmount++;
        if (s.Traits.Contains(Trait.P1DR) && strings[2] == "False")
            diceAmount++;

        isMenuOpen = true;
        diceRoller!.RollDice(diceAmount);
        StartCoroutine(diceRoller.WaitForDiceToStop((results) => {
            OnDiceResultsReady(data, results);
        }));
    }
    /// <summary>
    /// The host performs the search locally and then sends the result to the clients.
    /// </summary>
    /// <param name="s">Player who searched</param>
    private void SearchOnTile(Survivor s)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            List<Item>? items = gameModel!.Search(s.CurrentTile, s.HasFlashLight(), s.Traits.Contains(Trait.MATCHINGSET));
            //zombieamount;items;survivor
            string data = string.Empty;
            if(s.HasFlashLight())
            {
                if (items!.Count == 0)
                    data = "2;;"+s.Name;
                else if(items.Count == 3)
                    data = $"0;{string.Join(',', items.Select(x => x.Name))};{s.Name}";
                else
                    data = $"{2 - items.Count};{string.Join(',', items.Select(x => x.Name))};{s.Name}";
            }
            else
            {
                if (items == null)
                    data = "1;;" + s.Name;
                else
                    data = $"0;{string.Join(',', items.Select(x => x.Name))};{s.Name}";
            }
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.Search, data);
        }
        AnimationController.Instance.ShowPopupSearch(playerPrefabs[s.Name.Replace(" ", string.Empty)].transform.position);
    }
    /// <summary>
    /// The host picks up the objective locally and then sends it to the clients.
    /// </summary>
    /// <param name="s">Player who picked the objective</param>
    private void PickUpObjective(Survivor s)
    {
        GameObject? gObject = null;
        foreach (Transform child in GameObject.FindWithTag("MapPrefab").transform)
        {
            if (child.name.StartsWith("Obj") && int.Parse(child.name.Substring(10)) == s.CurrentTile.Id)
            {
                gObject=child.gameObject; break;
            }
        }
        s.PickUpObjective();
        DestroyWithTween(gObject!);
        ProgressBar.UpdateFill(survivor!.APoints);
    }
    /// <summary>
    /// The clients delete the pimp weapon that got picked up.
    /// </summary>
    /// <param name="s">Player who picked up the pimp weapon</param>
    /// <param name="pimp">Pimp weapon picked up</param>
    private void PickUpPimpWLocally(Survivor s, PimpWeapon pimp)
    {
        GameObject? gObject = null;
        foreach (Transform child in GameObject.FindWithTag("MapPrefab").transform)
        {
            if (child.name.StartsWith("Pimp") && int.Parse(child.name.Substring(6)) == s.CurrentTile.Id)
            {
                gObject = child.gameObject; break;
            }
        }
        s.CurrentTile.PickUpPimpWeapon();
        DestroyWithTween(gObject!);
        if (survivor == s)
        {
            OpenInventory(new List<Item>() { pimp },true);
        }
    }
    /// <summary>
    /// The host picks up the pimp weapon and then sends it to the clients.
    /// </summary>
    /// <param name="s">Player who picked up the pimp weapon</param>
    private void PickUpPimpW(Survivor s)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            var pimp = s.CurrentTile.PickUpPimpWeapon();
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.PimpWeapon, $"{pimp.Name.ToString()}:{s.Name}");
        }
    }
    /// <summary>
    /// Resets the players for the beginning of the new turn.
    /// </summary>
    private void NewRoundForPlayers()
    {
        foreach(var s in playerSelections.Values)
        {
            if(!SurvivorFactory.GetSurvivorByName(s).IsDead)
                SurvivorFactory.GetSurvivorByName(s).NewRound();
        }
    }
    /// <summary>
    /// Executes the zombies' turn.
    /// </summary>
    private void ZombieRound()
    {
        gameModel!.EndRound();
        if (gameModel.GameOver)
            return;
        if(NetworkManager.Singleton.IsHost)
        {
            List<string> spawns = new List<string>();
            for (int i = 0; i < gameModel.SpawnCount; i++)
            {
                (ZombieType, int, int, int, int, bool) option = gameModel.ChooseZombieSpawnOption();
                spawns.Add($"{option.Item1},{option.Item2},{option.Item3},{option.Item4},{option.Item5},{option.Item6}");
            }
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.ZombieSpawn, string.Join(';',spawns));
        }
        UpdatePlayerStats();
    }
    #endregion
    #region Event Handlers
    /// <summary>
    /// Event handler for the player's death.
    /// </summary>
    private void GameController_SurvivorDied(object sender, string e)
    {
        if (isSurvivorDead) return;
        SurvivorFactory.GetSurvivorByName(e).SurvivorDied -= GameController_SurvivorDied;
        if (e == survivor!.Name)
        {
            isSurvivorDead = true;
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.SurvivorDied, e);
        }
    }
    /// <summary>
    /// Event handler for the end of the game.
    /// </summary>
    private void GameModel_GameEnded(object sender, bool e)
    {
        EnableBoardInteraction(false);
        GameObject ui = GameObject.FindWithTag("GameUI");
        GameObject endPanelPrefab = Resources.Load<GameObject>("Prefabs/EndPanel");
        GameObject endPanel=Instantiate(endPanelPrefab,ui.transform);
        isMenuOpen = true;
        var innerImg=endPanel.transform.GetChild(0).GetComponent<Image>();
        if(e)
            innerImg.sprite= Resources.Load<Sprite>("Menu/survivors_won");
        else
            innerImg.sprite = Resources.Load<Sprite>("Menu/zombies_won");
        var okButton=endPanel.transform.GetChild(1).GetComponent<Button>();
        okButton.onClick.AddListener(() => OnOkEndGameClicked());
    }
    /// <summary>
    /// Event handler for in-game scene loading.
    /// </summary>
    private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName == "InGameScene")
        {
            selectedMapID = NetworkManagerController.Instance.SelectedMapID;
            NetworkManagerController.Instance.SendPlayerSelectionsServerRpc();
        }
    }
    /// <summary>
    /// Event handler for the completion of dice roll results.
    /// </summary>
    private void OnDiceResultsReady(string data, List<int> results)
    {
        string[] strings = data.Split(':');
        Survivor s = SurvivorFactory.GetSurvivorByName(strings[4]);
        Weapon? weapon = null;
        if (strings[1] == "True")
            weapon = (Weapon)s.RightHand!;
        else
            weapon = (Weapon)s.LeftHand!;
        bool isLucky = s.Traits.Contains(Trait.LUCKY) || (s.HasPlentyOfBullets()&&weapon.BulletType=='B')|| (s.HasPlentyOfShells() && weapon.BulletType == 'S');
        if (isLucky)
        {
            diceRoller!.ReRollDice(weapon.Accuracy, results);
            StartCoroutine(diceRoller.WaitForDiceToStop((newResults) => {
                diceRoller.RollFinished(data, newResults);
            }));
        }
        else
        {
            diceRoller!.RollFinished(data, results);
        }
    }
    /// <summary>
    /// Event handler for the player confirming the dice rolls.
    /// </summary>
    public void OnOkRollDiceClicked(string data, List<int> results)
    {
        isMenuOpen = false;
        NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.Attack, data += $":{string.Join(',', results)}");
    }
    /// <summary>
    /// Event handler for the player confirming the priority window.
    /// </summary>
    private void OnOkPriorityMenuClicked(string data)
    {
        List<string> newPriority = new List<string>();
        if (sniperMenuForS != null)
        {
            foreach (Transform child in sniperMenuForS.transform)
            {
                foreach (Transform subChild in child.transform)
                {
                    if (subChild.transform.childCount > 0)
                    {
                        Transform item = subChild.transform.GetChild(0);
                        if (child.name.StartsWith("Priority"))
                            newPriority.Add(item.GetComponent<Image>().sprite.name.TrimEnd("Slot"));
                    }
                }
            }
        }
        if (newPriority.Count == 0) return;
        cameraDrag!.SetActive(true);
        Destroy(sniperMenuForS);

        data += $":{string.Join(',', newPriority)}:{survivor!.Name}";
        isMenuOpen = false;
        RollDice(data);
    }
    /// <summary>
    /// Event handler for the player confirming the inventory window.
    /// </summary>
    private void OnOkInventoryClicked(bool isSearch)
    {
        cameraDrag!.SetActive(true);
        string leftH = string.Empty;string rightH = string.Empty;
        List<string> backP = new List<string>();
        List<string> throwAway = new List<string>();
        if (inventoryForS!=null)
        {
            foreach (Transform child in inventoryForS.transform)
            {
                foreach (Transform subChild in child.transform)
                {
                    if (subChild.transform.childCount > 0)
                    {
                        Transform item = subChild.transform.GetChild(0);
                        if (child.name.StartsWith("Hand")&& subChild.name.Substring(14)=="Left")
                            leftH= item.GetComponent<Image>().sprite.name;
                        if (child.name.StartsWith("Hand") && subChild.name.Substring(14) == "Right")
                            rightH = item.GetComponent<Image>().sprite.name;
                        if (child.name.StartsWith("Back"))
                            backP.Add(item.GetComponent<Image>().sprite.name);
                        if (child.name.StartsWith("Throw"))
                            throwAway.Add(item.GetComponent<Image>().sprite.name);
                    } 
                }
            }
        }
        Destroy(inventoryForS);
        string data = $"{leftH};{rightH};{string.Join(',', backP)};{string.Join(',', throwAway)};{isSearch}:{survivor!.Name}";
        NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.ItemsChanged, data);
        isMenuOpen = false;
        EnableBoardInteraction(survivor == gameModel!.CurrentPlayer);
    }
    /// <summary>
    /// Event handler for the player selecting the trait.
    /// </summary>
    private void OnTraitSelected(int level, int option)
    {
        isMenuOpen=false;
        NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.TraitUpgrade, $"{survivor!.Name};{level};{option}");
    }
    /// <summary>
    /// Event handler for the player confirming the game over panel.
    /// </summary>
    public void OnOkEndGameClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            // Ha a host nyomja meg, mindenki visszakerül a főmenübe
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.GameEnded, "");
        }
        else
        {
            // Ha kliens nyomja meg, csak ő lép ki
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.PlayerLeft, survivor!.Name);
            if(gameModel!.CurrentPlayer==survivor)
                NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.FinishedRound, survivor.Name);

            // Kliens kilépése a főmenübe
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("MenuScene");
        }
    }
    #endregion
    #endregion
}
