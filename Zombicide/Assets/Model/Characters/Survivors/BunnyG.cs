using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class BunnyG:Survivor
    {
        private static BunnyG instance;
        public static BunnyG Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BunnyG("BunnyG", true);
                }
                return instance;
            }
        }
        private BunnyG(string name, bool isKid) : base(name, isKid) { }
    }
}
