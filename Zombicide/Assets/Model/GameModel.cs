using Persistence;
using Model.Characters.Survivors;
using Model.Characters.Zombies;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Rendering.DebugUI;
using Model.Board;

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
            FindSpawns();
            MovePlayersToSpawn();
        }
        public void EndRound()
        {
            //jatekosok itt jonnek
            MoveZombies();
            ClearNoiseCounters();
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
            return zombiesOnTile.OrderByDescending(x => x.Priority).ToList();
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
                if (roll > items.Count)
                {
                    return null;
                }
                return new List<Item>() { items.ElementAt(roll) };
            }
        }
        public (ZombieType, int, int, int, int) ChooseZombieSpawnOption()
        {
            (ZombieType, int, int, int, int) spawn;
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
        public void SpawnZombies(List<(ZombieType, int, int, int, int)> spawns)
        {
            SpawnZombiesOnTile(spawns[0], firstSpawn);
            int i = 1;
            foreach (var item in zSpawns)
            {
                SpawnZombiesOnTile(spawns[i], item);
                i++;
            }
        }
        private void SpawnZombiesOnTile((ZombieType, int, int, int, int) spawn, MapTile tile)
        {
            switch (dangerLevel)
            {
                case 0:
                    SpawnZombie(spawn.Item1,spawn.Item2,tile); break;
                case 1:
                    SpawnZombie(spawn.Item1, spawn.Item3, tile); break;
                case 2:
                    SpawnZombie(spawn.Item1, spawn.Item4, tile); break;
                case 3:
                    SpawnZombie(spawn.Item1, spawn.Item5, tile); break;
                default:
                    SpawnZombie(spawn.Item1, spawn.Item5, tile); break;
            }
        }
        public void SpawnZombie(ZombieType type, int amount, MapTile tile)
        {
            if (type == ZombieType.ABOMINAWILD || type == ZombieType.ABOMINACOP || type== ZombieType.HOBOMINATION || type ==ZombieType.PATIENTZERO)
            {
                hasAbomination = true;
            }
            for (int i = 0; i < amount; i++)
            {
                Zombie zombie=ZombieFactory.CreateZombie(type);
                zombie.SetReference(this);
                zombie.MoveTo(tile);
                zombies.Add(zombie);
            }
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
                Debug.Log("uj currentplayer:" + currentPlayer);
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
