using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Board
{
    public class Building
    {
        public List<MapTile> Rooms { get; }
        public Building(List<MapTile> rooms)
        {
            Rooms = rooms;
        }
    }
}
