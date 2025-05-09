﻿using Model.Characters;
using Model.Characters.Survivors;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Weapon:Item
    {
        #region Properties
        public bool CanBeUsedAsMelee {  get; private set; }
        public int Damage { get; private set; }
        public int Accuracy { get; private set; }
        public int DiceAmount { get; private set; }
        public int Range { get; private set; }
        public char BulletType {  get; private set; }
        public bool IsLoud { get; private set; }
        public bool Reloadable { get; private set; }
        public WeaponType Type { get; private set; }
        public bool CanOpenDoors { get; private set; }
        public bool IsDual { get; private set; }
        #endregion
        public Weapon(ItemName name,char bulletType ,int damage, int accuracy, int diceAmount, int range, bool isLoud, bool reloadable, WeaponType type, bool canOpenDoors, bool canBeMelee, bool isDual) :base(name)
        {
            BulletType = bulletType;
            Damage = damage;
            Accuracy = accuracy;
            DiceAmount = diceAmount;
            Range = range;
            IsLoud = isLoud;
            Reloadable = reloadable;
            Type = type;
            CanOpenDoors = canOpenDoors;
            CanBeUsedAsMelee = canBeMelee;
            IsDual = isDual;
        }
    }
    public abstract class PimpWeapon : Weapon
    {
        public PimpWeapon(ItemName name, char bulletType, int damage, int accuracy, int diceAmount, int range, bool isLoud, bool reloadable, WeaponType type, bool canOpenDoors, bool canBeMelee) : base(name, bulletType, damage, accuracy, diceAmount, range, isLoud, reloadable, type, canOpenDoors,canBeMelee,false) { }
        public class AutShotGun : PimpWeapon
        {
            private static AutShotGun instance;
            public static AutShotGun Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new AutShotGun(ItemName.AUTSHOTGUN,'S',2,4,3,1,true,false,WeaponType.RANGE,false,true);
                    }
                    return instance;
                }
            }
            private AutShotGun(ItemName name, char bulletType, int damage, int accuracy, int diceAmount, int range, bool isLoud, bool reloadable, WeaponType type, bool canOpenDoors, bool canBeMelee) : base(name, bulletType, damage, accuracy, diceAmount, range, isLoud, reloadable, type, canOpenDoors, canBeMelee) { }
        }

        public class EvilTwins : PimpWeapon
        {
            private static EvilTwins instance;
            public static EvilTwins Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new EvilTwins(ItemName.EVILTWINS,'B', 1, 3, 2, 1, true, false, WeaponType.RANGE, false,true);
                    }
                    return instance;
                }
            }
            private EvilTwins(ItemName name, char bulletType, int damage, int accuracy, int diceAmount, int range, bool isLoud, bool reloadable, WeaponType type, bool canOpenDoors, bool canBeMelee) : base(name, bulletType, damage, accuracy, diceAmount, range, isLoud, reloadable, type, canOpenDoors, canBeMelee) { }
        }

        public class GoldenKukri : PimpWeapon
        {
            private static GoldenKukri instance;
            public static GoldenKukri Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new GoldenKukri(ItemName.GOLDENKUKRI,'N', 2, 4, 4, 0, false, false, WeaponType.MELEE, false,true);
                    }
                    return instance;
                }
            }
            private GoldenKukri(ItemName name, char bulletType, int damage, int accuracy, int diceAmount, int range, bool isLoud, bool reloadable, WeaponType type, bool canOpenDoors, bool canBeMelee) : base(name, bulletType, damage, accuracy, diceAmount, range, isLoud, reloadable, type, canOpenDoors, canBeMelee) { }
        }

        public class GoldenAK47 : PimpWeapon
        {
            private static GoldenAK47 instance;
            public static GoldenAK47 Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new GoldenAK47(ItemName.GOLDENAK47,'B', 2, 4, 3, 2, true, false, WeaponType.RANGE, false,false);
                    }
                    return instance;
                }
            }
            private GoldenAK47(ItemName name, char bulletType, int damage, int accuracy, int diceAmount, int range, bool isLoud, bool reloadable, WeaponType type, bool canOpenDoors, bool canBeMelee) : base(name, bulletType, damage, accuracy, diceAmount, range, isLoud, reloadable, type, canOpenDoors, canBeMelee) { }
        }
        public class GunBlade : PimpWeapon
        {
            private static GunBlade instance;
            public static GunBlade Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new GunBlade(ItemName.GUNBLADE,'B', 2, 4, 2, 1, true, false, WeaponType.MELLEANDRANGE, false,true);
                    }
                    return instance;
                }
            }
            private GunBlade(ItemName name, char bulletType, int damage, int accuracy, int diceAmount, int range, bool isLoud, bool reloadable, WeaponType type, bool canOpenDoors, bool canBeMelee) : base(name, bulletType, damage, accuracy, diceAmount, range, isLoud, reloadable, type, canOpenDoors, canBeMelee) { }
        }
        public class MasShotGun : PimpWeapon
        {
            private static MasShotGun instance;
            public static MasShotGun Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new MasShotGun(ItemName.MASSHOTGUN, 'S', 2, 3, 2, 1, true, true, WeaponType.MELLEANDRANGE, false,true);
                    }
                    return instance;
                }
            }
            private MasShotGun(ItemName name, char bulletType, int damage, int accuracy, int diceAmount, int range, bool isLoud, bool reloadable, WeaponType type, bool canOpenDoors, bool canBeMelee) : base(name, bulletType, damage, accuracy, diceAmount, range, isLoud, reloadable, type, canOpenDoors, canBeMelee) { }
        }
        public class MilitarySniperRifle : PimpWeapon
        {
            private static MilitarySniperRifle instance;
            public static MilitarySniperRifle Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new MilitarySniperRifle(ItemName.MILITARYSNIPERRIFLE, 'B', 2, 3, 2, 4, true, false, WeaponType.RANGE, false,false);
                    }
                    return instance;
                }
            }
            private MilitarySniperRifle(ItemName name, char bulletType, int damage, int accuracy, int diceAmount, int range, bool isLoud, bool reloadable, WeaponType type, bool canOpenDoors, bool canBeMelee) : base(name, bulletType, damage, accuracy, diceAmount, range, isLoud, reloadable, type, canOpenDoors, canBeMelee) { }
        }
        public class NailBat : PimpWeapon
        {
            private static NailBat instance;
            public static NailBat Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new NailBat(ItemName.NAILBAT, 'N', 2, 3, 2, 0, false, false, WeaponType.MELEE, false,true);
                    }
                    return instance;
                }
            }
            private NailBat(ItemName name, char bulletType, int damage, int accuracy, int diceAmount, int range, bool isLoud, bool reloadable, WeaponType type, bool canOpenDoors, bool canBeMelee) : base(name, bulletType, damage, accuracy, diceAmount, range, isLoud, reloadable, type, canOpenDoors, canBeMelee) { }
        }
        public class Zantetsuken : PimpWeapon
        {
            private static Zantetsuken instance;
            public static Zantetsuken Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new Zantetsuken(ItemName.ZANTETSUKEN, 'N', 1, 4, 5, 0, false, false, WeaponType.MELEE, false,true);
                    }
                    return instance;
                }
            }
            private Zantetsuken(ItemName name, char bulletType, int damage, int accuracy, int diceAmount, int range, bool isLoud, bool reloadable, WeaponType type, bool canOpenDoors, bool canBeMelee) : base(name,bulletType, damage, accuracy, diceAmount, range, isLoud, reloadable, type, canOpenDoors, canBeMelee) { }
        }
    }

}
