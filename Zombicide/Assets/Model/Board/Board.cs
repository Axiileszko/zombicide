using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Model.Board
{
    public class Board
    {
        public List<MapTile> Tiles { get; } = new List<MapTile>();
        public List<Building> Buildings { get; } = new List<Building>();
        public List<Street> Streets { get; } = new List<Street>();
        public Board(BoardData boardData)
        {
            Dictionary<int, MapTile> tileMap = new Dictionary<int, MapTile>();

            foreach (var tileData in boardData.tiles)
            {
                TileType? type = null;
                ZombieSpawnType? stype = null;
                switch (tileData.type)
                {
                    case "R": type = TileType.ROOM; break;
                    case "D": type = TileType.DARKROOM; break;
                    case "S": type = TileType.STREET; break;
                }
                if (tileData.extra != null && tileData.extra.Contains("Z"))
                {
                    switch (tileData.extra.Substring(tileData.extra.IndexOf('Z'), 2))
                    {
                        case "ZF": stype = ZombieSpawnType.FIRST; break;
                        case "ZB": stype = ZombieSpawnType.BLUE; break;
                        case "ZG": stype = ZombieSpawnType.GREEN; break;
                        case "ZR": stype = ZombieSpawnType.RED; break;
                    }
                }
                bool hasObj, hasPW, isExit, isStart;
                if (tileData.extra != null)
                {
                    hasObj = tileData.extra.Contains('O');
                    hasPW = tileData.extra.Contains('P');
                    isExit = tileData.extra.Contains('E');
                    isStart = tileData.extra.Contains('S');
                }
                else
                {
                    hasObj = false; hasPW = false; isExit = false; isStart = false;
                }
                MapTile tile = new MapTile(tileData.id, type, hasObj, hasPW, isExit, isStart, stype);
                tileMap[tileData.id] = tile;
                AddTile(tile);
            }

            foreach (var connectionData in boardData.connections)
            {
                if (tileMap.ContainsKey(connectionData.from) && tileMap.ContainsKey(connectionData.to))
                {
                    MapTile fromTile = tileMap[connectionData.from];
                    MapTile toTile = tileMap[connectionData.to];

                    TileConnection connection = new TileConnection(toTile, connectionData.isWall, connectionData.hasDoor, connectionData.isDoorOpen);
                    fromTile.AddNeighbour(connection);

                    TileConnection connection2 = new TileConnection(fromTile, connectionData.isWall, connectionData.hasDoor, connectionData.isDoorOpen);
                    toTile.AddNeighbour(connection2);
                }
            }

            foreach (var buildingData in boardData.buildings)
            {
                List<MapTile> rooms = new List<MapTile>();
                foreach (var roomId in buildingData.rooms)
                {
                    if (tileMap.ContainsKey(roomId))
                    {
                        rooms.Add(tileMap[roomId]);
                    }
                }
                AddBuilding(new Building(rooms));
            }
            foreach (var streetData in boardData.streets)
            {
                List<int> streetTiles = new List<int>();
                foreach (var tileId in streetData.tiles)
                {
                    if (tileMap.ContainsKey(tileId))
                    {
                        streetTiles.Add(tileId);
                    }
                }
                AddStreet(new Street(streetTiles));
            }
        }
        public void AddTile(MapTile tile)
        {
            Tiles.Add(tile);
        }

        public void AddBuilding(Building building)
        {
            Buildings.Add(building);
        }
        public void AddStreet(Street street)
        {
            Streets.Add(street);
        }
        public Street GetStreetByTiles(int tileID, int neighID)
        {
            foreach (var item in Streets)
            {
                if (item.Tiles.Contains(tileID) && item.Tiles.Contains(neighID))
                {
                    return item;
                }
            }
            return null;
        }
        public Building GetBuildingByTile(int tileID)
        {
            MapTile maptile = GetTileByID(tileID);
            foreach (var item in Buildings)
            {
                if (item.Rooms.Contains(maptile))
                {
                    return item;
                }
            }
            return null;
        }
        public MapTile GetTileByID(int tileID)
        {
            return Tiles.First(x=>x.Id==tileID);
        }
        public bool CanMove(MapTile from, MapTile to)
        {
            var connection = from.Neighbours.FirstOrDefault(n => n.Destination == to);
            if (connection == null) return false;
            if (connection.IsWall) return false;
            if (connection.HasDoor && !connection.IsDoorOpen) return false;
            return true;
        }
        public static int GetShortestPath(MapTile start, MapTile goal)
        {
            if (start == goal) return 0;

            Queue<(MapTile tile, int distance)> queue = new Queue<(MapTile, int)>();
            HashSet<MapTile> visited = new HashSet<MapTile>();

            queue.Enqueue((start, 0));
            visited.Add(start);

            while (queue.Count > 0)
            {
                var (currentTile, distance) = queue.Dequeue();

                foreach (var connection in currentTile.Neighbours)
                {
                    if (connection.IsWall || (connection.HasDoor && !connection.IsDoorOpen))
                        continue;

                    MapTile nextTile = connection.Destination;
                    if (visited.Contains(nextTile))
                        continue;

                    if (nextTile == goal)
                        return distance + 1;

                    queue.Enqueue((nextTile, distance + 1));
                    visited.Add(nextTile);
                }
            }

            return -1;
        }
    }

}
