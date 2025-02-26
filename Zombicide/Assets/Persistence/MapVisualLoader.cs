using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Persistence
{
    [System.Serializable]
    public class TileVisualData
    {
        public string tileName;  // A sprite neve
        public Vector3 position; // A pozíció
        public float rotation;   // A forgatás
    }
    [System.Serializable]
    public class MapVisualData
    {
        public int mapID;
        public List<TileVisualData> tiles;
    }
    [System.Serializable]
    public class MapVisualList
    {
        public List<MapVisualData> maps;
    }
    public class MapVisualLoader
    {
        public List<TileVisualData> LoadVisualMap(int mapID)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("mapVisuals");
            List<MapVisualData> maps= JsonUtility.FromJson<MapVisualList>(jsonFile.text).maps;
            return maps.First(x => x.mapID == mapID).tiles;
        }
    }
}
