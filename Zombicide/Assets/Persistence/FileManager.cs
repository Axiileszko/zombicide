using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Diagnostics;

namespace Persistence
{
    [Serializable]
    public class MapData
    {
        public int id;
        public string name;
        public string image;
        public string objectives;
        public string difficulty;
        public string rules;
    }

    [Serializable]
    public class CharacterData
    {
        public string name;
        public string image;
    }

    [System.Serializable]
    public class MapList
    {
        public List<MapData> maps;
    }

    [System.Serializable]
    public class CharacterList
    {
        public List<CharacterData> characters;
    }
    public class FileManager
    {
        private static FileManager instance;
        public static FileManager Instance { 
            get 
            {
                if (instance==null)
                {
                    instance = new FileManager();
                }
                return instance;
            } 
        }

        private FileManager() { }
        public List<MapData> LoadMaps()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("maps");
            MapList mapList = JsonUtility.FromJson<MapList>(jsonFile.text);
            return mapList.maps;
        }

        public List<CharacterData> LoadCharacters()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("characters");
            CharacterList characterList = JsonUtility.FromJson<CharacterList>(jsonFile.text);
            return characterList.characters;
        }
    }
}
