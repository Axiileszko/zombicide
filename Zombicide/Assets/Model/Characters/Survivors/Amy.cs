using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Amy:Survivor
    {
        private static Amy instance;
        public static Amy Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Amy("Amy", false);
                }
                return instance;
            }
        }
        private Amy(string name, bool isKid):base(name, isKid){ }
    }
}
