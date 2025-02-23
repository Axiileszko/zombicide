using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Odin:Survivor
    {
        private static Odin instance;
        public static Odin Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Odin("Odin", true);
                }
                return instance;
            }
        }
        private Odin(string name, bool isKid) : base(name, isKid) { }
    }
}
