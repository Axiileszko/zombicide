using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Lili:Survivor
    {
        private static Lili instance;
        public static Lili Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Lili("Lili", true);
                }
                return instance;
            }
        }
        private Lili(string name, bool isKid) : base(name, isKid) { }
    }
}
