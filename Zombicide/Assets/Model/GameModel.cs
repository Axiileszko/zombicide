using Persistence;
using Model.Characters.Survivors;
using Model.Characters.Zombies;
using System;
using System.Collections.Generic;
using System.Linq;
using Model.Board;
using System.Diagnostics;

namespace Model
{
    #nullable enable
    public class GameModel
    {
        #region Fields
        private Board.Board? board;
        private List<Survivor> survivors = new List<Survivor>();
        private List<Zombie> zombies = new List<Zombie>();
        private List<Item> items = new List<Item>();
        private Survivor? currentPlayer;
        private List<Survivor> playerOrder = new List<Survivor>();
        private MapTile? firstSpawn=null;
        private MapTile? exitTile=null;
        private List<MapTile> zSpawns=new List<MapTile>();
        private MapTile? startTile=null;
        private int dangerLevel;
        private bool hasAbomination;
        private Zombie? abomination = null;
        private System.Random random=new System.Random();
        private MapLoader mapLoader;
        #endregion
        #region Properties
        public event EventHandler<bool>? GameEnded;
        public bool GameOver {  get; private set; }=false;
        public bool IsPlayerRoundOver { get; set; } = false;
        public Zombie? Abomination { get {  return abomination; } }
        public int SpawnCount { get {  return zSpawns.Count+1; } }
        public MapTile? ExitTile { get { return exitTile; } }
        public Board.Board Board { get { return board!; } }
        public List<Survivor> PlayerOrder { get { return playerOrder; } }
        public Survivor? CurrentPlayer { get { return currentPlayer; } }
        public MapTile StartTile { get { return startTile!; } }
        public delegate bool WinningConditionDelegate();
        public WinningConditionDelegate? CheckWinningCondition;
        public List<int> SurvivorLocations {  get 
            {
                List<int> locations = new List<int>();
                foreach (var item in survivors)
                {
                    if(!item.IsDead && !item.LeftExit)
                        locations.Add(item.CurrentTile.Id);
                }
                return locations;
            } 
        }
        #endregion
        #region Constructors
        public GameModel()
        {
            mapLoader=new MapLoader();
        }
        public GameModel(MapLoader mapLoader)
        {
            this.mapLoader = mapLoader;
        }
        #endregion
        #region Methods
        #region Query Methods
        /// <summary>
        /// Returns the amount of survivors on the given tile
        /// </summary>
        /// <param name="tileID">ID of the tile</param>
        /// <returns>Amount of survivors on tile</returns>
        public int NumberOfPlayersOnTile(int tileID)
        {
            return SurvivorLocations.Count(x=>x==tileID);
        }
        /// <summary>
        /// Returns the survivor's reference by their name
        /// </summary>
        /// <param name="name">Name of the survivor</param>
        /// <returns>Survivor</returns>
        public Survivor GetSurvivorByName(string name)
        {
            return survivors.First(x => x.Name == name);
        }
        /// <summary>
        /// Returns the zombies in priority order on the tile given
        /// </summary>
        /// <param name="mapTile">Tile</param>
        /// <returns>List of zombies in priority order</returns>
        public List<Zombie> GetZombiesInPriorityOrderOnTile(MapTile mapTile)
        {
            List<Zombie> zombiesOnTile = new List<Zombie>();
            foreach (var item in zombies)
            {
                if(item.CurrentTile==mapTile)
                    zombiesOnTile.Add(item);
            }
            return zombiesOnTile.OrderBy(x => x.Priority).ToList();
        }
        /// <summary>
        /// Returns the given zombie's index within the given list
        /// </summary>
        /// <param name="z">Zombie</param>
        /// <param name="order">Priority order</param>
        /// <returns></returns>
        private static int GetPriorityIndex(Zombie z, List<string> order)
        {
            string typeName = z.GetType().Name;

            if (z is AbominationZombie)
                typeName = "Abomination";

            return order.IndexOf(typeName);
        }
        /// <summary>
        /// Returns the list of survivors on the given tile
        /// </summary>
        /// <param name="mapTile">Tile</param>
        /// <returns>List of survivors</returns>
        public List<Survivor> GetSurvivorsOnTile(MapTile mapTile)
        {
            List<Survivor> survivorsOnTile = new List<Survivor>();
            foreach (var item in survivors)
            {
                if (item.CurrentTile == mapTile)
                    survivorsOnTile.Add(item);
            }
            return survivorsOnTile;
        }
        #endregion
        #region New Game Methods
        /// <summary>
        /// Starts the game
        /// </summary>
        /// <param name="survivors">List of survivors that will play the game</param>
        /// <param name="mapID">ID of the chosen map</param>
        public void StartGame(List<string> survivors, int mapID)
        {
            IsPlayerRoundOver = false;
            GenerateItems();
            hasAbomination = false;
            abomination = null;
            dangerLevel = 0;
            this.survivors = SpawnSurvivors(survivors);
            LoadGame(mapID);
            SetWinningCondition(mapID);
            FindSpawns();
            MovePlayersToSpawn();
        }
        /// <summary>
        /// Loads the map by ID using MapLoader
        /// </summary>
        /// <param name="number">ID of the map</param>
        public void LoadGame(int number)
        {
            board=mapLoader.LoadMap(number);
        }
        /// <summary>
        /// Ending the players' round and starting the zombies' round 
        /// </summary>
        public void EndRound()
        {
            //jatekosok itt jonnek
            CheckWin();
            MoveZombies();
            ClearNoiseCounters();
            UpdateDangerLevel();
        }
        /// <summary>
        /// Gives every survivor their start weapon
        /// </summary>
        /// <param name="weapons">List of start weapons</param>
        public void GiveSurvivorsGenericWeapon(List<ItemName> weapons)
        {
            if(survivors.Count != weapons.Count) return;
            for (int i = 0; i < survivors.Count; i++)
            {
                survivors[i].PickGenericWeapon(ItemFactory.GetGenericWeaponByName(weapons[i]));
            }
        }
        /// <summary>
        /// Moves the players to the start tile
        /// </summary>
        private void MovePlayersToSpawn()
        {
            foreach (var item in survivors)
            {
                item.MoveTo(startTile);
            }
            Building building = board!.GetBuildingByTile(startTile!.Id);
            if (building != null)
                building.OpenBuilding();
        }
        /// <summary>
        /// Generates all the items using ItemFactory
        /// </summary>
        private void GenerateItems()
        {
            items = ItemFactory.CreateItems();
            ItemFactory.CreatePimpWeapons();
            ItemFactory.CreateGenericWeapons();
        }
        /// <summary>
        /// Finds all the zombie spawn locations, exit and start location
        /// </summary>
        private void FindSpawns()
        {
            foreach (var item in board!.Tiles)
            {
                if (item.SpawnType==ZombieSpawnType.FIRST)
                    firstSpawn = item;
                else if (item.SpawnType==ZombieSpawnType.RED || item.SpawnType == ZombieSpawnType.GREEN|| item.SpawnType == ZombieSpawnType.BLUE)
                    zSpawns.Add(item);
                else if (item.IsStart)
                    startTile = item;
                else if(item.IsExit)
                    exitTile = item;
            }
        }
        /// <summary>
        /// Instantiates all the survivors
        /// </summary>
        /// <param name="survivors">List of survivors</param>
        /// <returns></returns>
        private List<Survivor> SpawnSurvivors(List<string> survivors)
        {
            List<Survivor> sList= new List<Survivor>();
            foreach (string s in survivors)
            {
                Survivor survivor=SurvivorFactory.GetSurvivorByName(s);
                survivor.SetReference(this);
                sList.Add(survivor);
                survivor.SurvivorDied += Survivor_SurvivorDied;
            }
            ResetSurvivors(sList);
            return sList;
        }
        /// <summary>
        /// Resets every survivor to their beginning state
        /// </summary>
        /// <param name="sList">List of survivors</param>
        private void ResetSurvivors(List<Survivor> sList)
        {
            foreach (var survivor in sList)
            {
                survivor.Reset();
            }
        }
        /// <summary>
        /// Generates all the start weapons using ItemFactory
        /// </summary>
        /// <returns></returns>
        public List<Weapon> GenerateGenericWeapons()
        {
            List<Weapon> weapons = new List<Weapon>
            {
                ItemFactory.GetGenericWeaponByName(ItemName.AXE)
            };
            for (int i = 0; i < survivors.Count-1; i++)
            {
                weapons.Add(ItemFactory.GetGenericWeapon());
            }
            return weapons;
        }
        /// <summary>
        /// Determines the player order for the game
        /// </summary>
        public void DecidePlayerOrder()
        {
            playerOrder = survivors.OrderBy(x => random.Next()).ToList();
            currentPlayer = playerOrder[0];
        }
        /// <summary>
        /// Sets the player order to the given order
        /// </summary>
        /// <param name="survivorOrder"></param>
        public void SetPlayerOrder(List<string>? survivorOrder)
        {
            if (playerOrder.Count==0)
            {
                List<Survivor> newOrder = new List<Survivor>();
                foreach (var item in survivorOrder!)
                {
                    newOrder.Add(survivors.First(x => x.Name == item));
                }
                playerOrder = newOrder;
                currentPlayer = playerOrder[0];
            }
        }
        #endregion
        #region New Round Methods
        /// <summary>
        /// Opens the building corresponding to the given tile connection
        /// </summary>
        /// <param name="connection">Tile connection</param>
        /// <returns>Whether zombies should spawn or not</returns>
        public bool BuildingOpened(TileConnection connection)
        {
            Building building = board!.GetBuildingByTile(connection.Destination.Id);
            if (building != null && !building.IsOpened)
            {
                building.OpenBuilding();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        private void UpdateDangerLevel()
        {
            if (survivors.Any(x => x.APoints >= 43))
            {
                dangerLevel = 3;
                return;
            }
            else if (survivors.Any(x => x.APoints >= 19))
            {
                dangerLevel = 2;
                return;
            }
            else if (survivors.Any(x => x.APoints >= 7))
            {
                dangerLevel = 1;
                return;
            }
            else
                dangerLevel = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        private void ClearNoiseCounters()
        {
            foreach (var item in board!.Tiles)
            {
                item.NoiseCounter = 0;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void UpdateNoiseCounters()
        {
            foreach (var item in survivors)
            {
                if(item.CurrentTile!=null)
                    item.CurrentTile.NoiseCounter++;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void NextPlayer()
        {
            if (currentPlayer==playerOrder.Last() && currentPlayer.FinishedRound)
            {
                IsPlayerRoundOver = true;
                currentPlayer = null;
            }
            else
            {
                int index=playerOrder.IndexOf(currentPlayer!);
                index++;
                currentPlayer = playerOrder[index];
                if(currentPlayer.IsDead || currentPlayer.LeftExit)
                    NextPlayer();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void ShiftPlayerOrder()
        {
            var starter = playerOrder[0];
            playerOrder.RemoveAt(0);
            playerOrder.Add(starter);
            currentPlayer = playerOrder[0];
            if(currentPlayer.IsDead || currentPlayer.LeftExit)
                NextPlayer();
        }
        #endregion
        #region WinningCondition Methods
        private void SetWinningCondition(int mapID)
        {
            // Térkép azonosítótól függõen beállítjuk a megfelelõ metódust
            switch (mapID)
            {
                case 0:
                    CheckWinningCondition = WinningCondition_Map0;
                    break;
                case 1:
                    CheckWinningCondition = WinningCondition_Map1;
                    break;
            }
        }
        private bool WinningCondition_Map0()
        {
            bool everyoneHasObjective=true;

            if(survivors.Any(x=>x.IsDead)) return false;
            int foods = 0;
            foreach (var s in survivors)
            {
                if(s.ObjectiveCount==0) everyoneHasObjective = false;
                foreach (var item in s.BackPack)
                {
                    if(item.Name==ItemName.CANNEDFOOD || item.Name==ItemName.RICE || item.Name==ItemName.WATER) foods++;
                }
                if (s.LeftHand != null && (s.LeftHand.Name == ItemName.CANNEDFOOD || s.LeftHand.Name == ItemName.RICE || s.LeftHand.Name == ItemName.WATER)) foods++;
                if (s.RightHand != null && (s.RightHand.Name == ItemName.CANNEDFOOD || s.RightHand.Name == ItemName.RICE || s.RightHand.Name == ItemName.WATER)) foods++;
            }
            if(everyoneHasObjective && foods >= 3)
            {
                if(survivors.Any(x=>!x.LeftExit)) return false;
                return true;
            }
            else
                return false;
        }
        private bool WinningCondition_Map1()
        {
            if (survivors.Any(x => x.IsDead)) return false;
            int pimpW = 0;
            foreach (var s in survivors)
            {
                foreach (var item in s.BackPack)
                {
                    if (item is PimpWeapon) pimpW++;
                }
                if (s.LeftHand != null && s.LeftHand is PimpWeapon) pimpW++;
                if (s.RightHand != null && s.RightHand is PimpWeapon) pimpW++;
            }
            if (pimpW == 9)
            {
                if (survivors.Any(x => !x.LeftExit)) return false;
                return true;
            }
            else
                return false;
        }
        private bool AreThereSurvivorsLeft()
        {
            return survivors.Any(x => !x.IsDead);
        }
        public void CheckWin()
        {
            if (CheckWinningCondition != null && CheckWinningCondition())
                OnGameEnded(true);
            else if (exitTile != null && CheckWinningCondition != null && !CheckWinningCondition() && survivors.All(x => x.LeftExit))
                OnGameEnded(false);
            else if (!AreThereSurvivorsLeft())
                OnGameEnded(false);
        }
        #endregion
        #region Zombie Methods
        public List<Zombie> SortZombiesByNewPriority(List<Zombie> zombies,List<string> order)
        {
            return zombies.OrderBy(z => GetPriorityIndex(z, order)).ToList();
        }
        public void RemoveZombie(Zombie zombie)
        {
            if(zombie==abomination)
                abomination = null;
            zombies.Remove(zombie);
        }
        private void MoveZombies()
        {
            UpdateNoiseCounters();
            foreach (var item in zombies)
            {
                item.Move();
                if(item is RunnerZombie)
                    item.Move();
            }
        }
        public MapTile? FindNextStepToNoisiest(MapTile start)
        {
            MapTile target = board!.Tiles.OrderByDescending(t => t.NoiseCounter).FirstOrDefault();
            if (target == null || target == start) return null;

            Queue<MapTile> queue = new Queue<MapTile>();
            Dictionary<MapTile, MapTile?> cameFrom = new Dictionary<MapTile, MapTile?>();
            HashSet<MapTile> visited = new HashSet<MapTile>();

            queue.Enqueue(start);
            visited.Add(start);
            cameFrom[start] = null;

            while (queue.Count > 0)
            {
                MapTile current = queue.Dequeue();

                if (current == target) break;

                foreach (var connection in current.Neighbours)
                {
                    MapTile neighbor = connection.Destination;
                    if (!visited.Contains(neighbor) && board.CanMove(current, neighbor))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                        cameFrom[neighbor] = current;
                    }
                }
            }

            if (!cameFrom.ContainsKey(target)) return null;

            MapTile step = target;
            while (cameFrom[step] != start)
            {
                step = cameFrom[step]!;
            }

            return step;
        }
        public List<Item>? Search(MapTile tile, bool useFlashlight, bool matchingSet)
        {
            if (useFlashlight)
            {
                List<Item> result = new List<Item>();
                int roll = random.Next(0, items.Count() + 2);
                int roll2 = random.Next(0, items.Count() + 2);
                if (roll < items.Count)
                    result.Add(items.ElementAt(roll));
                if (roll2 < items.Count)
                    result.Add(items.ElementAt(roll2));
                Item? dualItem = result.FirstOrDefault(x => x is Weapon w && w.IsDual);
                if (dualItem != null && matchingSet)
                    result.Add(dualItem);
                return result;
            }
            else
            {
                int roll = random.Next(0, items.Count()+3);
                if (roll >= items.Count)
                {
                    return null;
                }
                List<Item> result = new List<Item>
                {
                    items.ElementAt(roll)
                };
                Item? dualItem = result.FirstOrDefault(x => x is Weapon w && w.IsDual);
                if (dualItem != null && matchingSet)
                    result.Add(dualItem);
                return result;
            }
        }
        public (ZombieType, int, int, int, int, bool) ChooseZombieSpawnOption()
        {
            (ZombieType, int, int, int, int, bool) spawn;
            if (hasAbomination)
            {
                do
                {
                    spawn = ZombieFactory.GetSpawnOption();
                } while (spawn.Item1 == ZombieType.ABOMINAWILD || spawn.Item1 == ZombieType.ABOMINACOP || spawn.Item1 == ZombieType.HOBOMINATION || spawn.Item1 == ZombieType.PATIENTZERO);
            }
            else
            {
                spawn = ZombieFactory.GetSpawnOption();
                if(spawn.Item1== ZombieType.ABOMINAWILD || spawn.Item1 == ZombieType.ABOMINACOP || spawn.Item1 == ZombieType.HOBOMINATION || spawn.Item1 == ZombieType.PATIENTZERO)
                    hasAbomination = true;
            }
            return spawn;
        }
        public void SpawnZombies(List<(ZombieType, int, int, int, int, bool)> spawns)
        {
            SpawnZombiesOnTile(spawns[0], firstSpawn!);
            int i = 1;
            foreach (var item in zSpawns)
            {
                SpawnZombiesOnTile(spawns[i], item);
                i++;
            }
        }
        public void SpawnZombiesOnTile((ZombieType, int, int, int, int, bool) spawn, MapTile tile)
        {
            switch (dangerLevel)
            {
                case 0:
                    SpawnZombie(spawn.Item1,spawn.Item2,tile,spawn.Item6); break;
                case 1:
                    SpawnZombie(spawn.Item1, spawn.Item3, tile, spawn.Item6); break;
                case 2:
                    SpawnZombie(spawn.Item1, spawn.Item4, tile, spawn.Item6); break;
                case 3:
                    SpawnZombie(spawn.Item1, spawn.Item5, tile, spawn.Item6); break;
                default:
                    SpawnZombie(spawn.Item1, spawn.Item5, tile, spawn.Item6); break;
            }
        }
        public void SpawnZombie(ZombieType type, int amount, MapTile tile, bool moveThem)
        {
            if ((type == ZombieType.ABOMINAWILD || type == ZombieType.ABOMINACOP || type== ZombieType.HOBOMINATION || type ==ZombieType.PATIENTZERO)&& amount>0)
            {
                if (abomination!=null)
                    MoveAbomination();
                else if(abomination == null)
                {
                    abomination = ZombieFactory.CreateZombie(type);
                    abomination.SetReference(this);
                    abomination.MoveTo(tile);
                    zombies.Add(abomination);
                }   
                return;
            }
            for (int i = 0; i < amount; i++)
            {
                Zombie zombie=ZombieFactory.CreateZombie(type);
                zombie.SetReference(this);
                zombie.MoveTo(tile);
                zombies.Add(zombie);
                if (moveThem)
                    zombie.Move();
            }
        }
        private void MoveAbomination()
        {
            abomination!.Move();
        }
        #endregion
        #region Event Handlers
        private void Survivor_SurvivorDied(object sender, string e)
        {
            Survivor s = survivors.First(x => x.Name == e);
            s.SurvivorDied -= Survivor_SurvivorDied;
            survivors.Remove(s);
        }
        private void OnGameEnded(bool survivorsWon)
        {
            GameOver = true;
            GameEnded!.Invoke(this, survivorsWon);
        }
        #endregion
        #endregion
    }
}
