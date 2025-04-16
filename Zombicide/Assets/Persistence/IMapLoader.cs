using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence
{
    public interface IMapLoader
    {
        public BoardData LoadMap(int mapID);
    }
}
