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
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Persistence
{

    [Serializable]
    internal class BoardData
    {
        public List<MapTileData> tiles;
        public List<TileConnectionData> connections;
        public List<BuildingData> buildings;
        public List<StreetData> streets;
    }
    [Serializable]
    internal class MapTileData
    {
        public int id;
        public string type;
        public string extra;
    }
    [Serializable]
    internal class BuildingData
    {
        public int id;
        public List<int> rooms;
    }
    [Serializable]
    internal class StreetData
    {
        public int id;
        public List<int> tiles;
    }
    [Serializable]
    internal class TileConnectionData
    {
        public int from;
        public int to;
        public bool isWall;
        public bool hasDoor;
        public bool isDoorOpen;
    }
    [Serializable]
    internal class MissionList
    {
        public List<Mission> missions;
    }
    [Serializable]
    internal class Mission
    {
        public int id;
        public BoardData boardData;
    }
    public class MapLoader
    {
        //private readonly string jsonFilePath;
        //public MapLoader(string jsonFilePath)
        //{
        //    this.jsonFilePath = jsonFilePath;
        //}
        public Board LoadMap(int mapID)
        {
            ////JSON beolvasása fájlból
            //string jsonText = File.ReadAllText(jsonFilePath);
            //MissionList missionList = JsonConvert.DeserializeObject<MissionList>(jsonText);

            TextAsset jsonFile = Resources.Load<TextAsset>("missions");
            MissionList missionList = JsonUtility.FromJson<MissionList>(jsonFile.text);

            if (missionList == null || missionList.missions.Count == 0)
            {
                throw new Exception("Nincsenek pályák a JSON fájlban!");
            }

            //Keresd meg a megfelelő ID missiont
            Mission mission = missionList.missions.FirstOrDefault(m => m.id == mapID);
            if (mission == null)
            {
                throw new Exception($"Nem található pálya ezzel az ID-val: {mapID}");
            }

            Board board = new Board();
            Dictionary<int, MapTile> tileMap = new Dictionary<int, MapTile>();

            //Mezők létrehozása és hozzáadása a Boardhoz
            foreach (var tileData in mission.boardData.tiles)
            {
                TileType? type=null;
                ZombieSpawnType? stype=null;
                switch (tileData.type)
                {
                    case "R":type=TileType.ROOM; break;
                    case "D":type=TileType.DARKROOM; break;
                    case "S":type=TileType.STREET; break;
                }
                if (tileData.extra != null && tileData.extra.Contains("Z"))
                {
                    switch (tileData.extra.Substring(tileData.extra.IndexOf('Z'),2))
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
                    hasObj=false;hasPW=false;isExit=false;isStart=false;
                }
                MapTile tile = new MapTile(tileData.id,type, hasObj,hasPW, isExit,isStart, stype);
                tileMap[tileData.id] = tile;
                board.AddTile(tile);
            }

            //Kapcsolatok létrehozása
            foreach (var connectionData in mission.boardData.connections)
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

            //Épületek létrehozása
            foreach (var buildingData in mission.boardData.buildings)
            {
                List<MapTile> rooms = new List<MapTile>();
                foreach (var roomId in buildingData.rooms)
                {
                    if (tileMap.ContainsKey(roomId))
                    {
                        rooms.Add(tileMap[roomId]);
                    }
                }
                board.AddBuilding(new Building(rooms));
            }
            //Utcák létrehozása
            foreach (var streetData in mission.boardData.streets)
            {
                List<int> streetTiles = new List<int>();
                foreach (var tileId in streetData.tiles)
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
