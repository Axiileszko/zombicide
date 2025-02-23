using Model.Board;
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
        public bool FinishedRound {  get; protected set; }
        public bool StartedRound {  get; set; }

        public Survivor(string name, bool isKid)
        {
            this.name = name;
            this.isKid = isKid;
            Reset();
        }
        public abstract void Attack(Character target, int damage);//még változhat
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
    }
}
