using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Board
{
    public class Board
    {
        public List<MapTile> Tiles { get; } = new List<MapTile>();
        public List<Building> Buildings { get; } = new List<Building>();
        public List<Street> Streets { get; } = new List<Street>();

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
                        continue; // Nem lehet áthaladni

                    MapTile nextTile = connection.Destination;
                    if (visited.Contains(nextTile))
                        continue; // Már meglátogattuk

                    if (nextTile == goal)
                        return distance + 1; // Cél elérése

                    queue.Enqueue((nextTile, distance + 1));
                    visited.Add(nextTile);
                }
            }

            return -1; // Ha nincs elérhető út
        }
    }

}
