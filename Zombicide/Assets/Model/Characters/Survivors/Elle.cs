using Model.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Elle:Survivor
    {
        private static Elle instance;
        public static Elle Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Elle("Elle", true);
                }
                return instance;
            }
        }
        private Elle(string name, bool isKid) : base(name, isKid) { Traits.Add(Trait.SNIPER); }
        public override void SetFreeActions()
        {
            FreeActions.Clear();
            if (Traits.Contains(Trait.P1FRA))
                FreeActions.Add("Range Attack", new GameAction("Range Attack", 0));
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
                if (tileClicked == CurrentTile && CurrentTile.IsExit && model.GetZombiesInPriorityOrderOnTile(tileClicked).Count == 0)
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
            }
            int amount = model.GetZombiesInPriorityOrderOnTile(CurrentTile).Count + 1;
            if (amount + UsedAction <= action)
            {
                if (CurrentTile.Neighbours.Select(x => x.Destination).ToList().Contains(tileClicked))
                {
                    if (CurrentTile.Neighbours.First(x => x.Destination == tileClicked).IsDoorOpen || (!CurrentTile.Neighbours.First(x => x.Destination == tileClicked).IsWall && !CurrentTile.Neighbours.First(x => x.Destination == tileClicked).HasDoor))
                    {
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
                                if (list.Contains("Melee") && FreeActions.Keys.Contains("Attack"))
                                    Actions.Add("Attack", new GameAction("Attack", 1));
                            }
                            else
                            {
                                if (list.Contains("Range") && (FreeActions.Keys.Contains("Attack")||FreeActions.Keys.Contains("Range Attack")))
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
                        Traits.Add(Trait.P1DC);
                    else
                        Traits.Add(Trait.P1FRA);
                    return;
                case 3:
                    if (option == 1)
                        Traits.Add(Trait.P1DR);
                    else if (option == 2)
                        Traits.Add(Trait.P1FCA);
                    else
                        Traits.Add(Trait.P1TDRR);
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
                        "+1 die: combat",
                        "+1 free ranged action"
                    };
                case 3:
                    return new List<string>()
                    {
                        "+1 die: ranged",
                        "+1 free combat action",
                        "+1 to dice roll: ranged"
                    };
                default: return null;
            }
        }
    }
}
