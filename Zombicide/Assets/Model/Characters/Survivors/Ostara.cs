using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Ostara:Survivor
    {
        private static Ostara instance;
        public static Ostara Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Ostara("Ostara", true);
                }
                return instance;
            }
        }
        private Ostara(string name, bool isKid) : base(name, isKid) { }
    }
}
