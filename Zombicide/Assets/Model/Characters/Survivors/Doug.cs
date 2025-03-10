using Model.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Doug:Survivor
    {
        private static Doug instance;
        public static Doug Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Doug("Doug", false);
                }
                return instance;
            }
        }
        private Doug(string name, bool isKid) : base(name, isKid) { Traits.Add(Trait.MATHCINGSET); }
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
            if (CurrentTile.Neighbours.Select(x => x.Destination).ToList().Contains(tileClicked))
            {
                if (CurrentTile.Neighbours.First(x => x.Destination == tileClicked).IsDoorOpen || !CurrentTile.Neighbours.First(x => x.Destination == tileClicked).IsWall)
                {
                    Actions.Add("Move", new GameAction("Move", 1, () => Move(tileClicked)));
                }
            }
            if (model.GetZombiesInPriorityOrderOnTile(tileClicked).Count > 0)
                Actions.Add("Attack", new GameAction("Attack", 1, null));
        }
    }
}
