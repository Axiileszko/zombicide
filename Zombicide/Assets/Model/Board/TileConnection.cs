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
        public bool IsWall { get; }
        public bool HasDoor { get; }
        public bool IsDoorOpen { get; set; }

        public TileConnection(MapTile destination, bool isWall, bool hasDoor, bool isDoorOpen)
        {
            Destination=destination;
            IsWall = isWall;
            HasDoor = hasDoor;
            IsDoorOpen = isDoorOpen;
        }
    }
}
