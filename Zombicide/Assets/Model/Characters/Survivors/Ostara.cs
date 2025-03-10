using Model.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Ostara:Survivor
    {
        private static Ostara instance;
        public static Ostara Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Ostara("Ostara", true);
                }
                return instance;
            }
        }
        private Ostara(string name, bool isKid) : base(name, isKid) { Traits.Add(Trait.MULTIPLESEARCH); }
        public override void SetFreeActions()
        {
        }
        public override void SetActions(MapTile tileClicked)
        {
            Actions.Clear();
            if (tileClicked == CurrentTile && CanOpenDoorOnTile())
                Actions.Add("Open Door", new GameAction("Open Door", 1, null));
            if (tileClicked == CurrentTile)
                Actions.Add("Search", new GameAction("Search", 1, () => Search()));
                Actions.Add("Multi Search", new GameAction("Multi Search", 1, () => Search()));
            if (CurrentTile.Neighbours.Select(x => x.Destination).ToList().Contains(tileClicked))
            {
                if (CurrentTile.Neighbours.First(x => x.Destination == tileClicked).IsDoorOpen || !CurrentTile.Neighbours.First(x => x.Destination == tileClicked).IsWall)
                {
                    Actions.Add("Move", new GameAction("Move", 1, () => Move(tileClicked)));
                    Actions.Add("Slippery Move", new GameAction("Slippery Move", 1, () => Move(tileClicked)));
                }
            }
            if (model.GetZombiesInPriorityOrderOnTile(tileClicked).Count > 0)
                Actions.Add("Attack", new GameAction("Attack", 1, null));
        }
    }
}
