using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Zombies
{
    public class Hobomination : AbominationZombie
    {
        private static Hobomination instance;
        public static Hobomination Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Hobomination();
                }
                return instance;
            }
        }
        private Hobomination() { }
    }
}
