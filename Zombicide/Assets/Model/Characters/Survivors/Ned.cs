using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Ned:Survivor
    {
        private static Ned instance;
        public static Ned Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Ned("Ned", false);
                }
                return instance;
            }
        }
        private Ned(string name, bool isKid) : base(name, isKid) { }
    }
}
