using Model;
using Model.Board;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using static UnityEngine.Rendering.DebugUI;

namespace Persistence
{
    internal class BoardData
    {
        public List<MapTileData> Tiles { get; set; }
        public List<TileConnectionData> Connections { get; set; }
        public List<BuildingData> Buildings { get; set; }
        public List<StreetData> Streets { get; set; }
    }
    internal class MapTileData
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Extra { get; set; }
    }
    internal class BuildingData
    {
        public int Id { get; set; }
        public List<int> Rooms { get; set; }
    }
    internal class StreetData
    {
        public int Id { get; set; }
        public List<int> Tiles { get; set; }
    }
    internal class TileConnectionData
    {
        public MapTile From { get; set; }
        public MapTile To { get; set; }
        public bool IsWall { get; }
        public bool HasDoor { get; }
        public bool IsDoorOpen { get; set; }
    }
    internal class MissionList
    {
        public List<Mission> Missions { get; set; }
    }
    internal class Mission
    {
        public int Id { get; set; }
        public BoardData BoardData { get; set; }
    }
    public class MapLoader
    {
        private readonly string jsonFilePath;
        public MapLoader(string jsonFilePath)
        {
            this.jsonFilePath = jsonFilePath;
        }
        public Board LoadMap(int mapID)
        {
            //JSON beolvasása fájlból
            string jsonText = File.ReadAllText(jsonFilePath);
            MissionList missionList = JsonConvert.DeserializeObject<MissionList>(jsonText);

            if (missionList == null || missionList.Missions.Count == 0)
            {
                throw new Exception("Nincsenek pályák a JSON fájlban!");
            }

            //Keresd meg a megfelelő ID missiont
            Mission mission = missionList.Missions.FirstOrDefault(m => m.Id == mapID);
            if (mission == null)
            {
                throw new Exception($"Nem található pálya ezzel az ID-val: {mapID}");
            }

            Board board = new Board();
            Dictionary<int, MapTile> tileMap = new Dictionary<int, MapTile>();

            //Mezők létrehozása és hozzáadása a Boardhoz
            foreach (var tileData in mission.BoardData.Tiles)
            {
                TileType? type=null;
                ZombieSpawnType? stype=null;
                switch (tileData.Type)
                {
                    case "R":type=TileType.ROOM; break;
                    case "D":type=TileType.DARKROOM; break;
                    case "S":type=TileType.STREET; break;
                }
                if (tileData.Extra.Contains("Z"))
                {
                    switch (tileData.Extra.Substring(tileData.Extra.IndexOf('Z'),2))
                    {
                        case "ZF": stype = ZombieSpawnType.FIRST; break;
                        case "ZB": stype = ZombieSpawnType.BLUE; break;
                        case "ZG": stype = ZombieSpawnType.GREEN; break;
                        case "ZR": stype = ZombieSpawnType.RED; break;
                    }
                }
                bool hasObj = tileData.Extra.Contains('O');
                bool hasPW = tileData.Extra.Contains('P');
                bool isExit = tileData.Extra.Contains('E');
                bool isStart = tileData.Extra.Contains('S');

                MapTile tile = new MapTile(tileData.Id,type, hasObj,hasPW, isExit,isStart, stype);
                tileMap[tileData.Id] = tile;
                board.AddTile(tile);
            }

            //Kapcsolatok létrehozása
            foreach (var connectionData in mission.BoardData.Connections)
            {
                if (tileMap.ContainsKey(connectionData.From.Id) && tileMap.ContainsKey(connectionData.To.Id))
                {
                    MapTile fromTile = tileMap[connectionData.From.Id];
                    MapTile toTile = tileMap[connectionData.To.Id];

                    TileConnection connection = new TileConnection(toTile, connectionData.IsWall, connectionData.HasDoor, connectionData.IsDoorOpen);
                    fromTile.AddNeighbour(connection);
                }
            }

            //Épületek létrehozása
            foreach (var buildingData in mission.BoardData.Buildings)
            {
                List<MapTile> rooms = new List<MapTile>();
                foreach (var roomId in buildingData.Rooms)
                {
                    if (tileMap.ContainsKey(roomId))
                    {
                        rooms.Add(tileMap[roomId]);
                    }
                }
                board.AddBuilding(new Building(rooms));
            }
            //Utcák létrehozása
            foreach (var streetData in mission.BoardData.Streets)
            {
                List<int> streetTiles = new List<int>();
                foreach (var tileId in streetData.Tiles)
                {
                    if (tileMap.ContainsKey(tileId))
                    {
                        streetTiles.Add(tileId);
                    }
                }
                board.AddStreet(new Street(streetTiles));
            }
            return board;
        }

    }
}
