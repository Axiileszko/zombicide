using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Zombies
{
    public class Abominawild : AbominationZombie
    {
        private static Abominawild instance;
        public static Abominawild Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Abominawild();
                }
                return instance;
            }
        }
        private Abominawild() { }
    }
}
