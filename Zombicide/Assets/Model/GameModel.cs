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
            SpawnZombies();
            MoveZombies();
            ClearNoiseCounters();
            ShiftPlayerOrder();
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
            foreach (var item in zombies)
            {
                item.Move();
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
        public Item Search(MapTile tile)
        {
            int roll = random.Next(0, items.Count()+3);
            if (roll > items.Count)
            {
                SpawnZombie(ZombieType.WALKER, 1, tile);
            }
            return items.ElementAt(roll);
        }
        private void SpawnZombies()
        {
            (ZombieType,int,int,int,int) spawn;
            if (hasAbomination)
            {
                do
                {
                    spawn = ZombieFactory.GetSpawnOption();
                } while (spawn.Item1!=ZombieType.ABOMINAWILD || spawn.Item1 != ZombieType.ABOMINACOP|| spawn.Item1 != ZombieType.HOBOMINATION|| spawn.Item1 != ZombieType.PATIENTZERO);
            }
            else
            {
                spawn = ZombieFactory.GetSpawnOption();
            }
            switch (dangerLevel)
            {
                case 0:
                    SpawnZombie(spawn.Item1,spawn.Item2,firstSpawn); break;
                case 1:
                    SpawnZombie(spawn.Item1, spawn.Item3, firstSpawn); break;
                case 2:
                    SpawnZombie(spawn.Item1, spawn.Item4, firstSpawn); break;
                case 3:
                    SpawnZombie(spawn.Item1, spawn.Item5, firstSpawn); break;
                default:
                    SpawnZombie(spawn.Item1, spawn.Item5, firstSpawn); break;
            }
            foreach (var item in zSpawns)
            {
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
                switch (dangerLevel)
                {
                    case 0:
                        SpawnZombie(spawn.Item1, spawn.Item2, item); break;
                    case 1:
                        SpawnZombie(spawn.Item1, spawn.Item3, item); break;
                    case 2:
                        SpawnZombie(spawn.Item1, spawn.Item4, item); break;
                    case 3:
                        SpawnZombie(spawn.Item1, spawn.Item5, item); break;
                    default:
                        SpawnZombie(spawn.Item1, spawn.Item5, item); break;
                }
            }
        }
        private void SpawnZombie(ZombieType type, int amount, MapTile tile)
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
                Survivor survivor=SurvivorFactory.CreateSurvivor(s);
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
            List<Weapon> weapons = new List<Weapon>();
            for (int i = 0; i < survivors.Count; i++)
            {
                weapons.Add(ItemFactory.GetGenericWeapon());
            }
            return weapons;
        }
        public void DecidePlayerOrder(List<string>? survivorOrder)
        {
            if(survivorOrder ==null)
                playerOrder=survivors.OrderBy(x=>random.Next()).ToList();
            else
            {
                List<Survivor> newOrder=new List<Survivor>();
                foreach (var item in survivorOrder)
                {
                    newOrder.Add(survivors.First(x => x.Name == item));
                }
                playerOrder = newOrder;
            }
            currentPlayer = playerOrder[0];
        }

        public int NumberOfPlayersOnTile(int tileID)
        {
            return SurvivorLocations.Count(x=>x==tileID);
        }
        private void ShiftPlayerOrder()
        {
            var starter = playerOrder[0];
            playerOrder.RemoveAt(0);
            playerOrder.Add(starter);
        }

    }
}
