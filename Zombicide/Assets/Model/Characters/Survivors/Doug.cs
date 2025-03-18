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
            FreeActions.Clear();
        }
        public override void SetActions(MapTile tileClicked)
        {
            Actions.Clear();
            bool l = UsedAction != action;
            if (l)
            {
                Actions.Add("Rearrange Items", new GameAction("Rearrange Items", 1));
                if (tileClicked == CurrentTile && CanOpenDoorOnTile())
                    Actions.Add("Open Door", new GameAction("Open Door", 1));
                if (tileClicked == CurrentTile && CurrentTile.Type != TileType.STREET && !SearchedAlready)
                    Actions.Add("Search", new GameAction("Search", 1));
                if (model.GetZombiesInPriorityOrderOnTile(tileClicked).Count > 0)
                {
                    List<string> list = GetAvailableAttacksOnTile(tileClicked);
                    if (list != null && list.Count > 0)
                        if (tileClicked == CurrentTile)
                        {
                            if (list.Contains("Melee"))
                                Actions.Add("Attack", new GameAction("Attack", 1));
                        }
                        else
                        {
                            if (list.Contains("Range"))
                                Actions.Add("Attack", new GameAction("Attack", 1));
                        }
                }
                if (CurrentTile.Objective != null && tileClicked == CurrentTile)
                    Actions.Add("Pick Up Objective", new GameAction("Pick Up Objective", 1));
                if (CurrentTile.PimpWeapon != null && tileClicked == CurrentTile)
                    Actions.Add("Pick Up Pimp Weapon", new GameAction("Pick Up Pimp Weapon", 1));
            }
            if (CurrentTile.Neighbours.Select(x => x.Destination).ToList().Contains(tileClicked))
            {
                if (CurrentTile.Neighbours.First(x => x.Destination == tileClicked).IsDoorOpen || (!CurrentTile.Neighbours.First(x => x.Destination == tileClicked).IsWall && !CurrentTile.Neighbours.First(x => x.Destination == tileClicked).HasDoor))
                {
                    int amount = model.GetZombiesInPriorityOrderOnTile(CurrentTile).Count + 1;
                    if (model.GetZombiesInPriorityOrderOnTile(CurrentTile).Count > 0 && !SlipperyMovedAlready && Traits.Contains(Trait.SLIPPERY))
                        Actions.Add("Slippery Move", new GameAction("Slippery Move", 1));
                    if (amount + UsedAction <= action)
                        Actions.Add("Move", new GameAction("Move", amount));
                }
            }
        }
    }
}
