using Model;
using Model.Board;
using Model.Characters.Survivors;
using Model.Characters.Zombies;
using Network;
using Persistence;
using System;
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

public class GameController : MonoBehaviour
{
    private GameModel gameModel;
    private Survivor survivor;
    private GameObject MapPrefab;
    [SerializeField] private GameObject cameraDrag;
    [SerializeField] private DiceRoller diceRoller;
    [SerializeField] private GameObject rightHand;
    [SerializeField] private GameObject leftHand;
    [SerializeField] private List<GameObject> backPack=new List<GameObject>();
    private GameObject APointsText;
    private GameObject HealthText;
    private int selectedMapID;
    private Dictionary<ulong, string> playerSelections;
    private Dictionary<string, GameObject> playerPrefabs=new Dictionary<string, GameObject>();
    private GameObject charImagePrefab;
    private HorizontalLayoutGroup charListContainer;
    private GameObject? inventoryForS;
    private GameObject? sniperMenuForS;
    private bool isInventoryOpen=false;
    private Dictionary<int, GameObject> zombieCanvases = new Dictionary<int, GameObject>();
    public string AttackFlag {  get; set; }
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
        gameModel.StartGame(playerSelections.Values.ToList(),mapID);
        GenerateBoard(mapID);
        GeneratePlayersOnBoard();
        survivor = gameModel.GetSurvivorByName(playerSelections[NetworkManager.Singleton.LocalClientId]);
        survivor.SetReference(gameModel);
        if (NetworkManager.Singleton.IsHost)
        {
            gameModel.DecidePlayerOrder();

            // Küldjük a sorrendet a klienseknek
            string serializedOrder = string.Join(',', gameModel.PlayerOrder.Select(x => x.Name));
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.PlayerOrder, serializedOrder);

            // Kezdő fegyverek küldése 
            List<Weapon> genericW=gameModel.GenerateGenericWeapons();
            string serializedGenericW = string.Join(",", genericW.Select(x => x.Name));
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.GenericWeapon, serializedGenericW);

            StartNextTurn();
        }
        ShowPlayerUI();
    }
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
                        HealthText = subChild.gameObject;
                    else if (subChild.name == "Points")
                        APointsText = subChild.gameObject;
                }
            }
        }
    }
    private void GeneratePlayersOnBoard()
    {
        Transform tile = GameObject.FindWithTag("MapPrefab").transform.Find($"SubTile_{gameModel.StartTile.Id}");
        BoxCollider collider = tile.GetComponent<BoxCollider>();
        float startX = collider.transform.position.x - 1.5f;
        float startZ = collider.transform.position.z + 0.5f;
        float startY = 1.631f;
        Vector3 newPosition = new Vector3();
        newPosition.x = startX; newPosition.y= startY; newPosition.z = startZ;
        int multiply = 1;

        foreach (var item in playerSelections.Values)
        {
            GameObject playerPrefab = Resources.Load<GameObject>($"Prefabs/Players/{item.Replace(" ",string.Empty)}");
            GameObject player=Instantiate(playerPrefab);
            playerPrefabs.Add(item.Replace(" ", string.Empty), player);
            player.transform.position = newPosition;
            if (multiply < 4)
            {
                newPosition.x = startX + (multiply * 1.5f);
                newPosition.z = startZ;
            }
            else
            {
                newPosition.x = startX;
                newPosition.z = startZ + ((multiply - 3) * 0.7f);
            }
            multiply++;
        }
    }
    private void MovePlayerToTile(int tileID, GameObject player)
    {
        Transform tile = GameObject.FindWithTag("MapPrefab").transform.Find($"SubTile_{tileID}");
        BoxCollider collider = tile.GetComponent<BoxCollider>();
        float startX=collider.transform.position.x-1.5f;
        float startZ=collider.transform.position.z + 0.5f;
        float startY = 1.631f; //ez mindig marad
        int playerCount = gameModel.NumberOfPlayersOnTile(tileID);
        Vector3 newPosition = player.transform.position;

        if (playerCount < 4)
        {
            newPosition.x = startX + (playerCount * 1.5f);
            newPosition.z = startZ;
        }
        else
        {
            newPosition.x = startX;
            newPosition.z = startZ + ((playerCount - 3) * 0.7f);
        }
        newPosition.y = startY;
        player.transform.position = newPosition;
    }
    private void UpdateZombieCanvasOnTile(int tileID)
    {
        if(zombieCanvases.ContainsKey(tileID))
        {
            Destroy(zombieCanvases[tileID]);
            zombieCanvases.Remove(tileID);
        }
        List<Zombie> zombies = gameModel.GetZombiesInPriorityOrderOnTile(gameModel.Board.GetTileByID(tileID));
        if(zombies!=null && zombies.Count > 0)
        {
            GameObject zombieCanvasPrefab = Resources.Load<GameObject>("Prefabs/ZombieCanvas");
            Transform tile = GameObject.FindWithTag("MapPrefab").transform.Find($"SubTile_{tileID}");
            BoxCollider collider = tile.GetComponent<BoxCollider>();
            float startX = collider.transform.position.x - 0.8f;
            float startZ = collider.transform.position.z+0.5f;
            float startY = 2f;
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
    private void StartNextTurn()
    {
        gameModel.CurrentPlayer.StartedRound = true;
        gameModel.CurrentPlayer.SetFreeActions();
        ShowPlayerOrder();
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
        GameObject zombieEntry = Instantiate(charImagePrefab, charListContainer.transform);
        zombieEntry.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Characters/zombie_head");
        Outline zoutline = zombieEntry.GetComponent<Outline>();
        zoutline.enabled = gameModel.IsPlayerRoundOver;
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
    public void ReceivePlayerOrder(string serializedOrder)
    {
        List<string> orderedPlayers = serializedOrder.Split(',').ToList();
        gameModel.SetPlayerOrder(orderedPlayers);
        ShowPlayerOrder();
    }
    public void SetPlayerSelections(Dictionary<ulong, string> selections)
    {
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
    public void UpdatePlayerStats()
    {
        HealthText.GetComponent<TMP_Text>().text = survivor.HP.ToString();
        APointsText.GetComponent<TMP_Text>().text=survivor.APoints.ToString();
    }
    private void UpdateBoardForActivePlayer(ulong playerID)
    {
        // Itt kapcsoljuk ki/be az interakciót
        bool isActive = playerID == NetworkManager.Singleton.LocalClientId;
        EnableBoardInteraction(isActive);
    }
    public void EnableBoardInteraction(bool enable)
    {
        if (enable && isInventoryOpen) return; 
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
    public void EnableDoors(bool enable)
    {
        EnableBoardInteraction(false);
        foreach (Transform child in GameObject.FindWithTag("MapPrefab").transform)
        {
            if (child.name=="Doors")
            {
                foreach (Transform subChild in child.transform)
                {
                    if (int.Parse(subChild.name.Substring(5).Split('_')[0])==survivor.CurrentTile.Id || int.Parse(subChild.name.Substring(5).Split('_')[1]) == survivor.CurrentTile.Id)
                    {
                        var collider = subChild.GetComponent<BoxCollider>();
                        if (collider != null)
                            collider.enabled = enable;
                    }
                }
            }
        }
    }
    public void OpenDoor(string objectName, string weaponOption, Survivor s)
    {
        GameObject door = null;
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
        TileConnection connection = s.CurrentTile.GetTileConnectionById(int.Parse(door.gameObject.name.Substring(5).Split('_')[0]), int.Parse(door.gameObject.name.Substring(5).Split('_')[1]));
        if (weaponOption == "Right Hand")
            s.CurrentTile.OpenDoor(connection, (Weapon)s.RightHand);
        else
            s.CurrentTile.OpenDoor(connection, (Weapon)s.LeftHand);
        Destroy(door);
        EnableDoors(false);
        EnableBoardInteraction(survivor == gameModel.CurrentPlayer);
        if (NetworkManager.Singleton.IsHost)
        {
            if (!gameModel.BuildingOpened(connection))
                return;
            Building building = gameModel.Board.GetBuildingByTile(connection.Destination.Id);
            List<string> spawns = new List<string>();
            var darkRooms = building.Rooms.Where(x => x.Type == TileType.DARKROOM).Select(x=>x.Id).ToList();
            for (int i = 0; i < darkRooms.Count; i++)
            {
                (ZombieType, int, int, int, int) option = gameModel.ChooseZombieSpawnOption();
                spawns.Add($"{option.Item1},{option.Item2},{option.Item3},{option.Item4},{option.Item5}");
            }
            string data = $"{string.Join(';', spawns)}:{string.Join(';',darkRooms)}";
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.ZombieSpawnInBuilding, data);
        }
    }
    public void IncreaseUsedActions(string action, Survivor s, string? isMelee)
    {
        s.OnUsedAction(action, isMelee);
        UpdatePlayerStats();
        int level = s.CanUpgradeTo();
        if (level > 0)
        {
            TraitController.Instance.OpenMenu(level, survivor.GetTraitUpgrades(level), OnTraitSelected);
        }
    }
    private void OnTraitSelected(int level, int option)
    {
        NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.TraitUpgrade, $"{survivor.Name};{level};{option}");
    }
    public void ReceiveGenericWeapons(string data)
    {
        List<ItemName> weapons=data.Split(',').Select(x=>(ItemName)Enum.Parse(typeof(ItemName), x.Replace("ItemName.",""),true)).ToList();
        gameModel.GiveSurvivorsGenericWeapon(weapons);
        UpdateItemSlots();
    }
    private void UpdateItemSlots()
    {
        if (survivor.LeftHand == null)
            leftHand.GetComponent<Image>().sprite= Resources.Load<Sprite>("Objects/card_lefthand");
        else
            leftHand.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{survivor.LeftHand.Name.ToString().ToLower()}");
        if (survivor.RightHand == null)
            rightHand.GetComponent<Image>().sprite = Resources.Load<Sprite>("Objects/card_righthand");
        else
            rightHand.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{survivor.RightHand.Name.ToString().ToLower()}");
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
    public List<string> GetAvailableActionsOnTile(string tileName)
    {
        SurvivorFactory.GetSurvivorByName(playerSelections[NetworkManager.Singleton.LocalClientId]).SetActions(gameModel.Board.GetTileByID(int.Parse(tileName.Substring(8))));
        return SurvivorFactory.GetSurvivorByName(playerSelections[NetworkManager.Singleton.LocalClientId]).Actions.Keys.ToList();
    }
    public List<string> GetAvailableDoorOpeners()
    {
        List<string> result = new List<string>();
        if (survivor.RightHand != null && survivor.RightHand is Weapon weapon && weapon.CanOpenDoors) result.Add("Right Hand");
        if (survivor.LeftHand != null && survivor.LeftHand is Weapon weapon2 && weapon2.CanOpenDoors) result.Add("Left Hand");
        return result;
    }
    public List<string> GetAvailableAttacks(string tileID)
    {
        List<string> options = survivor.GetAvailableAttacksOnTile(gameModel.Board.GetTileByID(int.Parse(tileID)));
        if (options.Contains("Melee") && survivor.CurrentTile.Id != int.Parse(tileID))
            options.Remove("Melee");
        return options;
    }
    public List<string> GetAvailableWeapons(bool isMelee, string tileID)
    {
        List<string> result = new List<string>();
        if (isMelee)
        {
            if(survivor.LeftHand != null && survivor.LeftHand is Weapon weapon && weapon.CanBeUsedAsMelee)
                result.Add("Left Hand");
            if (survivor.RightHand != null && survivor.RightHand is Weapon weapon2 && weapon2.CanBeUsedAsMelee)
                result.Add("Right Hand");
        }
        else
        {
            int distance = 1;
            if (survivor.CurrentTile.Type == TileType.STREET && gameModel.Board.GetTileByID(int.Parse(tileID)).Type==TileType.STREET)
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
    public void StartAttack(string objectName, bool isRightHand)
    {
        bool isMelee = AttackFlag == "Melee";
        Weapon weapon = null;
        if (isRightHand)
            weapon = (Weapon)survivor.RightHand;
        else
            weapon = (Weapon)survivor.LeftHand;

        string data = $"{objectName.Substring(8)}:{isRightHand}:{isMelee}";

        if (isMelee)
        {
            OpenSniperMenu(data);
            return;
        }
        else if((!isMelee && survivor.Traits.Contains(Trait.SNIPER))||((!isMelee)&& (weapon.Name==ItemName.SNIPERRIFLE || weapon.Name==ItemName.MILITARYSNIPERRIFLE)))
        {
            OpenSniperMenu(data);
            return;
        }
        else 
        { 
            data += $"::{survivor.Name}";
            RollDice(data);
        }
    }
    public void RollDice(string data)
    {
        string[] strings = data.Split(':');
        Survivor s = SurvivorFactory.GetSurvivorByName(strings[4]);
        Weapon weapon = null;
        if (strings[1] == "True")
            weapon = (Weapon)s.RightHand;
        else
            weapon = (Weapon)s.LeftHand;
        int diceAmount=weapon.DiceAmount;

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

        diceRoller.RollDice(diceAmount);
        StartCoroutine(diceRoller.WaitForDiceToStop((results) => {
            OnDiceResultsReady(data, results);
        }));
    }
    private void OnDiceResultsReady(string data, List<int> results)
    {
        string[] strings = data.Split(':');
        Survivor s = SurvivorFactory.GetSurvivorByName(strings[4]);
        Weapon weapon = null;
        if (strings[1] == "True")
            weapon = (Weapon)s.RightHand;
        else
            weapon = (Weapon)s.LeftHand;
        bool isLucky = s.Traits.Contains(Trait.LUCKY) || (s.HasPlentyOfBullets()&&weapon.BulletType=='B')|| (s.HasPlentyOfShells() && weapon.BulletType == 'S');
        if (isLucky)
        {
            diceRoller.ReRollDice(weapon.Accuracy, results);
            StartCoroutine(diceRoller.WaitForDiceToStop((newResults) => {
                diceRoller.RollFinished(data, newResults);
            }));
        }
        else
        {
            diceRoller.RollFinished(data, results);
        }
    }
    public void OnOkRollDiceClicked(string data, List<int> results)
    {
        NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.Attack, data += $":{string.Join(',', results)}");
    }
    public void ApplyActionLocally(ulong playerID, string actionName, string objectName)
    {
        Survivor s = SurvivorFactory.GetSurvivorByName(playerSelections[playerID]);
        switch (actionName)
        {
            case "Open Door Right Hand":
                OpenDoor(objectName, "Right Hand", s);
                if (survivor.Name == s.Name)
                    IncreaseUsedActions("Open Door", s, null);
                EnableBoardInteraction(gameModel.CurrentPlayer == survivor);
                break;
            case "Open Door Left Hand":
                OpenDoor(objectName, "Left Hand", s);
                if (survivor.Name == s.Name)
                    IncreaseUsedActions("Open Door", s, null);
                EnableBoardInteraction(gameModel.CurrentPlayer == survivor);
                break;
            case "Search":
                SearchOnTile(s);
                if (survivor.Name == s.Name)
                    IncreaseUsedActions("Search", s, null);
                break;
            case "Skip":
                s.Skip();
                break;
            case "Move":
                s.Move(gameModel.Board.GetTileByID(int.Parse(objectName.Substring(8))));
                MovePlayerToTile(int.Parse(objectName.Substring(8)), playerPrefabs[s.Name.Replace(" ",string.Empty)]);
                if (survivor.Name == s.Name)
                    IncreaseUsedActions("Move", s, null);
                break;
            case "Slippery Move":
                s.SlipperyMove(gameModel.Board.GetTileByID(int.Parse(objectName.Substring(8))));
                MovePlayerToTile(int.Parse(objectName.Substring(8)), playerPrefabs[s.Name.Replace(" ", string.Empty)]);
                if (survivor.Name == s.Name)
                    IncreaseUsedActions("Slippery Move", s, null);
                break;
            case "Pick Up Pimp Weapon":
                PickUpPimpW(s);
                if (survivor.Name == s.Name)
                    IncreaseUsedActions("Pick Up Pimp Weapon", s, null);
                break;
            case "Pick Up Objective":
                PickUpObjective(s);
                if (survivor.Name == s.Name)
                    IncreaseUsedActions("Pick Up Objective", s, null);
                break;
            default:
                Debug.LogWarning("Unknown action: " + actionName);
                break;
        }
        if (survivor == s)
            UpdatePlayerStats();
        Debug.Log(s.Name + " used actions: " + s.UsedAction + " finished: " + s.FinishedRound + " currenttile: " + s.CurrentTile.Id);
        Debug.Log(survivor.Name + " used actions: " + survivor.UsedAction + " finished: " + survivor.FinishedRound + " currenttile: " + survivor.CurrentTile.Id);
        if (survivor.FinishedRound && survivor==gameModel.CurrentPlayer)
        {
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.FinishedRound, s.Name);
        }
    }
    public void SearchOnTile(Survivor s)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            List<Item> items = gameModel.Search(s.CurrentTile, s.HasFlashLight());
            //zombieamount;items;survivor
            string data = string.Empty;
            if(s.HasFlashLight())
            {
                if (items.Count == 0)
                    data = "2;;"+s.Name;
                else
                    data = $"{2-items.Count};{string.Join(',', items.Select(x => x.Name))};{s.Name}";
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
    }
    public void OpenSniperMenu(string data)
    {
        List<Zombie> zombies = gameModel.GetZombiesInPriorityOrderOnTile(gameModel.Board.GetTileByID(int.Parse(data.Split(':')[0])));
        isInventoryOpen = true;
        cameraDrag.SetActive(false);
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
                        if (zombieTypeList[i].StartsWith("Hobo")|| zombieTypeList[i].StartsWith("Abo"))
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
                    OnOkSniperMenuClicked(data);
                });
            }
        }
        sniperMenuForS = sniper;
    }
    public void OnOkSniperMenuClicked(string data)
    {
        cameraDrag.SetActive(true);
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
        Destroy(sniperMenuForS);

        data += $":{string.Join(',', newPriority)}:{survivor.Name}";
        isInventoryOpen = false;
        RollDice(data);
    }
    public void OpenInventory(List<Item>? additionalItems)
    {
        isInventoryOpen = true;
        cameraDrag.SetActive(false);
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
                    if (subChild.name.Substring(14) == "Left" && survivor.LeftHand != null)
                    {
                        GameObject itemPrefab = Resources.Load<GameObject>($"Prefabs/Inventory/Item");
                        GameObject item = Instantiate(itemPrefab, subChild.transform);
                        item.GetComponent<Image>().sprite= Resources.Load<Sprite>($"Items/{survivor.LeftHand.Name.ToString().ToLower()}");
                    }
                    else if(subChild.name.Substring(14) == "Right" && survivor.RightHand != null)
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
                    if (i >= survivor.BackPack.Count)
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
                    OnOkInventoryClicked();
                });
            }
        }
        inventoryForS=inventory;
    }
    public void OnOkInventoryClicked()
    {
        cameraDrag.SetActive(true);
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
        string data = $"{leftH};{rightH};{string.Join(',', backP)};{string.Join(',', throwAway)}:{survivor.Name}";
        NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.ItemsChanged, data);
        isInventoryOpen = false;
        EnableBoardInteraction(survivor == gameModel.CurrentPlayer);
    }
    public void ReceiveTraitUpgrade(string data)
    {
        string[] strings = data.Split(';');
        Survivor s = SurvivorFactory.GetSurvivorByName(strings[0]);
        s.UpgradeTo(int.Parse(strings[1]), int.Parse(strings[2]));
    }
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
        ProgressBar.UpdateFill(survivor.APoints);
        UpdateItemSlots();
        UpdatePlayerStats();
        int level = s.CanUpgradeTo();
        if (level > 0 && s==survivor)
        {
            TraitController.Instance.OpenMenu(level, survivor.GetTraitUpgrades(level), OnTraitSelected);
        }
        if (survivor.FinishedRound && survivor == gameModel.CurrentPlayer)
        {
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.FinishedRound, s.Name);
        }
    }
    public void ReceiveAttack(string data)
    {
        //tileid:isrighthand:ismelee:prioritylist(,):survivorname:results(,)
        string[] strings = data.Split(':');
        Survivor s = SurvivorFactory.GetSurvivorByName(strings[4]);
        Weapon weapon = null;
        if (strings[1] == "True")
            weapon = (Weapon)s.RightHand;
        else
            weapon=(Weapon)s.LeftHand;
        List<int> throws=new List<int>();
        foreach (var item in strings[5].Split(',').ToList())
            throws.Add(int.Parse(item));
        s.Attack(gameModel.Board.GetTileByID(int.Parse(strings[0])), weapon, bool.Parse(strings[2]), throws, strings[3].Split(',').ToList());
        UpdateZombieCanvasOnTile(int.Parse(strings[0]));
        if (survivor.Name == s.Name)
            IncreaseUsedActions("Attack", s, strings[2]);
        EnableBoardInteraction(survivor == gameModel.CurrentPlayer);
        ProgressBar.UpdateFill(survivor.APoints);
        if (survivor.FinishedRound && survivor == gameModel.CurrentPlayer)
        {
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.FinishedRound, s.Name);
        }
    }
    public void ReceiveZombieSpawns(string data)
    {
        string[] strings = data.Split(";");
        List<(ZombieType, int, int, int, int)> spawns = new List<(ZombieType, int, int, int, int)>();
        foreach (string s in strings)
        {
            string[] spawnData = s.Split(",");
            ZombieType type = (ZombieType)Enum.Parse(typeof(ZombieType), spawnData[0], true);
            spawns.Add((type, int.Parse(spawnData[1]), int.Parse(spawnData[2]), int.Parse(spawnData[3]), int.Parse(spawnData[4])));
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
    public void ReceiveZombieSpawnsInBuilding(string data)
    {
        string[] strings = data.Split(":");
        List<(ZombieType, int, int, int, int)> spawns = new List<(ZombieType, int, int, int, int)>();
        foreach (string s in strings[0].Split(';'))
        {
            string[] spawnData = s.Split(",");
            ZombieType type = (ZombieType)Enum.Parse(typeof(ZombieType), spawnData[0], true);
            spawns.Add((type, int.Parse(spawnData[1]), int.Parse(spawnData[2]), int.Parse(spawnData[3]), int.Parse(spawnData[4])));
        }
        string[] rooms = strings[1].Split(";");
        for (int i = 0; i<rooms.Length;i++)
        {
            gameModel.SpawnZombiesOnTile(spawns[i], gameModel.Board.GetTileByID(int.Parse(rooms[i])));
            UpdateZombieCanvasOnTile(int.Parse(rooms[i]));
        }
    }
    public void ReceiveSearch(string data)
    {
        string[] strings = data.Split(';');
        Survivor s = SurvivorFactory.GetSurvivorByName(strings[2]);
        s.SearchedAlready = true;
        int zombieAmount = int.Parse(strings[0]);
        List<string> items = strings[1].Split(",").ToList();
        if(zombieAmount > 0)
            gameModel.SpawnZombie(ZombieType.WALKER, zombieAmount, s.CurrentTile);//itt meg is kell jeleníteni
        if (survivor == s)
        {
            List<Item> realItems = new List<Item>();
            foreach (var item in items)
            {
                if (item != string.Empty)
                    realItems.Add(ItemFactory.GetItemByName((ItemName)Enum.Parse(typeof(ItemName), item, true)));
            }
            if (realItems.Count > 0)
                OpenInventory(realItems);
        }
        UpdateZombieCanvasOnTile(s.CurrentTile.Id);
    }
    public void PickUpObjective(Survivor s)
    {
        GameObject gObject = null;
        foreach (Transform child in GameObject.FindWithTag("MapPrefab").transform)
        {
            if (child.name.StartsWith("Obj") && int.Parse(child.name.Substring(10)) == s.CurrentTile.Id)
            {
                gObject=child.gameObject; break;
            }
        }
        s.PickUpObjective();
        Destroy(gObject);
        ProgressBar.UpdateFill(survivor.APoints);
    }
    public void PickUpPimpWLocally(Survivor s, PimpWeapon pimp)
    {
        GameObject gObject = null;
        foreach (Transform child in GameObject.FindWithTag("MapPrefab").transform)
        {
            if (child.name.StartsWith("Pimp") && int.Parse(child.name.Substring(6)) == s.CurrentTile.Id)
            {
                gObject = child.gameObject; break;
            }
        }
        s.CurrentTile.PickUpPimpWeapon();
        Destroy(gObject);
        if (survivor == s)
        {
            OpenInventory(new List<Item>() { pimp });
        }
    }
    public void PickUpPimpW(Survivor s)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            var pimp = s.CurrentTile.PickUpPimpWeapon();
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.PimpWeapon, $"{pimp.Name.ToString()}:{s.Name}");
        }
    }
    public void PlayerFinishedRound(string data)
    {
        SurvivorFactory.GetSurvivorByName(data).FinishedRound=true;//beállitani a használt képességeket falsera
        gameModel.NextPlayer();
        if(gameModel.IsPlayerRoundOver)
        {
            ShowPlayerOrder();
            ZombieRound();
        }
        else
            StartNextTurn();
    }
    private void ZombieRound()
    {
        gameModel.EndRound();
        if(NetworkManager.Singleton.IsHost)
        {
            List<string> spawns = new List<string>();
            for (int i = 0; i < gameModel.SpawnCount; i++)
            {
                (ZombieType, int, int, int, int) option = gameModel.ChooseZombieSpawnOption();
                spawns.Add($"{option.Item1},{option.Item2},{option.Item3},{option.Item4},{option.Item5}");
            }
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.ZombieSpawn, string.Join(';',spawns));
        }
        UpdatePlayerStats();
    }
    private void NewRoundForPlayers()
    {
        foreach(var s in playerSelections.Values)
        {
            SurvivorFactory.GetSurvivorByName(s).NewRound();
        }
    }
    public void ReceivePimpWeapon(string data)
    {
        string[] strings=data.Split(':');
        PimpWeapon pimp = (PimpWeapon)ItemFactory.GetItemByName((ItemName)Enum.Parse(typeof(ItemName), strings[0].Replace("ItemName.", ""), true));
        Survivor s = SurvivorFactory.GetSurvivorByName(strings[1]);
        PickUpPimpWLocally(s,pimp);
    }
}
