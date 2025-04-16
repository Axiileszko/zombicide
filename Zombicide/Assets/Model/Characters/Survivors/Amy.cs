using Model.Board;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Amy:Survivor
    {
        private static Amy instance;
        public static Amy Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Amy("Amy", false);
                }
                return instance;
            }
        }
        private Amy(string name, bool isKid):base(name, isKid){ }
        public override void SetFreeActions()
        {
            FreeActions.Clear();
            FreeActions.Add("Move", new GameAction("Move", 0));
            if(Traits.Contains(Trait.P1FMA))
                FreeActions.Add("Melee Attack", new GameAction("Melee Attack", 0));
            if (Traits.Contains(Trait.P1FRA))
                FreeActions.Add("Range Attack", new GameAction("Range Attack", 0));
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
                if (tileClicked==CurrentTile && CurrentTile.Type!=TileType.STREET&&!SearchedAlready)
                    Actions.Add("Search", new GameAction("Search", 1));
                if (model.GetZombiesInPriorityOrderOnTile(tileClicked).Count > 0)
                {
                    List<string> list = GetAvailableAttacksOnTile(tileClicked);
                    if(list!=null && list.Count>0)
                        if (tileClicked == CurrentTile)
                        {
                            if(list.Contains("Melee"))
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
                int amount = model.GetZombiesInPriorityOrderOnTile(CurrentTile).Count + 1;
                if (amount + UsedAction <= action)
                {
                    if (CurrentTile.Neighbours.Select(x => x.Destination).ToList().Contains(tileClicked))
                    {
                        if (CurrentTile.Neighbours.First(x => x.Destination == tileClicked).IsDoorOpen || (!CurrentTile.Neighbours.First(x => x.Destination == tileClicked).IsWall&& !CurrentTile.Neighbours.First(x => x.Destination == tileClicked).HasDoor))
                        {
                            Actions.Add("Move", new GameAction("Move", amount));
                        }
                    }
                }
            }
            else if(FreeActions.Count>0)
            {
                if (FreeActions.ContainsKey("Move"))
                {
                    if (CurrentTile.Neighbours.Select(x => x.Destination).ToList().Contains(tileClicked))
                    {
                        if (CurrentTile.Neighbours.First(x => x.Destination == tileClicked).IsDoorOpen || (!CurrentTile.Neighbours.First(x => x.Destination == tileClicked).IsWall && !CurrentTile.Neighbours.First(x => x.Destination == tileClicked).HasDoor))
                        {
                            Actions.Add("Move", new GameAction("Move", 0));
                        }
                    }
                }
                if (FreeActions.Keys.Any(x => x.EndsWith("Attack")))
                {
                    if (model.GetZombiesInPriorityOrderOnTile(tileClicked).Count > 0)
                    {
                        List<string> list = GetAvailableAttacksOnTile(tileClicked);
                        if (list != null && list.Count > 0)
                            if (tileClicked == CurrentTile)
                            {
                                if (list.Contains("Melee") && FreeActions.ContainsKey("Melee Attack"))
                                    Actions.Add("Attack", new GameAction("Attack", 1));
                            }
                            else
                            {
                                if (list.Contains("Range") && FreeActions.ContainsKey("Range Attack"))
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
                    {
                        Traits.Add(Trait.P1FMA);
                        FreeActions.Add("Melee Attack", new GameAction("Melee Attack", 0));
                    }
                    else
                    {
                        Traits.Add(Trait.P1FRA);
                        FreeActions.Add("Range Attack", new GameAction("Range Attack", 0));
                    }
                    return;
                case 3:
                    if (option == 1)
                        Traits.Add(Trait.P1DC);
                    else if (option == 2)
                        Traits.Add(Trait.P1TDRC);
                    else
                        Traits.Add(Trait.MEDIC);
                    return;
                default: return;
            }
        }
        public override List<string> GetTraitUpgrades(int level)
        {
            switch(level)
            {
                case 1:
                    return new List<string>()
                    {
                        "+1 action"
                    };
                case 2:
                    return new List<string>()
                    {
                        "+1 free melee action",
                        "+1 free ranged action"
                    };
                case 3:
                    return new List<string>()
                    {
                        "+1 die: combat",
                        "+1 to dice roll: combat",
                        "medic"
                    };
                default: return null;
            }
        }
    }
}
