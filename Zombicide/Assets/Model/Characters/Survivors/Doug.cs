using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Doug:Survivor
    {
        private static Doug instance;
        public static Doug Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Doug("Doug", false);
                }
                return instance;
            }
        }
        private Doug(string name, bool isKid) : base(name, isKid) { }
    }
}
