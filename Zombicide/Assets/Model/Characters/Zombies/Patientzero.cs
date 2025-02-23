using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Zombies
{
    public class Patientzero : AbominationZombie
    {
        private static Patientzero instance;
        public static Patientzero Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Patientzero();
                }
                return instance;
            }
        }
        private Patientzero() { }
    }
}
