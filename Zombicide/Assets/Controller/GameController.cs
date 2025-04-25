using Model;
using Model.Board;
using Model.Characters.Survivors;
using Model.Characters.Zombies;
using Network;
using View;
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
    [SerializeField] private DiceRoller? diceRoller;
    private int selectedMapID;
    private Dictionary<ulong, string> playerSelections=new Dictionary<ulong, string>();
    private bool isSurvivorDead=false;
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
        InGameView.Instance!.GenerateBoard(mapID);
        InGameView.Instance!.GeneratePlayersOnBoard(gameModel!.StartTile.Id, playerSelections.Values.ToList());
        survivor = gameModel.GetSurvivorByName(playerSelections[NetworkManager.Singleton.LocalClientId]);
        survivor.SetReference(gameModel);
        if (NetworkManager.Singleton.IsHost)
        {
            gameModel.DecidePlayerOrder();

            string serializedOrder = string.Join(',', gameModel.PlayerOrder.Select(x => x.Name));
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.PlayerOrder, serializedOrder);

            List<Weapon> genericW=gameModel.GenerateGenericWeapons();
            string serializedGenericW = string.Join(",", genericW.Select(x => x.Name));
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.GenericWeapon, serializedGenericW);
        }
        InGameView.Instance.ShowPlayerUI(survivor!,playerSelections[NetworkManager.Singleton.LocalClientId]);
        if(NetworkManager.Singleton.IsHost)
            StartNextTurn();
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
                return entry.Key;
            }
        }
        return null;
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
        InGameView.Instance!.ShowPlayerOrder(gameModel!.IsPlayerRoundOver, gameModel.CurrentPlayer!, gameModel.PlayerOrder.ToList());
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
            InGameView.Instance!.UpdatePlayerStats(survivor!);
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
        InGameView.Instance!.UpdateItemSlots(survivor!);
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
            InGameView.Instance!.UpdatePlayerStats(survivor!);
        InGameView.Instance!.EnableBoardInteraction(survivor == gameModel!.CurrentPlayer);
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
        InGameView.Instance!.UpdateItemSlots(survivor!);
        InGameView.Instance.UpdatePlayerStats(survivor!);
        if (survivor.Name == s.Name && isSearch=="False")
            IncreaseUsedActions("Rearrange Items", s, null);
        int level = s.CanUpgradeTo();
        if (level > 0 && s==survivor)
        {
            if(s.FinishedRound) s.FinishedRound = false;
            InGameView.Instance.IsMenuOpen = true;
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
        InGameView.Instance!.UpdateZombieCanvasOnTile(int.Parse(strings[0]), gameModel!.GetZombiesInPriorityOrderOnTile(gameModel.Board.GetTileByID(int.Parse(strings[0]))));
        if (survivor!.Name == s.Name)
            IncreaseUsedActions("Attack", s, strings[2]);
        InGameView.Instance.EnableBoardInteraction(survivor == gameModel.CurrentPlayer);
        ProgressBar.UpdateFill(survivor.APoints);
        InGameView.Instance.UpdatePlayerStats(survivor!);
        InGameView.Instance.UpdateItemSlots(survivor!);
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
            InGameView.Instance!.UpdateZombieCanvasOnTile(item.Id, gameModel!.GetZombiesInPriorityOrderOnTile(gameModel.Board.GetTileByID(item.Id)));
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
            InGameView.Instance!.UpdateZombieCanvasOnTile(item.Id, gameModel!.GetZombiesInPriorityOrderOnTile(gameModel.Board.GetTileByID(item.Id)));
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
                InGameView.Instance!.OpenInventory(realItems,true, survivor);
            if (survivor.Name == s.Name)
                IncreaseUsedActions("Search", s, null);
        }
        InGameView.Instance!.UpdateZombieCanvasOnTile(s.CurrentTile.Id, gameModel!.GetZombiesInPriorityOrderOnTile(gameModel.Board.GetTileByID(s.CurrentTile.Id)));
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
        SurvivorFactory.GetSurvivorByName(data).FinishedRound=true;
        gameModel!.NextPlayer();
        if(gameModel.IsPlayerRoundOver)
        {
            InGameView.Instance!.ShowPlayerOrder(gameModel!.IsPlayerRoundOver, gameModel.CurrentPlayer!, gameModel.PlayerOrder.ToList());
            ZombieRound();
        }
        else
            StartNextTurn();
    }
    #endregion
    #region Game Methods
    /// <summary>
    /// Updates the map interaction lock for the current player.
    /// </summary>
    /// <param name="playerID">ID of the player</param>
    private void UpdateBoardForActivePlayer(ulong playerID)
    {
        bool isActive = playerID == NetworkManager.Singleton.LocalClientId;
        InGameView.Instance!.EnableBoardInteraction(isActive);
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
        InGameView.Instance!.RemovePlayer(playerName);
        if (survivor!.Name == playerName)
            InGameView.Instance.EnableBoardInteraction(false);
        if(gameModel!.CurrentPlayer==survivor && survivor.Name==playerName && !survivor.LeftExit)
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.FinishedRound, survivor.Name);
    }
    /// <summary>
    /// Starts the next player's turn.
    /// </summary>
    private void StartNextTurn()
    {
        gameModel!.CurrentPlayer!.StartedRound = true;
        gameModel.CurrentPlayer.SetFreeActions();
        InGameView.Instance!.ShowPlayerOrder(gameModel!.IsPlayerRoundOver, gameModel.CurrentPlayer!, gameModel.PlayerOrder.ToList());
        if (gameModel.CurrentPlayer==survivor)
            InGameView.Instance.UpdatePlayerStats(survivor!);

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
        InGameView.Instance!.DestroyDoorWithTween(door);
        InGameView.Instance.EnableDoors(false,survivor!);
        InGameView.Instance.IsMenuOpen = false;
        InGameView.Instance.EnableBoardInteraction(survivor == gameModel!.CurrentPlayer);
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
        InGameView.Instance!.UpdatePlayerStats(survivor!);
        int level = s.CanUpgradeTo();
        if (level > 0)
        {
            if(s.FinishedRound) s.FinishedRound = false;
            InGameView.Instance.IsMenuOpen = true;
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
                InGameView.Instance!.EnableBoardInteraction(gameModel!.CurrentPlayer == survivor);
                break;
            case "Open Door Left Hand":
                OpenDoor(objectName, "Left Hand", s);
                if (survivor!.Name == s.Name)
                    IncreaseUsedActions("Open Door", s, null);
                InGameView.Instance!.EnableBoardInteraction(gameModel!.CurrentPlayer == survivor);
                break;
            case "Search":
                SearchOnTile(s);
                break;
            case "Skip":
                s.Skip();
                break;
            case "Move":
                s.Move(gameModel!.Board.GetTileByID(int.Parse(objectName.Substring(8))));
                InGameView.Instance!.MovePlayerToTile(int.Parse(objectName.Substring(8)), s.Name.Replace(" ",string.Empty), gameModel!.NumberOfPlayersOnTile(int.Parse(objectName.Substring(8))) - 1, gameModel!.GetSurvivorsOnTile(gameModel.Board.GetTileByID(int.Parse(objectName.Substring(8)))).Select(x => x.Name).ToList());
                if (survivor!.Name == s.Name)
                    IncreaseUsedActions("Move", s, null);
                break;
            case "Slippery Move":
                s.SlipperyMove(gameModel!.Board.GetTileByID(int.Parse(objectName.Substring(8))));
                InGameView.Instance!.MovePlayerToTile(int.Parse(objectName.Substring(8)), s.Name.Replace(" ", string.Empty), gameModel!.NumberOfPlayersOnTile(int.Parse(objectName.Substring(8))) - 1, gameModel!.GetSurvivorsOnTile(gameModel.Board.GetTileByID(int.Parse(objectName.Substring(8)))).Select(x => x.Name).ToList());
                if (survivor!.Name == s.Name)
                    IncreaseUsedActions("Slippery Move", s, null);
                break;
            case "Sprint Move":
                s.SprintMove(gameModel!.Board.GetTileByID(int.Parse(objectName.Substring(8))));
                InGameView.Instance!.MovePlayerToTile(int.Parse(objectName.Substring(8)), s.Name.Replace(" ", string.Empty), gameModel!.NumberOfPlayersOnTile(int.Parse(objectName.Substring(8))) - 1, gameModel!.GetSurvivorsOnTile(gameModel.Board.GetTileByID(int.Parse(objectName.Substring(8)))).Select(x => x.Name).ToList());
                if (survivor!.Name == s.Name)
                    IncreaseUsedActions("Sprint Move", s, null);
                break;
            case "Jump":
                s.Jump(gameModel!.Board.GetTileByID(int.Parse(objectName.Substring(8))));
                InGameView.Instance!.MovePlayerToTile(int.Parse(objectName.Substring(8)), s.Name.Replace(" ", string.Empty), gameModel!.NumberOfPlayersOnTile(int.Parse(objectName.Substring(8))) - 1, gameModel!.GetSurvivorsOnTile(gameModel.Board.GetTileByID(int.Parse(objectName.Substring(8)))).Select(x => x.Name).ToList());
                if (survivor!.Name == s.Name)
                    IncreaseUsedActions("Jump", s, null);
                break;
            case "Charge":
                s.Charge(gameModel!.Board.GetTileByID(int.Parse(objectName.Substring(8))));
                InGameView.Instance!.MovePlayerToTile(int.Parse(objectName.Substring(8)), s.Name.Replace(" ", string.Empty), gameModel!.NumberOfPlayersOnTile(int.Parse(objectName.Substring(8))) - 1, gameModel!.GetSurvivorsOnTile(gameModel.Board.GetTileByID(int.Parse(objectName.Substring(8)))).Select(x => x.Name).ToList());
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
            InGameView.Instance!.UpdatePlayerStats(survivor!);
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
            InGameView.Instance!.OpenPriorityMenu(data, gameModel!.GetZombiesInPriorityOrderOnTile(gameModel.Board.GetTileByID(int.Parse(data.Split(':')[0]))));
            return;
        }
        else if((!isMelee && survivor.Traits.Contains(Trait.SNIPER))||((!isMelee)&& (weapon.Name==ItemName.SNIPERRIFLE || weapon.Name==ItemName.MILITARYSNIPERRIFLE)))
        {
            InGameView.Instance!.OpenPriorityMenu(data, gameModel!.GetZombiesInPriorityOrderOnTile(gameModel.Board.GetTileByID(int.Parse(data.Split(':')[0]))));
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

        InGameView.Instance!.IsMenuOpen = true;
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
        InGameView.Instance!.ShowPopupSearch(s.Name.Replace(" ", string.Empty));
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
        InGameView.Instance!.DestroyWithTween(gObject!);
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
        InGameView.Instance!.DestroyWithTween(gObject!);
        if (survivor == s)
        {
            InGameView.Instance.OpenInventory(new List<Item>() { pimp },true,survivor);
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
        InGameView.Instance!.UpdatePlayerStats(survivor!);
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
        InGameView.Instance!.EnableBoardInteraction(false);
        GameObject ui = GameObject.FindWithTag("GameUI");
        GameObject endPanelPrefab = Resources.Load<GameObject>("Prefabs/EndPanel");
        GameObject endPanel=Instantiate(endPanelPrefab,ui.transform);
        InGameView.Instance.IsMenuOpen = true;
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
        InGameView.Instance!.IsMenuOpen = false;
        NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.Attack, data += $":{string.Join(',', results)}");
    }
    /// <summary>
    /// Event handler for the player confirming the priority window.
    /// </summary>
    public void OnOkPriorityMenuClicked(string data, GameObject sniperMenuForS)
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
        InGameView.Instance!.SetCameraDrag(true);
        Destroy(sniperMenuForS);

        data += $":{string.Join(',', newPriority)}:{survivor!.Name}";
        InGameView.Instance.IsMenuOpen = false;
        RollDice(data);
    }
    /// <summary>
    /// Event handler for the player confirming the inventory window.
    /// </summary>
    public void OnOkInventoryClicked(bool isSearch, GameObject inventoryForS)
    {
        InGameView.Instance!.SetCameraDrag(true);
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
        InGameView.Instance.IsMenuOpen = false;
        InGameView.Instance.EnableBoardInteraction(survivor == gameModel!.CurrentPlayer);
    }
    /// <summary>
    /// Event handler for the player selecting the trait.
    /// </summary>
    private void OnTraitSelected(int level, int option)
    {
        InGameView.Instance!.IsMenuOpen =false;
        NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.TraitUpgrade, $"{survivor!.Name};{level};{option}");
    }
    /// <summary>
    /// Event handler for the player confirming the game over panel.
    /// </summary>
    public void OnOkEndGameClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.GameEnded, "");
        }
        else
        {
            NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.PlayerLeft, survivor!.Name);
            if(gameModel!.CurrentPlayer==survivor)
                NetworkManagerController.Instance.SendMessageToClientsServerRpc(MessageType.FinishedRound, survivor.Name);

            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("MenuScene");
        }
    }
    public void OnOpenInventory(bool isSearch)
    {
        InGameView.Instance!.OpenInventory(null, isSearch, survivor!);
    }
    public void OnOpenDoors(bool enable)
    {
        InGameView.Instance!.EnableDoors(true,survivor!); 
        InGameView.Instance.EnableBoardInteraction(false);
    }
    #endregion
    #endregion
}
