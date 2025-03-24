using Model.Board;
using Model.Characters.Zombies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Matchmaker.Models;

namespace Model.Characters.Survivors
{
    public enum Trait
    {
        SLIPPERY, MEDIC, LUCKY, SNIPER, SPRINT,JUMP,CHARGE, SHOVE, AMBIDEXTROUS, 
        P1A, //+1 action
        P1FCA,
        P1FMA,
        P1FRA, //+1 free combat/melee/range/move action
        P1FMOA,
        P1DC, //+1 die combat/range/melee
        P1DR,
        P1DM,
        P1DMR,
        P1DMM, //+1 damage melee/range
        P1TDRC,
        P1TDRR,
        P1TDRM, //+1 to dice roll combat/melee/range
        ROLL6, //roll 6 +1 die combat
    }
    public abstract class Survivor : Character
    {
        protected string name;
        protected int aPoints;
        protected bool isKid;
        protected int level = 0;
        protected List<Item> backpack = new List<Item>();
        protected Item rightHand;
        protected Item leftHand;
        protected bool isDead;
        public event EventHandler<string> SurvivorDied;
        public int ObjectiveCount {  get; protected set; }
        public bool FinishedRound { get; set; }
        public bool StartedRound { get; set; }
        public bool SearchedAlready { get; set; }
        public bool SlipperyMovedAlready { get; set; }
        public bool LeftExit {  get; private set; }
        public List<Trait> Traits { get; protected set; } = new List<Trait>();
        public bool IsDead { get { return isDead; } set { isDead = value; if (value) OnSurvivorDied(); } }
        public int APoints { get {  return aPoints; } }
        public string Name { get { return name; } }
        public Item RightHand { get { return rightHand; } }
        public Item LeftHand { get { return leftHand; } }
        public List<Item> BackPack { get { return backpack; } }
        public Dictionary<string, GameAction> Actions { get; protected set; } = new Dictionary<string, GameAction>();
        public Dictionary<string, GameAction> FreeActions { get; protected set; } = new Dictionary<string, GameAction>();
        public Survivor(string name, bool isKid)
        {
            this.name = name;
            this.isKid = isKid;
            Reset();
        }
        public int CanUpgradeTo()
        {
            if (APoints >= 43 && level==2)
            {
                return 3;
            }
            else if (APoints >= 19 && level == 1)
            {
                return 2;
            }
            else if (APoints >= 7 && level == 0)
            {
                return 1;
            }
            else
                return 0;
        }
        public abstract List<string> GetTraitUpgrades(int level);
        public abstract void UpgradeTo(int level, int option);
        public bool CanOpenDoorOnTile()
        {
            if(model.GetZombiesInPriorityOrderOnTile(CurrentTile).ToList().Count > 0) return false;
            if (CurrentTile.Neighbours.Count(x=>x.HasDoor && !x.IsDoorOpen)==0) return false;
            bool rightH = false;
            bool leftH = false;
            if (rightHand != null && rightHand is Weapon weapon && weapon.CanOpenDoors) rightH = true;
            if (leftHand != null && leftHand is Weapon weapon2 && weapon2.CanOpenDoors) leftH = true;
            if (!rightH && !leftH) return false;
            return true;
        }
        public bool HasPlentyOfShells()
        {
            bool l=false;
            if(leftHand!=null && leftHand.Name==ItemName.PLENTYOFSHELLS)
                l = true;
            else if(rightHand != null && rightHand.Name == ItemName.PLENTYOFSHELLS)
                l = true;
            foreach (var item in backpack)
            {
                if (item.Name == ItemName.PLENTYOFSHELLS) l = true;
            }
            return l;
        }
        public bool HasPlentyOfBullets()
        {
            bool l = false;
            if (leftHand != null && leftHand.Name == ItemName.PLENTYOFBULLETS)
                l = true;
            else if (rightHand != null && rightHand.Name == ItemName.PLENTYOFBULLETS)
                l = true;
            foreach (var item in backpack)
            {
                if (item.Name == ItemName.PLENTYOFBULLETS) l = true;
            }
            return l;
        }
        public void NewRound()
        {
            UsedAction = 0;
            StartedRound = false;
            SlipperyMovedAlready = false;
            SearchedAlready = false;
            FinishedRound = false;
        }
        public List<string> GetAvailableAttacksOnTile(MapTile targetTile)
        {
            List<string> result = new List<string>();
            bool isLeftMelee = false; bool isRightMelee = false; bool isLeftRange = false; bool isRightRange = false;
            if (LeftHand != null && LeftHand is Weapon leftW)
            {
                if (leftW.CanBeUsedAsMelee)
                    isLeftMelee = true;
                if (leftW.Range >= 1)
                    isLeftRange = true;
            }
            if (RightHand != null && RightHand is Weapon rightW)
            {
                if (rightW.CanBeUsedAsMelee)
                    isRightMelee = true;
                if (rightW.Range >= 1)
                    isRightRange = true;
            }
            if ((isLeftMelee || isRightMelee) && CanTargetTile(targetTile))
                result.Add("Melee");
            if (CanTargetTile(targetTile))
            {
                int distance = 1;
                if (CurrentTile.Type == TileType.STREET && targetTile.Type==TileType.STREET)
                {
                    var street = model.Board.GetStreetByTiles(CurrentTile.Id, targetTile.Id);
                    distance = Math.Abs(street.Tiles.IndexOf(CurrentTile.Id) - street.Tiles.IndexOf(targetTile.Id));
                }
                if (targetTile == CurrentTile)
                {
                    if ((isLeftRange && ((Weapon)leftHand).CanBeUsedAsMelee || (isRightRange && ((Weapon)rightHand).CanBeUsedAsMelee))) result.Add("Range");
                }
                else
                    if ((isLeftRange && distance <= ((Weapon)leftHand).Range) || (isRightRange && distance <= ((Weapon)leftHand).Range)) result.Add("Range");
            }
            return result;
        }
        public abstract void SetActions(MapTile tileClicked);
        public abstract void SetFreeActions();
        public void Move(MapTile targetTile)
        {
            MoveTo(targetTile);
        }
        public void SlipperyMove(MapTile targetTile)
        {
            if (Traits.Contains(Trait.SLIPPERY))
                MoveTo(targetTile);
            SlipperyMovedAlready=true;
        }
        public void Attack(MapTile targetTile, Weapon weapon, bool isMelee, List<int> throws, List<string>? newPriority)
        {
            if (weapon == null || !CanTargetTile(targetTile)) return;
            if (!isMelee)
            {
                int distance = 1;
                if (CurrentTile.Type == TileType.STREET && targetTile.Type == TileType.STREET)
                {
                    var street = model.Board.GetStreetByTiles(CurrentTile.Id, targetTile.Id);
                    distance = Math.Abs(street.Tiles.IndexOf(CurrentTile.Id) - street.Tiles.IndexOf(targetTile.Id));
                }
                if (distance > weapon.Range) return;
            }

            // Ha hangos a fegyver zajt generálunk
            if (weapon.IsLoud)
                CurrentTile.NoiseCounter++;

            List<Zombie> zombies = model.GetZombiesInPriorityOrderOnTile(targetTile);
            if (newPriority != null && newPriority.Count>0)
                zombies = model.SortZombiesByNewPriority(zombies, newPriority);

            List<Survivor> survivors = model.GetSurvivorsOnTile(targetTile);

            // Dice traits
            if (Traits.Contains(Trait.P1TDRC))
            {
                for (int i = 0; i < throws.Count; i++)
                    throws[i]++;
            }
            if (Traits.Contains(Trait.P1TDRM) && isMelee)
            {
                for (int i = 0; i < throws.Count; i++)
                    throws[i]++;
            }
            if (Traits.Contains(Trait.P1TDRR) && !isMelee)
            {
                for (int i = 0; i < throws.Count; i++)
                    throws[i]++;
            }

            // Molotov
            if (weapon.Type == WeaponType.BOMB)
            {
                if (model.Abomination!=null && model.Abomination is Abominawild && model.Abomination.CurrentTile==targetTile)
                {
                    model.RemoveZombie(model.Abomination);
                    return;
                }
                foreach (var item in zombies)
                {
                    model.RemoveZombie(item);
                    if (item is AbominationZombie)
                        aPoints += 5;
                    else
                        aPoints++;
                }
                foreach (var item in survivors)
                {
                    SurvivorFactory.GetSurvivorByName(item.name);IsDead =true;
                }
                return;
            }

            // Dobások végrehajtása
            int successfulHits = 0;
            for (int i = 0; i < weapon.DiceAmount; i++)
            {
                if (throws[i] >= weapon.Accuracy)
                    successfulHits++;
            }
            int damageAmount = throws.Count - successfulHits;

            // Zombik támadása prioritási sorrendben
            while (successfulHits > 0 && zombies.Count > 0)
            {
                Zombie targetZombie = zombies.First();

                int damage = weapon.Damage;

                // Damage trait
                if (Traits.Contains(Trait.P1DMM) && isMelee)
                    damage++;
                if (Traits.Contains(Trait.P1DMR) && !isMelee)
                    damage++;

                if (targetZombie.TakeDamage(damage))
                {
                    // Megöljük a zombit
                    model.RemoveZombie(targetZombie);
                    zombies.RemoveAt(0);
                    successfulHits--;
                    if (targetZombie is AbominationZombie)
                        aPoints += 5;
                    else
                        aPoints++;
                }
                else
                {
                    successfulHits--; // Elpazarolt támadás
                }
            }
            // Ha volt túlélő a mezőn akkor sebződik
            if (damageAmount>0 && survivors.Count>0 && !isMelee && weapon.Name != ItemName.MILITARYSNIPERRIFLE && weapon.Name != ItemName.SNIPERRIFLE && !Traits.Contains(Trait.SNIPER))
            {
                while (damageAmount>0)
                {
                    foreach (var item in survivors)
                    {
                        if (item.Name != Name)
                        {
                            Survivor s = SurvivorFactory.GetSurvivorByName(item.name);
                            s.TakeDamage(1);
                            damageAmount--;
                            if (damageAmount == 0) break;
                        }
                    }
                }
            }
        }
        public override bool TakeDamage(int amount)
        {
            hp =Math.Max(0,hp-amount);
            if (hp <= 0)
            {
                IsDead = true;
                return true;//true ha meghalt a karakter
            }
            return false;
        }
        private bool CanTargetTile(MapTile targetTile)
        {
            if(CurrentTile.Id == targetTile.Id)
                return true;
            if (CurrentTile.Type == TileType.STREET && targetTile.Type == TileType.STREET)
            {
                var street=model.Board.GetStreetByTiles(CurrentTile.Id, targetTile.Id);
                if (street == null) return false;
            }
            else 
            { 
                if(!CurrentTile.Neighbours.Select(x=>x.Destination.Id).Contains(targetTile.Id)) return false;
                if(!model.Board.CanMove(CurrentTile, targetTile)) return false;
            }
            return true;
        }
        public void PickUpObjective() 
        { 
            CurrentTile.PickUpObjective();//lehet kezdünk még valamit az object refel amit visszaadna
            aPoints += 5;
            ObjectiveCount++;
        }
        public void ThrowAway(Item item)
        {
            if (item.Name==ItemName.RICE || item.Name == ItemName.WATER || item.Name == ItemName.CANNEDFOOD)
            {
                aPoints += 3;
            }
        }
        public void Reset()
        {
            StartedRound = false;
            FinishedRound = false;
            SearchedAlready = false;
            SlipperyMovedAlready = false;
            LeftExit=false;
            UsedAction = 0;
            ObjectiveCount = 0;
            level = 0;
            aPoints = 0;
            backpack = new List<Item>();
            if (isKid)
                Traits.Add(Trait.SLIPPERY);
            rightHand = null;
            leftHand = null;
            if (isKid)
                hp = 2;
            else
                hp = 3;
            action = 3;
        }
        //public void SwitchItems(Survivor survivor, Item item, bool gives)
        //{
        //    //másiknak adunk vagy másiktól kérünk
        //    if (survivor.CurrentTile == CurrentTile)
        //    {
        //        if(gives)
        //        {
        //            //mi adunk
        //            var itemToGive = ThrowAway(item);
        //            survivor.RecieveItem(itemToGive);
        //        }
        //        else
        //        {
        //            //mi kapunk
        //            var itemToGet = survivor.ThrowAway(item);
        //            RecieveItem(itemToGet);
        //        }
        //    }
        //}

        public void PutIntoBackpack(List<Item> items)
        {
            if (items.Count <= 3)
            {
                backpack = items;
            }
        }
        public void PutIntoHand(bool isRightHand, Item item)
        {
            if (isRightHand)
            {
                rightHand = item;
            }
            else
            {
                leftHand = item;
            }
        }
        public void Skip()
        {
            FinishedRound=true;
        }
        public bool HasFlashLight()
        {
            return (rightHand != null && rightHand.Name == ItemName.FLASHLIGHT) || (leftHand != null && leftHand.Name == ItemName.FLASHLIGHT) || backpack.Any(x => x.Name == ItemName.FLASHLIGHT);
        }
        public void PickGenericWeapon(Weapon weapon)
        {
            rightHand = weapon;
        }
        public void OnUsedAction(string action, string? isMelee)
        {
            if(isMelee!= null && FreeActions.Keys.Any(x=>x.EndsWith("Attack")))
            {
                if (isMelee=="True" && FreeActions.Keys.Contains("Melee Attack"))
                    FreeActions.Remove("Melee Attack");
                else if (isMelee == "False" && FreeActions.Keys.Contains("Range Attack"))
                    FreeActions.Remove("Range Attack");
                else
                    FreeActions.Remove("Attack");
            }
            else if (FreeActions.Keys.Contains(action))
                FreeActions.Remove(action);
            else
                UsedAction += Actions[action].Cost;
            if (UsedAction == this.action)
            {
                FinishedRound = true;
            }
        }
        public void LeaveThroughExit()
        {
            LeftExit = true;
            FinishedRound= true;
        }
        private void OnSurvivorDied()
        {
            SurvivorDied.Invoke(this, name);
        }
    }
}
