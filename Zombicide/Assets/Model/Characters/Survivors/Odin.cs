using Model.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Odin:Survivor
    {
        private static Odin instance;
        public static Odin Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Odin("Odin", true);
                }
                return instance;
            }
        }
        private Odin(string name, bool isKid) : base(name, isKid) { }
        public override void SetFreeActions()
        {
            FreeActions.Clear();
        }
        public override void SetActions(MapTile tileClicked)
        {
            bool l = UsedAction != action;
            if (l)
            {
                Actions.Add("Rearrange Items", new GameAction("Rearrange Items", 1));
                if (tileClicked == CurrentTile && CanOpenDoorOnTile())
                    Actions.Add("Open Door", new GameAction("Open Door", 1));
                if (tileClicked == CurrentTile && CurrentTile.Type != TileType.STREET && !SearchedAlready)
                    Actions.Add("Search", new GameAction("Search", 1));
                if (model.GetZombiesInPriorityOrderOnTile(tileClicked).Count > 0)
                    Actions.Add("Attack", new GameAction("Attack", 1));
                if (CurrentTile.Objective != null && tileClicked == CurrentTile)
                    Actions.Add("Pick Up Objective", new GameAction("Pick Up Objective", 1));
                if (CurrentTile.PimpWeapon != null && tileClicked == CurrentTile)
                    Actions.Add("Pick Up Pimp Weapon", new GameAction("Pick Up Pimp Weapon", 1));
            }
            if (CurrentTile.Neighbours.Select(x => x.Destination).ToList().Contains(tileClicked))
            {
                if (CurrentTile.Neighbours.First(x => x.Destination == tileClicked).IsDoorOpen || !CurrentTile.Neighbours.First(x => x.Destination == tileClicked).IsWall)
                {
                    int amount = model.GetZombiesInPriorityOrderOnTile(tileClicked).Count + 1;
                    if (model.GetZombiesInPriorityOrderOnTile(CurrentTile).Count > 0 && !SlipperyMovedAlready)
                        Actions.Add("Slippery Move", new GameAction("Slippery Move", 1));
                    if (amount + UsedAction <= action)
                        Actions.Add("Move", new GameAction("Move", amount));
                }
            }
        }
    }
}
