using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Zombies
{
    public class WalkerZombie:Zombie
    {
        public WalkerZombie() 
        {
            hp = 1;
            action = 1;
            Priority = 2;
        }
    }
}
