using Model.Board;
using Model.Characters.Zombies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public enum Trait
    {
        SLIPPERY, MEDIC, LUCKY, MATHCINGSET, SNIPER, SPRINT,JUMP,CHARGE, SHOVE, MULTIPLESEARCH, AMBIDEXTROUS
    }
    public abstract class Survivor : Character
    {
        protected string name;
        protected int aPoints;
        protected bool isKid;
        protected List<Item> backpack = new List<Item>();
        protected Item rightHand;
        protected Item leftHand;
        public bool FinishedRound { get; set; }
        public bool StartedRound { get; set; }
        public bool SearchedAlready { get; set; }
        public bool SlipperyMovedAlready { get; set; }
        public List<Trait> Traits { get; protected set; } = new List<Trait>();
        public bool IsDead { get; set; }
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
        public bool CanOpenDoorOnTile()
        {
            if (!CurrentTile.Neighbours.Select(x => x.IsDoorOpen).ToList().Contains(false)) return false;
            bool rightH = false;
            bool leftH = false;
            if (rightHand != null && rightHand is Weapon weapon && weapon.CanOpenDoors) rightH = true;
            if (leftHand != null && leftHand is Weapon weapon2 && weapon2.CanOpenDoors) leftH = true;
            if (!rightH && !leftH) return false;
            return true;
        }
        public void NewRound()
        {
            UsedAction = 0;
            StartedRound = false;
            SlipperyMovedAlready = false;
            SearchedAlready = false;
            FinishedRound = false;
        }
        public List<string> GetAvailableAttacks()
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
            if (isLeftMelee || isRightMelee)
                result.Add("Melee");
            if (isLeftRange || isRightRange)
                result.Add("Range");
            return (result);
        }
        public abstract void SetActions(MapTile tileClicked);
        public abstract void SetFreeActions();
        public virtual void Move(MapTile targetTile)
        {
            if(model.GetZombiesInPriorityOrderOnTile(targetTile).Count==0)
                MoveTo(targetTile);
        }
        public void SlipperyMove(MapTile targetTile)
        {
            if (Traits.Contains(Trait.SLIPPERY))
                MoveTo(targetTile);
            SlipperyMovedAlready=true;
        }
        public virtual void Attack(MapTile targetTile, Weapon weapon, bool isMelee, List<int> throws, List<string>? newPriority)
        {
            if (weapon == null || !CanTargetTile(targetTile)) return;
            if (!isMelee)
            {
                int distance = 1;
                if (CurrentTile.Type == TileType.STREET)
                {
                    distance = Math.Abs(CurrentTile.Id - targetTile.Id);
                }
                if (distance > weapon.Range) return;
            }

            // Ha hangos a fegyver zajt generálunk
            if (weapon.IsLoud)
                CurrentTile.NoiseCounter++;

            List<Zombie> zombies = model.GetZombiesInPriorityOrderOnTile(targetTile);
            if (newPriority != null)
                zombies = model.SortZombiesByNewPriority(zombies, newPriority);

            List<Survivor> survivors = model.GetSurvivorsOnTile(targetTile);

            // Molotov
            if (weapon.Type == WeaponType.BOMB)
            {
                var abomina = zombies.First(x => x is Abominawild);
                if (abomina != null)
                {
                    model.RemoveZombie(abomina);
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
                    item.IsDead=true;
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

            // Zombik támadása prioritási sorrendben
            while (successfulHits > 0 && zombies.Count > 0)
            {
                Zombie targetZombie = zombies.First();

                if (targetZombie.TakeDamage(weapon.Damage))
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
                    // Ha volt túlélő a mezőn akkor sebződik
                    int amount = weapon.Damage;
                    foreach (var item in survivors)
                    {
                        item.TakeDamage(1);
                        amount--;
                        if (amount == 0) break;
                    }
                }
            }
        }
        private bool CanTargetTile(MapTile targetTile)
        {
            if (CurrentTile.Type == TileType.STREET)
            {
                var street=model.Board.GetStreetByTiles(CurrentTile.Id, targetTile.Id);
                if (street == null) return false;
            }
            else if(CurrentTile.Type==TileType.DARKROOM || CurrentTile.Type == TileType.ROOM)
            {
                if(!CurrentTile.Neighbours.Select(x=>x.Destination).Contains(targetTile)) return false;
                if(!model.Board.CanMove(CurrentTile, targetTile)) return false;
            }
            return true;
        }
        public void PickUpObjective() 
        { 
            CurrentTile.PickUpObjective();//lehet kezdünk még valamit az object refel amit visszaadna
            aPoints += 5;
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
            UsedAction = 0;
            aPoints = 0;
            backpack = new List<Item>();
            //Traits.Clear(); valamikor kéne clearelni
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

        public void OnUsedAction(string action)
        {
            if (FreeActions.Keys.Contains(action))
                FreeActions.Remove(action);
            else
                UsedAction += Actions[action].Cost;
            if (UsedAction == this.action)
            {
                FinishedRound = true;
            }
        }
        
    }
}
