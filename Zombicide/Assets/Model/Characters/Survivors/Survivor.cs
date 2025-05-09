﻿using Model.Board;
using Model.Characters.Zombies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    #nullable enable
    public enum Trait
    {
        SLIPPERY, MEDIC, LUCKY, SNIPER, SPRINT,JUMP,CHARGE, SHOVE, AMBIDEXTROUS, MATCHINGSET,
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
        #region Fields
        protected string name;
        protected int aPoints;
        protected bool isKid;
        protected int level = 0;
        protected List<Item> backpack = new List<Item>();
        protected Item? rightHand;
        protected Item? leftHand;
        protected bool isDead;
        public event EventHandler<string>? SurvivorDied;
        #endregion
        #region Properties
        public int ObjectiveCount {  get; protected set; }
        public bool FinishedRound { get; set; }
        public bool StartedRound { get; set; }
        public bool SearchedAlready { get; set; }
        public bool SlipperyMovedAlready { get; set; }
        public bool ChargeMovedAlready { get; set; }
        public bool JumpMovedAlready { get; set; }
        public bool SprintMovedAlready { get; set; }
        public bool LeftExit {  get; private set; }
        public List<Trait> Traits { get; protected set; } = new List<Trait>();
        public bool IsDead { get { return isDead; } set { isDead = value; if (value) OnSurvivorDied(); } }
        public int APoints { get {  return aPoints; } }
        public string Name { get { return name; } }
        public Item? RightHand { get { return rightHand; } }
        public Item? LeftHand { get { return leftHand; } }
        public List<Item> BackPack { get { return backpack; } }
        public Dictionary<string, GameAction> Actions { get; protected set; } = new Dictionary<string, GameAction>();
        public Dictionary<string, GameAction> FreeActions { get; protected set; } = new Dictionary<string, GameAction>();
        #endregion
        #region Constructors
        public Survivor(string name, bool isKid)
        {
            this.name = name;
            this.isKid = isKid;
            if (isKid)
                Traits.Add(Trait.SLIPPERY);
            Reset();
        }
        #endregion
        #region Methods
        #region Abstract Methods
        /// <summary>
        /// Returns the character's traits corresponding to the given level
        /// </summary>
        /// <param name="level">Number of the level</param>
        /// <returns>List of traits</returns>
        public abstract List<string> GetTraitUpgrades(int level);
        /// <summary>
        /// Upgrades the character to the given level with the selected trait
        /// </summary>
        /// <param name="level"></param>
        /// <param name="option"></param>
        public abstract void UpgradeTo(int level, int option);
        /// <summary>
        /// Sets the actions that the player can perform on the given tile.
        /// </summary>
        /// <param name="tileClicked">Tile the player clicked</param>
        public abstract void SetActions(MapTile tileClicked);
        /// <summary>
        /// Sets the free actions that the player can preform.
        /// </summary>
        public abstract void SetFreeActions();
        #endregion
        #region Query Methods
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
                    if ((isLeftRange && ((Weapon)leftHand!).CanBeUsedAsMelee || (isRightRange && ((Weapon)rightHand!).CanBeUsedAsMelee))) result.Add("Range");
                }
                else
                    if ((isLeftRange && distance <= ((Weapon)leftHand!).Range) || (isRightRange && distance <= ((Weapon)rightHand!).Range)) result.Add("Range");
            }
            return result;
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
        public bool HasFlashLight()
        {
            return (rightHand != null && rightHand.Name == ItemName.FLASHLIGHT) || (leftHand != null && leftHand.Name == ItemName.FLASHLIGHT) || backpack.Any(x => x.Name == ItemName.FLASHLIGHT);
        }
        #endregion
        #region Action Methods
        /// <summary>
        /// Moves the player to the given tile with a simple movement.
        /// </summary>
        /// <param name="targetTile">Tile the player wants to move to</param>
        public void Move(MapTile targetTile)
        {
            MoveTo(targetTile);
        }
        /// <summary>
        /// Moves the player to the given tile with a slippery movement.
        /// </summary>
        /// <param name="targetTile">Tile the player wants to move to</param>
        public void SlipperyMove(MapTile targetTile)
        {
            if (Traits.Contains(Trait.SLIPPERY))
                MoveTo(targetTile);
            SlipperyMovedAlready=true;
        }
        /// <summary>
        /// Moves the player to the given tile with a sprint movement.
        /// </summary>
        /// <param name="targetTile">Tile the player wants to move to</param>
        public void SprintMove(MapTile targetTile)
        {
            if(Traits.Contains(Trait.SLIPPERY) && model.GetZombiesInPriorityOrderOnTile(CurrentTile).Count>0 && !SlipperyMovedAlready)
                SlipperyMovedAlready = true;
            if (Traits.Contains(Trait.SPRINT))
                CurrentTile = targetTile;
            SprintMovedAlready = true;
        }
        /// <summary>
        /// Moves the player to the given tile with a charge movement.
        /// </summary>
        /// <param name="targetTile">Tile the player wants to move to</param>
        public void Charge(MapTile targetTile)
        {
            if (Traits.Contains(Trait.CHARGE))
                CurrentTile = targetTile;
            ChargeMovedAlready = true;
        }
        /// <summary>
        /// Moves the player to the given tile with a jump movement.
        /// </summary>
        /// <param name="targetTile">Tile the player wants to move to</param>
        public void Jump(MapTile targetTile)
        {
            if (Traits.Contains(Trait.SLIPPERY) && model.GetZombiesInPriorityOrderOnTile(CurrentTile).Count > 0 && !SlipperyMovedAlready)
                SlipperyMovedAlready = true;
            if (Traits.Contains(Trait.JUMP))
                CurrentTile = targetTile;
            JumpMovedAlready = true;
        }
        /// <summary>
        /// Performs an attack on the given tile.
        /// </summary>
        /// <param name="targetTile">Attacked tile</param>
        /// <param name="weapon">Choice of weapon</param>
        /// <param name="isMelee">True - Melee attack, False - Range attack</param>
        /// <param name="throws">Dice results</param>
        /// <param name="newPriority">If not null then the zombie priority order</param>
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
                if(rightHand!=null && rightHand.Name==ItemName.MOLOTOV)
                    rightHand = null;
                else if (leftHand != null && leftHand.Name == ItemName.MOLOTOV)
                    leftHand = null;

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
            for (int i = 0; i < throws.Count; i++)
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
        /// <summary>
        /// The player picks up the objective.
        /// </summary>
        public void PickUpObjective() 
        { 
            CurrentTile.PickUpObjective();
            aPoints += 5;
            ObjectiveCount++;
        }
        /// <summary>
        /// The player drops the given items.
        /// </summary>
        /// <param name="item">List of items to throw away</param>
        public void ThrowAway(Item item)
        {
            if (item.Name==ItemName.RICE || item.Name == ItemName.WATER || item.Name == ItemName.CANNEDFOOD)
            {
                aPoints += 3;
            }
        }
        /// <summary>
        /// The player skips their turn.
        /// </summary>
        public void Skip()
        {
            FinishedRound=true;
        }
        /// <summary>
        /// Sets the starting weapon.
        /// </summary>
        /// <param name="weapon">Weapon to set</param>
        public void PickGenericWeapon(Weapon weapon)
        {
            rightHand = weapon;
        }
        /// <summary>
        /// The player leaves the board through the exit.
        /// </summary>
        public void LeaveThroughExit()
        {
            LeftExit = true;
            FinishedRound= true;
            CurrentTile = null;
        }
        #endregion
        #region Modifier Methods
        /// <summary>
        /// Resets the data needed for the new round.
        /// </summary>
        public void NewRound()
        {
            UsedAction = 0;
            StartedRound = false;
            SlipperyMovedAlready = false;
            SearchedAlready = false;
            if (!isDead || !LeftExit)
                FinishedRound = false;
            SprintMovedAlready = false;
            ChargeMovedAlready = false;
            JumpMovedAlready = false;
        }
        /// <summary>
        /// Subtracts the damage taken from the HP.
        /// </summary>
        /// <param name="amount">Amount of damage</param>
        /// <returns></returns>
        public override bool TakeDamage(int amount)
        {
            hp =Math.Max(0,hp-amount);
            if (hp <= 0)
            {
                IsDead = true;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Resets the player's data to the starting state.
        /// </summary>
        public void Reset()
        {
            FreeActions.Clear();
            Actions.Clear();
            CurrentTile = null;
            StartedRound = false;
            FinishedRound = false;
            SearchedAlready = false;
            SlipperyMovedAlready = false;
            SprintMovedAlready = false;
            LeftExit=false;
            UsedAction = 0;
            ObjectiveCount = 0;
            isDead = false;
            level = 0;
            aPoints = 0;
            backpack = new List<Item>();
            rightHand = null;
            leftHand = null;
            if (isKid)
                hp = 2;
            else
                hp = 3;
            action = 3;
        }
        /// <summary>
        /// Puts the obtained items into the backpack.
        /// </summary>
        /// <param name="items">Recieved items</param>
        public void PutIntoBackpack(List<Item> items)
        {
            if (items.Count <= 3)
            {
                backpack = items;
            }
        }
        /// <summary>
        /// Puts the obtained item into the left or right hand.
        /// </summary>
        /// <param name="isRightHand">True - put into right hand, False - put into left hand</param>
        /// <param name="item"></param>
        public void PutIntoHand(bool isRightHand, Item? item)
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
        #endregion
        #region Event Handlers
        /// <summary>
        /// Handles the action the player performed.
        /// </summary>
        public void OnUsedAction(string action, string? isMelee)
        {
            if(isMelee!= null && FreeActions.Keys.Any(x=>x.EndsWith("Attack")))
            {
                if (isMelee == "True")
                {
                    if (FreeActions.Keys.Contains("Melee Attack"))
                        FreeActions.Remove("Melee Attack");
                    else if (FreeActions.Keys.Contains("Attack"))
                        FreeActions.Remove("Attack");
                    else
                        UsedAction += Actions[action].Cost;
                }
                else
                {
                    if (FreeActions.Keys.Contains("Range Attack"))
                        FreeActions.Remove("Range Attack");
                    else if (FreeActions.Keys.Contains("Attack"))
                        FreeActions.Remove("Attack");
                    else
                        UsedAction += Actions[action].Cost;
                }
            }
            else if (FreeActions.Keys.Contains(action))
                FreeActions.Remove(action);
            else
                UsedAction += Actions[action].Cost;
            if (UsedAction == this.action && FreeActions.Count==0)
            {
                FinishedRound = true;
            }
        }
        #endregion
        #region Invoke methods
        /// <summary>
        /// Invokes the player's death event.
        /// </summary>
        private void OnSurvivorDied()
        {
            FinishedRound = true;
            SurvivorDied!.Invoke(this, name);
        }
        #endregion
        #endregion
    }
}
