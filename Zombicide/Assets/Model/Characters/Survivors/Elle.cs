using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Elle:Survivor
    {
        private static Elle instance;
        public static Elle Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Elle("Elle", true);
                }
                return instance;
            }
        }
        private Elle(string name, bool isKid) : base(name, isKid) { }
    }
}
