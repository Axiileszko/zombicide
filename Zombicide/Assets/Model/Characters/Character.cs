using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Board;

namespace Model.Characters
{
    public abstract class Character
    {
        #region Fields
        protected int hp;
        protected int action;
        protected GameModel model;
        #endregion
        #region Properties
        public MapTile CurrentTile { get; protected set; }
        public int UsedAction {  get; set; }
        public int HP { get { return hp; } }
        #endregion
        public bool MoveTo(MapTile to)
        {
            if (CurrentTile is null)
            {
                CurrentTile = to;
                return true;
            }
            else if (CurrentTile.Neighbours.Any(n => n.Destination == to && !n.IsWall || (n.HasDoor == true && n.IsDoorOpen)))
            {
                CurrentTile = to;
                return true;
            }
            return false;
        }
        public virtual bool TakeDamage(int amount)
        {
            if (amount<hp)
                return false;
            hp -= amount;
            if (hp <= 0)
                return true;//true ha meghalt a karakter
            return false;
        }
        public void SetReference(GameModel gameModel)
        {
            this.model = gameModel;
        }
    }
}
