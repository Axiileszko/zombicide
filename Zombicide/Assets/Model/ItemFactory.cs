using Model.Characters.Survivors;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Model
{
    public class ItemFactory
    {
        private static Random rand = new Random();
        private static List<PimpWeapon> pimpWeapons = new()
        {
            PimpWeapon.AutShotGun.Instance,
                PimpWeapon.EvilTwins.Instance,
                PimpWeapon.GoldenAK47.Instance,
                PimpWeapon.GoldenKukri.Instance,
                PimpWeapon.GunBlade.Instance,
                PimpWeapon.MasShotGun.Instance,
                PimpWeapon.MilitarySniperRifle.Instance,
                PimpWeapon.NailBat.Instance,
                PimpWeapon.Zantetsuken.Instance,
        };

        private static List<Weapon> genericWeapons = new()
        {
             new Weapon(ItemName.AXE,2,4,1,0,false,false,WeaponType.MELEE,true,true),
             new Weapon(ItemName.BASEBALLBAT,1,3,2,0,false,false,WeaponType.MELEE,false,true),
             new Weapon(ItemName.CROWBAR,1,4,1,0,false,false,WeaponType.MELEE,true,true),
             new Weapon(ItemName.PISTOL,1,3,1,1,true,false,WeaponType.RANGE,false,true),
             new Weapon(ItemName.PISTOL,1,3,1,1,true,false,WeaponType.RANGE,false,true),
             new Weapon(ItemName.PISTOL,1,3,1,1,true,false,WeaponType.RANGE,false,true),
        };
        public static List<Item> CreateItems()
        {
            List<Item> list = new()
            {
                new Weapon(ItemName.AXE,2,4,1,0,false,false,WeaponType.MELEE,true,true),
                new Weapon(ItemName.BASEBALLBAT,1,3,2,0,false,false,WeaponType.MELEE,false,true),
                new Weapon(ItemName.CHAINSAW,2,5,5,0,true,false,WeaponType.MELEE,true,true),
                new Weapon(ItemName.CROWBAR,1,4,1,0,false,false,WeaponType.MELEE,true,true),
                new Weapon(ItemName.KATANA,1,4,2,0,false,false,WeaponType.MELEE,false,true),
                new Weapon(ItemName.KUKRI,2,4,2,0,false,false,WeaponType.MELEE,false,true),
                new Weapon(ItemName.MACHETE,2,3,1,0,false,false,WeaponType.MELEE,false,true),
                new Weapon(ItemName.MOLOTOV,-1,-1,-1,1,true,false,WeaponType.BOMB,false,false),
                new Weapon(ItemName.PISTOL,1,3,1,1,true,false,WeaponType.RANGE,false,true),
                new Weapon(ItemName.SAWEDOFF,1,3,2,1,true,true,WeaponType.RANGE,false,true),
                new Weapon(ItemName.SHOTGUN,2,4,2,1,true,false,WeaponType.RANGE,false, true),
                new Weapon(ItemName.SNIPERRIFLE,2,3,1,3,false,false,WeaponType.RANGE,false,false),
                new Weapon(ItemName.SUBMG,1,5,3,1,true,false,WeaponType.RANGE,false,true),
                new Consumable(ItemName.CANNEDFOOD,3),
                new Consumable(ItemName.PLENTYOFBULLETS,0),
                new Consumable(ItemName.PLENTYOFSHELLS,0),
                new Consumable(ItemName.FLASHLIGHT,0),
                new Consumable(ItemName.RICE,3),
                new Consumable(ItemName.WATER,3)
            };
            return list;
        }

        public static void CreatePimpWeapons()
        {
            pimpWeapons = new()
            {
                PimpWeapon.AutShotGun.Instance,
                PimpWeapon.EvilTwins.Instance,
                PimpWeapon.GoldenAK47.Instance,
                PimpWeapon.GoldenKukri.Instance,
                PimpWeapon.GunBlade.Instance,
                PimpWeapon.MasShotGun.Instance,
                PimpWeapon.MilitarySniperRifle.Instance,
                PimpWeapon.NailBat.Instance,
                PimpWeapon.Zantetsuken.Instance,
            };
        }

        public static PimpWeapon GetPimpWeapon()
        {
            int roll = rand.Next(0, pimpWeapons.Count);
            var pimp= pimpWeapons[roll];
            pimpWeapons.Remove(pimp);
            return pimp;
        }
        public static void CreateGenericWeapons()
        {
            genericWeapons = new()
            {
             new Weapon(ItemName.AXE,2,4,1,0,false,false,WeaponType.MELEE,true,true),
             new Weapon(ItemName.BASEBALLBAT,1,3,2,0,false,false,WeaponType.MELEE,false,true),
             new Weapon(ItemName.CROWBAR,1,4,1,0,false,false,WeaponType.MELEE,true,true),
             new Weapon(ItemName.PISTOL,1,3,1,1,true,false,WeaponType.RANGE,false,true),
             new Weapon(ItemName.PISTOL,1,3,1,1,true,false,WeaponType.RANGE,false,true),
             new Weapon(ItemName.PISTOL,1,3,1,1,true,false,WeaponType.RANGE,false,true),
            };
        }
        public static Weapon GetGenericWeapon()
        {
            int roll = rand.Next(0, genericWeapons.Count);
            var gen = genericWeapons[roll];
            genericWeapons.Remove(gen);
            return gen;
        }
        public static Weapon GetGenericWeaponByName(ItemName s)
        {
            switch (s)
            {
                case ItemName.AXE:return new Weapon(ItemName.AXE, 2, 4, 1, 0, false, false, WeaponType.MELEE, true, true);
                case ItemName.BASEBALLBAT:return new Weapon(ItemName.BASEBALLBAT, 1, 3, 2, 0, false, false, WeaponType.MELEE, false, true);
                case ItemName.CROWBAR:return new Weapon(ItemName.CROWBAR, 1, 4, 1, 0, false, false, WeaponType.MELEE, true, true);
                case ItemName.PISTOL:return new Weapon(ItemName.PISTOL, 1, 3, 1, 1, true, false, WeaponType.RANGE, false, true);
                default: return null;
            }
        }

        public static Item GetItemByName(ItemName s) 
        { switch (s)
            {
                case ItemName.AUTSHOTGUN: return PimpWeapon.AutShotGun.Instance;
                case ItemName.EVILTWINS: return PimpWeapon.EvilTwins.Instance;
                case ItemName.GOLDENAK47: return PimpWeapon.GoldenAK47.Instance;
                case ItemName.GOLDENKUKRI: return PimpWeapon.GoldenKukri.Instance;
                case ItemName.GUNBLADE: return PimpWeapon.GunBlade.Instance;
                case ItemName.MASSHOTGUN: return PimpWeapon.MasShotGun.Instance;
                case ItemName.MILITARYSNIPERRIFLE: return PimpWeapon.MilitarySniperRifle.Instance;
                case ItemName.NAILBAT: return PimpWeapon.NailBat.Instance;
                case ItemName.ZANTETSUKEN: return PimpWeapon.Zantetsuken.Instance;
                case ItemName.AXE : return new Weapon(ItemName.AXE, 2, 4, 1, 0, false, false, WeaponType.MELEE, true, true);
                case ItemName.BASEBALLBAT : return new Weapon(ItemName.BASEBALLBAT, 1, 3, 2, 0, false, false, WeaponType.MELEE, false, true);
                case ItemName.CHAINSAW : return new Weapon(ItemName.CHAINSAW, 2, 5, 5, 0, true, false, WeaponType.MELEE, true, true);
                case ItemName.CROWBAR : return new Weapon(ItemName.CROWBAR, 1, 4, 1, 0, false, false, WeaponType.MELEE, true, true);
                case ItemName.KATANA : return new Weapon(ItemName.KATANA, 1, 4, 2, 0, false, false, WeaponType.MELEE, false, true);
                case ItemName.KUKRI : return new Weapon(ItemName.KUKRI, 2, 4, 2, 0, false, false, WeaponType.MELEE, false, true);
                case ItemName.MACHETE : return new Weapon(ItemName.MACHETE, 2, 3, 1, 0, false, false, WeaponType.MELEE, false, true);
                case ItemName.MOLOTOV : return new Weapon(ItemName.MOLOTOV, -1, -1, -1, 1, true, false, WeaponType.BOMB, false, false);
                case ItemName.PISTOL : return new Weapon(ItemName.PISTOL, 1, 3, 1, 1, true, false, WeaponType.RANGE, false, true);
                case ItemName.SAWEDOFF : return new Weapon(ItemName.SAWEDOFF, 1, 3, 2, 1, true, true, WeaponType.RANGE, false, true);
                case ItemName.SHOTGUN : return new Weapon(ItemName.SHOTGUN, 2, 4, 2, 1, true, false, WeaponType.RANGE, false, true);
                case ItemName.SNIPERRIFLE : return new Weapon(ItemName.SNIPERRIFLE, 2, 3, 1, 3, false, false, WeaponType.RANGE, false, false);
                case ItemName.SUBMG: return new Weapon(ItemName.SUBMG, 1, 5, 3, 1, true, false, WeaponType.RANGE, false, true);
                case ItemName.CANNEDFOOD : return new Consumable(ItemName.CANNEDFOOD, 3);
                case ItemName.PLENTYOFBULLETS : return new Consumable(ItemName.PLENTYOFBULLETS, 0);
                case ItemName.PLENTYOFSHELLS : return new Consumable(ItemName.PLENTYOFSHELLS, 0);
                case ItemName.FLASHLIGHT : return new Consumable(ItemName.FLASHLIGHT, 0);
                case ItemName.RICE : return new Consumable(ItemName.RICE, 3);
                case ItemName.WATER : return new Consumable(ItemName.WATER, 3);
                default: return null;
            } 
        }
    }
}