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
        private static List<(ZombieType, int, int, int, int)> spawnOptions = new List<(ZombieType, int, int, int, int)>
        {
            (ZombieType.WALKER,2,4,6,8),
            (ZombieType.FATTY,2,4,6,8)//még feltöltöm
        };
        public static (ZombieType, int, int, int, int) GetSpawnOption()
        {
            Random r=new Random();
            return spawnOptions[r.Next(spawnOptions.Count)];
        }
    }
}
