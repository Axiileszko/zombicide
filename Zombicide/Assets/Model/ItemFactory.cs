using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Model
{
    public class ItemFactory
    {
        private static Random rand = new Random();
        private static List<PimpWeapon> list = new()
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
            list = new()
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
            int roll = rand.Next(0, list.Count);
            var pimp= list[roll];
            list.Remove(pimp);
            return pimp;
        }
    }
}