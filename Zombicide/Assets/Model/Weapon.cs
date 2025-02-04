using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Weapon
    {
        protected int damage;
        protected int accuracy;
        protected int diceAmount;
        protected int range;
        protected bool isLoud;
        protected bool reloadable;
        protected WeaponType type;
        protected bool canOpenDoors;

        public virtual void SpecialEffect()
        {
            //bef
        }
    }
}
