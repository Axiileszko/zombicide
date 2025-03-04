using Model.Board;
using Model.Characters.Zombies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public abstract class Survivor:Character
    {
        protected int usedAction;
        protected string name;
        protected int aPoints;
        protected bool isKid;
        protected List<Item> backpack;
        protected Item rightHand;
        protected Item leftHand;
        public bool FinishedRound {  get; set; }
        public bool StartedRound {  get; set; }
        public bool IsDead { get; set; }
        public string Name { get { return name; } }
        public Survivor(string name, bool isKid)
        {
            this.name = name;
            this.isKid = isKid;
            Reset();
        }
        public virtual void Attack(MapTile targetTile, Weapon weapon, bool isMelee)
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
            Random random = new Random();
            for (int i = 0; i < weapon.DiceAmount; i++)
            {
                int roll = random.Next(1, 7); // 1-6 között
                if (roll >= weapon.Accuracy)
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
        public void RecieveItem(Item item)
        {
            backpack.Add(item);
            //ilyenkor lehet majd rendezni
        }
        public void Search() 
        {
            var item=model.Search(CurrentTile);
            if (item != null) { backpack.Add(item); }
            //nézzuk hogy belefér-e->muszáj e eldobni
        }

        public Item ThrowAway(Item item)
        {
            if (item.Name==ItemName.RICE || item.Name == ItemName.WATER || item.Name == ItemName.CANNEDFOOD)
            {
                aPoints += 3;
            }
            if(rightHand==item)
                rightHand = null;
            else if (leftHand==item)
                leftHand = null;
            backpack.Remove(item);
            return item;
        }
        public void Reset()
        {
            StartedRound = false;
            FinishedRound = false;
            usedAction = 0;
            aPoints = 0;
            model = null;
            backpack = new List<Item>();
            rightHand = null;
            leftHand = null;
            if (isKid)
                hp = 2;
            else
                hp = 3;
            action = 3;
        }
        public void SwitchItems(Survivor survivor, Item item, bool gives)
        {
            //másiknak adunk vagy másiktól kérünk
            if (survivor.CurrentTile == CurrentTile)
            {
                if(gives)
                {
                    //mi adunk
                    var itemToGive = ThrowAway(item);
                    survivor.RecieveItem(itemToGive);
                }
                else
                {
                    //mi kapunk
                    var itemToGet = survivor.ThrowAway(item);
                    RecieveItem(itemToGet);
                }
            }
        }

        public void PutIntoBackpack(bool isRightHand)
        {
            if (backpack.Count < 3)
            {
                if (isRightHand)
                {
                    var item = rightHand;
                    rightHand = null;
                    backpack.Add(item);
                }
                else
                {
                    var item = leftHand;
                    leftHand = null;
                    backpack.Add(item);
                }
            }
        }
        public void TakeFromBackpack(bool isRightHand, Item item)
        {
            if (isRightHand && rightHand==null)
            {
                backpack.Remove(item);
                rightHand = item;
            }
            else if(leftHand==null)
            {
                backpack.Remove(item);
                leftHand = item;
            }
        }
        public void Skip()
        {
            FinishedRound=true;
            StartedRound=false;
            usedAction = 0;
        }

        public void PickGenericWeapon()
        {
            rightHand = ItemFactory.GetGenericWeapon();
        }
    }
}
