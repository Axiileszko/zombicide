using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Board
{
    public class Street
    {
        public List<int> Tiles { get; }
        public Street(List<int> tiles)
        {
            Tiles = tiles;
        }
    }
}
