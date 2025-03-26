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
        private Doug(string name, bool isKid) : base(name, isKid) { }
        public override void SetFreeActions()
        {
            FreeActions.Clear();
            if (Traits.Contains(Trait.P1FCA))
                FreeActions.Add("Attack", new GameAction("Attack", 0));
        }
        public override void SetActions(MapTile tileClicked)
        {
            Actions.Clear();
            bool l = UsedAction != action;
            if (l)
            {
                Actions.Add("Rearrange Items", new GameAction("Rearrange Items", 1));
                if (tileClicked == CurrentTile && CurrentTile.IsExit)
                    Actions.Add("Leave Through Exit", new GameAction("Leave Through Exit", 1));
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
            else if (FreeActions.Count > 0)
            {
                if (FreeActions.Keys.Any(x => x.EndsWith("Attack")))
                {
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
                }
            }
        }
        public override void UpgradeTo(int level, int option)
        {
            this.level++;
            switch (level)
            {
                case 0: return;
                case 1: action++; return;
                case 2:
                    if (option == 1)
                        Traits.Add(Trait.P1DR);
                    else
                        Traits.Add(Trait.P1FCA);
                    return;
                case 3:
                    if (option == 1)
                        Traits.Add(Trait.P1TDRC);
                    else if (option == 2)
                        Traits.Add(Trait.AMBIDEXTROUS);
                    else
                        Traits.Add(Trait.SLIPPERY);
                    return;
                default: return;
            }
        }
        public override List<string> GetTraitUpgrades(int level)
        {
            switch (level)
            {
                case 1:
                    return new List<string>()
                    {
                        "+1 action"
                    };
                case 2:
                    return new List<string>()
                    {
                        "+1 die: ranged",
                        "+1 free combat action"
                    };
                case 3:
                    return new List<string>()
                    {
                        "+1 to dice roll: combat",
                        "ambidextrous",
                        "slippery"
                    };
                default: return null;
            }
        }
    }
}
