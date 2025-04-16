using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Persistence
{
    public class DummyMapLoader:IMapLoader
    {
        private readonly BoardData testData;
        public DummyMapLoader()
        {
            testData = new BoardData();
            testData.tiles = new List<MapTileData>();
            testData.connections = new List<TileConnectionData>();
            testData.streets = new List<StreetData>();
            testData.buildings = new List<BuildingData>();

            for (int i = 0; i < 25; i++)
            {
                testData.tiles.Add(new MapTileData());
            }
            for (int i = 0; i < 29; i++)
            {
                testData.connections.Add(new TileConnectionData());
            }
            testData.tiles[0].id = 0; testData.tiles[0].type = "D"; testData.tiles[0].extra = "O";
            testData.tiles[1].id = 1; testData.tiles[1].type = "R"; testData.tiles[1].extra = "P";
            testData.tiles[2].id = 2; testData.tiles[2].type = "D";
            testData.tiles[3].id = 3; testData.tiles[3].type = "D";
            testData.tiles[4].id = 4; testData.tiles[4].type = "R"; testData.tiles[4].extra = "OP";
            testData.tiles[5].id = 5; testData.tiles[5].type = "R"; testData.tiles[5].extra = "O";
            testData.tiles[6].id = 6; testData.tiles[6].type = "R";
            testData.tiles[7].id = 7; testData.tiles[7].type = "D"; testData.tiles[7].extra = "P";
            testData.tiles[8].id = 8; testData.tiles[8].type = "D"; testData.tiles[8].extra = "O";
            testData.tiles[9].id = 9; testData.tiles[9].type = "R"; testData.tiles[9].extra = "P";
            testData.tiles[10].id = 10; testData.tiles[10].type = "S";
            testData.tiles[11].id = 11; testData.tiles[11].type = "S";
            testData.tiles[12].id = 12; testData.tiles[12].type = "S"; testData.tiles[12].extra = "ZR";
            testData.tiles[13].id = 13; testData.tiles[13].type = "S";
            testData.tiles[14].id = 14; testData.tiles[14].type = "S"; testData.tiles[14].extra = "S";
            testData.tiles[15].id = 15; testData.tiles[15].type = "S";
            testData.tiles[16].id = 16; testData.tiles[16].type = "S"; testData.tiles[16].extra = "ZF";
            testData.tiles[17].id = 17; testData.tiles[17].type = "S";
            testData.tiles[18].id = 18; testData.tiles[18].type = "S";
            testData.tiles[19].id = 19; testData.tiles[19].type = "S";
            testData.tiles[20].id = 20; testData.tiles[20].type = "S"; testData.tiles[20].extra = "ZR";
            testData.tiles[21].id = 21; testData.tiles[21].type = "S";
            testData.tiles[22].id = 22; testData.tiles[22].type = "S";
            testData.tiles[23].id = 23; testData.tiles[23].type = "S";
            testData.tiles[24].id = 24; testData.tiles[24].type = "S";

            testData.connections[0].from = 0; testData.connections[0].to = 1; testData.connections[0].isWall = false; testData.connections[0].hasDoor = true; testData.connections[0].isDoorOpen = true;
            testData.connections[1].from = 0; testData.connections[1].to = 2; testData.connections[1].isWall = false; testData.connections[1].hasDoor = true; testData.connections[1].isDoorOpen = true;
            testData.connections[2].from = 1; testData.connections[2].to = 13; testData.connections[2].isWall = false; testData.connections[2].hasDoor = true; testData.connections[2].isDoorOpen = false;
            testData.connections[3].from = 1; testData.connections[3].to = 2; testData.connections[3].isWall = false; testData.connections[3].hasDoor = true; testData.connections[3].isDoorOpen = true;
            testData.connections[4].from = 2; testData.connections[4].to = 16; testData.connections[4].isWall = false; testData.connections[4].hasDoor = true; testData.connections[4].isDoorOpen = false;
            testData.connections[5].from = 3; testData.connections[5].to = 16; testData.connections[5].isWall = false; testData.connections[5].hasDoor = true; testData.connections[5].isDoorOpen = false;
            testData.connections[6].from = 3; testData.connections[6].to = 4; testData.connections[6].isWall = false; testData.connections[6].hasDoor = true; testData.connections[6].isDoorOpen = true;
            testData.connections[7].from = 4; testData.connections[7].to = 17; testData.connections[7].isWall = false; testData.connections[7].hasDoor = true; testData.connections[7].isDoorOpen = false;
            testData.connections[8].from = 9; testData.connections[8].to = 17; testData.connections[8].isWall = false; testData.connections[8].hasDoor = true; testData.connections[8].isDoorOpen = false;
            testData.connections[9].from = 9; testData.connections[9].to = 8; testData.connections[9].isWall = false; testData.connections[9].hasDoor = true; testData.connections[9].isDoorOpen = true;
            testData.connections[10].from = 8; testData.connections[10].to = 7; testData.connections[10].isWall = false; testData.connections[10].hasDoor = true; testData.connections[10].isDoorOpen = true;
            testData.connections[11].from = 7; testData.connections[11].to = 6; testData.connections[11].isWall = false; testData.connections[11].hasDoor = true; testData.connections[11].isDoorOpen = true;
            testData.connections[12].from = 6; testData.connections[12].to = 13; testData.connections[12].isWall = false; testData.connections[12].hasDoor = true; testData.connections[12].isDoorOpen = false;
            testData.connections[13].from = 6; testData.connections[13].to = 5; testData.connections[13].isWall = false; testData.connections[13].hasDoor = true; testData.connections[13].isDoorOpen = true;
            testData.connections[14].from = 5; testData.connections[14].to = 10; testData.connections[14].isWall = false; testData.connections[14].hasDoor = true; testData.connections[14].isDoorOpen = false;
            testData.connections[15].from = 10; testData.connections[15].to = 11; testData.connections[15].isWall = false; testData.connections[15].hasDoor = false;
            testData.connections[16].from = 11; testData.connections[16].to = 12; testData.connections[16].isWall = false; testData.connections[16].hasDoor = false;
            testData.connections[17].from = 12; testData.connections[17].to = 13; testData.connections[17].isWall = false; testData.connections[17].hasDoor = false;
            testData.connections[18].from = 13; testData.connections[18].to = 14; testData.connections[18].isWall = false; testData.connections[18].hasDoor = false;
            testData.connections[19].from = 14; testData.connections[19].to = 15; testData.connections[19].isWall = false; testData.connections[19].hasDoor = false;
            testData.connections[20].from = 14; testData.connections[20].to = 17; testData.connections[20].isWall = false; testData.connections[20].hasDoor = false;
            testData.connections[21].from = 15; testData.connections[21].to = 16; testData.connections[21].isWall = false; testData.connections[21].hasDoor = false;
            testData.connections[22].from = 17; testData.connections[22].to = 20; testData.connections[22].isWall = false; testData.connections[22].hasDoor = false;
            testData.connections[23].from = 18; testData.connections[23].to = 19; testData.connections[23].isWall = false; testData.connections[23].hasDoor = false;
            testData.connections[24].from = 19; testData.connections[24].to = 20; testData.connections[24].isWall = false; testData.connections[24].hasDoor = false;
            testData.connections[25].from = 20; testData.connections[25].to = 21; testData.connections[25].isWall = false; testData.connections[25].hasDoor = false;
            testData.connections[26].from = 21; testData.connections[26].to = 22; testData.connections[26].isWall = false; testData.connections[26].hasDoor = false;
            testData.connections[27].from = 22; testData.connections[27].to = 23; testData.connections[27].isWall = false; testData.connections[27].hasDoor = false;
            testData.connections[28].from = 23; testData.connections[28].to = 24; testData.connections[28].isWall = false; testData.connections[28].hasDoor = false;

            BuildingData b1 = new BuildingData(); b1.id = 0; b1.rooms = new List<int> { 0, 1, 2 };
            BuildingData b2 = new BuildingData(); b2.id = 1; b2.rooms = new List<int> { 3,4};
            BuildingData b3 = new BuildingData(); b3.id = 2; b3.rooms = new List<int> { 5,6,7,8,9 };
            testData.buildings.Add(b1);
            testData.buildings.Add(b2);
            testData.buildings.Add(b3);

            StreetData s1 = new StreetData(); s1.id = 0; s1.tiles = new List<int> { 10,11,12 };
            StreetData s2 = new StreetData(); s2.id = 1; s2.tiles = new List<int> { 12,13,14,17,20 };
            StreetData s3 = new StreetData(); s3.id = 2; s3.tiles = new List<int> { 14,15,16 };
            StreetData s4 = new StreetData(); s4.id = 3; s4.tiles = new List<int> { 18,19,20,21,22 };
            StreetData s5 = new StreetData(); s5.id = 4; s5.tiles = new List<int> { 22,23,24 };
            testData.streets.Add(s1);
            testData.streets.Add(s2);
            testData.streets.Add(s3);
            testData.streets.Add(s4);
            testData.streets.Add(s5);
        }
        public BoardData LoadMap(int mapID)
        {
            return testData;
        }
    }
}
