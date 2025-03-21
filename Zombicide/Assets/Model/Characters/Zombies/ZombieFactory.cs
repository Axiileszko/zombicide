using Model.Characters.Zombies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Zombies
{
    public class ZombieFactory
    {
        public static Zombie CreateZombie(ZombieType z)
        {
            switch (z)
            {
                case ZombieType.WALKER:return new WalkerZombie();
                case ZombieType.RUNNER:return new RunnerZombie();
                case ZombieType.FATTY:return new FattyZombie();
                case ZombieType.ABOMINACOP:return Abominacop.Instance;
                case ZombieType.ABOMINAWILD:return Abominawild.Instance;
                case ZombieType.HOBOMINATION:return Hobomination.Instance;
                case ZombieType.PATIENTZERO:return Patientzero.Instance;
                    default: return null;
            }
        }
        private static List<(ZombieType, int, int, int, int, bool)> spawnOptions = new List<(ZombieType, int, int, int, int, bool)>
        {
            (ZombieType.FATTY,2,3,4,5,true),
            (ZombieType.FATTY,0,1,2,3,true),
            (ZombieType.FATTY,1,1,2,3,false),
            (ZombieType.FATTY,1,2,3,4,true),
            (ZombieType.FATTY,2,3,4,4,false),
            (ZombieType.FATTY,1,2,3,4,true),
            (ZombieType.FATTY,1,2,3,4,false),
            (ZombieType.FATTY,3,4,5,6,false),
            (ZombieType.RUNNER,0,1,2,3,false),
            (ZombieType.RUNNER,1,1,2,3,false),
            (ZombieType.RUNNER,2,3,4,4,false),
            (ZombieType.RUNNER,1,2,3,4,false),
            (ZombieType.RUNNER,1,2,3,4,false),
            (ZombieType.RUNNER,1,2,3,4,false),
            (ZombieType.RUNNER,2,3,4,5,false),
            (ZombieType.RUNNER,3,4,5,6,false),
            (ZombieType.WALKER,1,2,4,6,false),
            (ZombieType.WALKER,1,2,4,6,true),
            (ZombieType.WALKER,2,3,5,7,false),
            (ZombieType.WALKER,2,3,5,7,true),
            (ZombieType.WALKER,2,4,6,8,false),
            (ZombieType.WALKER,2,4,6,8,true),
            (ZombieType.WALKER,3,5,7,9,true),
            (ZombieType.WALKER,3,5,7,9,true),
            (ZombieType.WALKER,3,5,7,9,false),
            (ZombieType.WALKER,3,5,7,9,false),
            (ZombieType.WALKER,4,6,8,10,false),
            (ZombieType.WALKER,4,6,8,10,true),
            (ZombieType.WALKER,4,6,8,10,true),
            (ZombieType.WALKER,4,6,8,10,false),
            (ZombieType.WALKER,6,8,10,12,true),
            (ZombieType.WALKER,6,8,10,12,false),
            (ZombieType.ABOMINACOP,0,1,1,1,false),
            (ZombieType.ABOMINACOP,0,1,1,1,false),
            (ZombieType.ABOMINACOP,1,1,1,1,false),
            (ZombieType.ABOMINACOP,1,1,1,1,false),
            (ZombieType.ABOMINAWILD,0,1,1,1,false),
            (ZombieType.ABOMINAWILD,0,1,1,1,false),
            (ZombieType.ABOMINAWILD,1,1,1,1,false),
            (ZombieType.ABOMINAWILD,1,1,1,1,false),
            (ZombieType.HOBOMINATION,0,1,1,1,false),
            (ZombieType.HOBOMINATION,0,1,1,1,false),
            (ZombieType.HOBOMINATION,1,1,1,1,false),
            (ZombieType.HOBOMINATION,1,1,1,1,false),
            (ZombieType.PATIENTZERO,0,1,1,1,false),
            (ZombieType.PATIENTZERO,0,1,1,1,false),
            (ZombieType.PATIENTZERO,1,1,1,1,false),
            (ZombieType.PATIENTZERO,1,1,1,1,false)
        };
        public static (ZombieType, int, int, int, int, bool) GetSpawnOption()
        {
            Random r=new Random();
            return spawnOptions[r.Next(spawnOptions.Count)];
        }
    }
}
