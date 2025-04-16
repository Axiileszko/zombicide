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

namespace Persistence
{

    [Serializable]
    public class BoardData
    {
        public List<MapTileData> tiles;
        public List<TileConnectionData> connections;
        public List<BuildingData> buildings;
        public List<StreetData> streets;
    }
    [Serializable]
    public class MapTileData
    {
        public int id;
        public string type;
        public string extra;
    }
    [Serializable]
    public class BuildingData
    {
        public int id;
        public List<int> rooms;
    }
    [Serializable]
    public class StreetData
    {
        public int id;
        public List<int> tiles;
    }
    [Serializable]
    public class TileConnectionData
    {
        public int from;
        public int to;
        public bool isWall;
        public bool hasDoor;
        public bool isDoorOpen;
    }
    [Serializable]
    public class MissionList
    {
        public List<Mission> missions;
    }
    [Serializable]
    public class Mission
    {
        public int id;
        public BoardData boardData;
    }
    public class MapLoader:IMapLoader
    {
        public BoardData LoadMap(int mapID)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("missions");
            MissionList missionList = JsonUtility.FromJson<MissionList>(jsonFile.text);

            if (missionList == null || missionList.missions.Count == 0)
            {
                throw new Exception("Nincsenek pályák a JSON fájlban!");
            }

            Mission mission = missionList.missions.FirstOrDefault(m => m.id == mapID);
            if (mission == null)
            {
                throw new Exception($"Nem található pálya ezzel az ID-val: {mapID}");
            }
            return mission.boardData;
        }

    }
}
