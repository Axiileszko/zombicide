using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Board
{
    public class Building
    {
        public bool IsOpened { get; private set; }
        public List<MapTile> Rooms { get; }
        public Building(List<MapTile> rooms)
        {
            Rooms = rooms;
        }

        public void OpenBuilding()
        {
            IsOpened = true;
        }
    }
}
