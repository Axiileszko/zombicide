using Model.Characters.Survivors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Zombies
{
    public class Abominacop : AbominationZombie
    {
        private static Abominacop instance;
        public static Abominacop Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Abominacop();
                }
                return instance;
            }
        }
        private Abominacop(){ }
    }
}
