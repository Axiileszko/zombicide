using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Wanda:Survivor
    {
        private static Wanda instance;
        public static Wanda Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Wanda("Wanda", false);
                }
                return instance;
            }
        }
        private Wanda(string name, bool isKid) : base(name, isKid) { }
    }
}
