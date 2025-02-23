using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Board
{
    public class TileConnection
    {
        public MapTile Destination { get; }
        public bool IsWall { get; }  // Ha fal, akkor nem lehet áthaladni
        public bool HasDoor { get; } // Van-e ajtó
        public bool IsDoorOpen { get; set; } // Az ajtó nyitva van-e

        public TileConnection(MapTile destination, bool isWall, bool hasDoor, bool isDoorOpen)
        {
            Destination=destination;
            IsWall = isWall;
            HasDoor = hasDoor;
            IsDoorOpen = isDoorOpen;
        }
    }
}
