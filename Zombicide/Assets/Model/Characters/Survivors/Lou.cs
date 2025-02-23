using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Lou:Survivor
    {
        private static Lou instance;
        public static Lou Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Lou("Lou", true);
                }
                return instance;
            }
        }
        private Lou(string name, bool isKid) : base(name, isKid) { }
    }
}
