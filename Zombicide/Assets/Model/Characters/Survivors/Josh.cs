using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Josh:Survivor
    {
        private static Josh instance;
        public static Josh Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Josh("Josh", false);
                }
                return instance;
            }
        }
        private Josh(string name, bool isKid) : base(name, isKid) { }
    }
}
