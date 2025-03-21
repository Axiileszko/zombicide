using Persistence;
using Model.Characters.Survivors;
using Model.Characters.Zombies;
using System;
using System.Collections.Generic;
using System.Linq;
using Model.Board;
using static UnityEditor.Progress;

namespace Model
{
    public class GameModel
    {
        private Board.Board board;
        private List<Survivor> survivors = new List<Survivor>();
        private List<Zombie> zombies = new List<Zombie>();
        private List<Item> items = new List<Item>();
        private Survivor currentPlayer;
        private List<Survivor> playerOrder = new List<Survivor>();
        private MapTile firstSpawn=null;
        private List<MapTile> zSpawns=new List<MapTile>();
        private MapTile startTile=null;
        private int dangerLevel;
        private bool hasAbomination;

        private System.Random random=new System.Random();

        private MapLoader mapLoader;
        public bool IsPlayerRoundOver=false;
        public int SpawnCount { get {  return zSpawns.Count+1; } }
        public Board.Board Board { get { return board; } }
        public List<Survivor> PlayerOrder { get { return playerOrder; } }
        public Survivor CurrentPlayer { get { return currentPlayer; } }
        public MapTile StartTile { get { return startTile; } }
        public delegate bool WinningConditionDelegate();
        public WinningConditionDelegate CheckWinningCondition;
        public List<int> SurvivorLocations {  get 
            {
                List<int> locations = new List<int>();
                foreach (var item in survivors)
                {
                    locations.Add(item.CurrentTile.Id);
                }
                return locations;
            } 
        }

        public GameModel()
        {
            mapLoader=new MapLoader();
        }
        public GameModel(MapLoader mapLoader)
        {
            this.mapLoader = mapLoader;
        }

        public void StartGame(List<string> survivors, int mapID)
        {
            IsPlayerRoundOver = false;
            GenerateItems();
            hasAbomination = false;
            dangerLevel = 0;
            this.survivors = SpawnSurvivors(survivors);
            LoadGame(mapID);
            SetWinningCondition(mapID);
            FindSpawns();
            MovePlayersToSpawn();
        }
        public void SetWinningCondition(int mapID)
        {
            // Térkép azonosítótól függõen beállítjuk a megfelelõ metódust
            switch (mapID)
            {
                case 0:
                    CheckWinningCondition = WinningCondition_Map0;
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
        public void EndRound()
        {
            //jatekosok itt jonnek
            MoveZombies();
            ClearNoiseCounters();
            UpdateDangerLevel();
        }

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

        private void ClearNoiseCounters()
        {
            foreach (var item in board.Tiles)
            {
                item.NoiseCounter = 0;
            }
        }
        public void GiveSurvivorsGenericWeapon(List<ItemName> weapons)
        {
            if(survivors.Count != weapons.Count) return;
            for (int i = 0; i < survivors.Count; i++)
            {
                survivors[i].PickGenericWeapon(ItemFactory.GetGenericWeaponByName(weapons[i]));
            }
        }
        public bool BuildingOpened(TileConnection connection)
        {
            Building building = board.GetBuildingByTile(connection.Destination.Id);
            if (building != null && !building.IsOpened)
            {
                building.OpenBuilding();
                return true;
            }
            return false;
        }
        public Survivor GetSurvivorByName(string name)
        {
            return survivors.First(x => x.Name == name);
        }
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
        public List<Zombie> SortZombiesByNewPriority(List<Zombie> zombies,List<string> order)
        {
            return zombies.OrderBy(z => GetPriorityIndex(z, order)).ToList();
        }
        private static int GetPriorityIndex(Zombie z, List<string> order)
        {
            string typeName = z.GetType().Name.ToLower();

            if (z is AbominationZombie)
                typeName = "abomination";

            return order.IndexOf(typeName);
        }
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
        public void RemoveZombie(Zombie zombie)
        {
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

        private void UpdateNoiseCounters()
        {
            foreach (var item in survivors)
            {
                item.CurrentTile.NoiseCounter++;
            }
        }

        private void MovePlayersToSpawn()
        {
            foreach (var item in survivors)
            {
                item.MoveTo(startTile);
            }
            Building building = board.GetBuildingByTile(startTile.Id);
            if (building != null)
                building.OpenBuilding();
        }
        private void GenerateItems()
        {
            items = ItemFactory.CreateItems();
            ItemFactory.CreatePimpWeapons();
            ItemFactory.CreateGenericWeapons();
        }
        public MapTile FindNextStepToNoisiest(MapTile start)
        {
            //Megkeressük a leghangosabb mezõt
            MapTile target = board.Tiles.OrderByDescending(t => t.NoiseCounter).FirstOrDefault();
            if (target == null || target == start) return null;

            //Dijkstra algoritmus
            Queue<MapTile> queue = new Queue<MapTile>();
            Dictionary<MapTile, MapTile> cameFrom = new Dictionary<MapTile, MapTile>(); // Honnan jöttünk egy mezõre
            HashSet<MapTile> visited = new HashSet<MapTile>(); // Már bejárt mezõk

            queue.Enqueue(start);
            visited.Add(start);
            cameFrom[start] = null;

            while (queue.Count > 0)
            {
                MapTile current = queue.Dequeue();

                if (current == target) break; // Megtaláltuk a célt

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

            // 3. Útvonal visszafejtése: Az elsõ lépés kinyerése
            if (!cameFrom.ContainsKey(target)) return null; // Nincs elérhetõ út

            MapTile step = target;
            while (cameFrom[step] != start) // Visszakövetjük az utat a kezdõ mezõig
            {
                step = cameFrom[step];
            }

            return step; // Az elsõ lépés a start mezõ szomszédja
        }
        public List<int> DiceRoll(int diceAmount, int accuracy)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < diceAmount; i++)
            {
                result.Add(random.Next(1, 7)); // 1-6 között
            }
            return result;
        }
        public void ReRoll(ref List<int> before, int accuracy)
        {
            for (int i = 0; i < before.Count; i++)
            {
                if (before[i]<accuracy)
                    before[i]=random.Next(1, 7); // 1-6 között
            }
        }
        public List<Item> Search(MapTile tile, bool useFlashlight)
        {
            if (useFlashlight)
            {
                List<Item> result = new List<Item>();
                int roll = random.Next(0, items.Count() + 3);
                int roll2 = random.Next(0, items.Count() + 3);
                if (roll < items.Count)
                    result.Add(items.ElementAt(roll));
                if (roll2 < items.Count)
                    result.Add(items.ElementAt(roll2));
                return result;
            }
            else
            {
                int roll = random.Next(0, items.Count()+3);
                if (roll >= items.Count)
                {
                    return null;
                }
                return new List<Item>() { items.ElementAt(roll) };
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
                } while (spawn.Item1 != ZombieType.ABOMINAWILD || spawn.Item1 != ZombieType.ABOMINACOP || spawn.Item1 != ZombieType.HOBOMINATION || spawn.Item1 != ZombieType.PATIENTZERO);
            }
            else
            {
                spawn = ZombieFactory.GetSpawnOption();
            }
            return spawn;
        }
        public void SpawnZombies(List<(ZombieType, int, int, int, int, bool)> spawns)
        {
            SpawnZombiesOnTile(spawns[0], firstSpawn);
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
            if (type == ZombieType.ABOMINAWILD || type == ZombieType.ABOMINACOP || type== ZombieType.HOBOMINATION || type ==ZombieType.PATIENTZERO)
            {
                if (hasAbomination)
                    MoveAbomination();
                else
                    hasAbomination = true;
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
            Zombie abo = zombies.First(x => x is AbominationZombie);
            abo.Move();
        }
        private void FindSpawns()
        {
            foreach (var item in board.Tiles)
            {
                if (item.SpawnType==ZombieSpawnType.FIRST)
                {
                    firstSpawn = item;
                }
                else if (item.SpawnType==ZombieSpawnType.RED || item.SpawnType == ZombieSpawnType.GREEN|| item.SpawnType == ZombieSpawnType.BLUE)
                {
                    zSpawns.Add(item);
                }
                else if (item.IsStart)
                {
                    startTile = item;
                }
            }
        }

        private List<Survivor> SpawnSurvivors(List<string> survivors)
        {
            List<Survivor> sList= new List<Survivor>();
            foreach (string s in survivors)
            {
                Survivor survivor=SurvivorFactory.GetSurvivorByName(s);
                survivor.SetReference(this);
                sList.Add(survivor);
            }
            ResetSurvivors(sList);
            return sList;
        }

        private void ResetSurvivors(List<Survivor> sList)
        {
            foreach (var survivor in sList)
            {
                survivor.Reset();
            }
        }

        public void LoadGame(int number)
        {
            board=mapLoader.LoadMap(number);
        }

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
        public void DecidePlayerOrder()
        {
            playerOrder = survivors.OrderBy(x => random.Next()).ToList();
            currentPlayer = playerOrder[0];
        }

        public void SetPlayerOrder(List<string>? survivorOrder)
        {
            if (playerOrder.Count==0)
            {
                List<Survivor> newOrder = new List<Survivor>();
                foreach (var item in survivorOrder)
                {
                    newOrder.Add(survivors.First(x => x.Name == item));
                }
                playerOrder = newOrder;
                currentPlayer = playerOrder[0];
            }
        }
        public void NextPlayer()
        {
            if (currentPlayer==playerOrder.Last())
            {
                IsPlayerRoundOver = true;
                currentPlayer = null;
            }
            else
            {
                int index=playerOrder.IndexOf(currentPlayer);
                index++;
                currentPlayer = playerOrder[index];
            }
        }
        public int NumberOfPlayersOnTile(int tileID)
        {
            return SurvivorLocations.Count(x=>x==tileID);
        }
        public void ShiftPlayerOrder()
        {
            var starter = playerOrder[0];
            playerOrder.RemoveAt(0);
            playerOrder.Add(starter);
            currentPlayer = playerOrder[0];
        }

    }
}
