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
        protected int hp;
        protected int action;
        protected GameModel model;
        public MapTile CurrentTile { get; private set; }
        protected Character() { }
        public bool MoveTo(MapTile to)
        {
            if (CurrentTile is null)
            {
                CurrentTile = to;return true;
            }
            else if (CurrentTile.Neighbours.Any(n => n.Destination == to && !n.IsWall && (n.HasDoor == false || n.IsDoorOpen)))
            {
                CurrentTile = to;
                return true;
            }
            return false;
        }
        public bool TakeDamage(int amount)
        {
            hp -= amount;
            if (hp <= 0)
                return true;//true ha meghalt a karakter
            return false;
        }
        public void SetReference(GameModel gameModel)
        {
            model = gameModel;
        }
    }
}
