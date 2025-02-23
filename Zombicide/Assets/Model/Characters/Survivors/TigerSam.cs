using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class TigerSam:Survivor
    {
        private static TigerSam instance;
        public static TigerSam Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TigerSam("TigerSam", true);
                }
                return instance;
            }
        }
        private TigerSam(string name, bool isKid) : base(name, isKid) { }
    }
}
