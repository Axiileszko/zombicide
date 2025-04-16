using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Zombies
{
    public class RunnerZombie:Zombie
    {
        public RunnerZombie() 
        {
            hp = 1;
            action = 2;
            Priority = 4;
        }
    }
}
