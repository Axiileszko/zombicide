using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Zombies
{
    public class FattyZombie : Zombie
    {
        public FattyZombie()
        {
            hp = 2;
            action = 1;
            Priority = 2;
        }
    }
}
