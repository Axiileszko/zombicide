using Model.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class BunnyG:Survivor
    {
        private static BunnyG instance;
        public static BunnyG Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BunnyG("Bunny G", true);
                }
                return instance;
            }
        }
        private BunnyG(string name, bool isKid) : base(name, isKid) { Traits.Add(Trait.LUCKY); }
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

                int amount = model.GetZombiesInPriorityOrderOnTile(CurrentTile).Count + 1;
                if (CurrentTile.Neighbours.Select(x => x.Destination).ToList().Contains(tileClicked))
                {
                    if (CurrentTile.Neighbours.First(x => x.Destination == tileClicked).IsDoorOpen || (!CurrentTile.Neighbours.First(x => x.Destination == tileClicked).IsWall && !CurrentTile.Neighbours.First(x => x.Destination == tileClicked).HasDoor))
                    {
                        if (model.GetZombiesInPriorityOrderOnTile(CurrentTile).Count > 0 && !SlipperyMovedAlready && Traits.Contains(Trait.SLIPPERY))
                            Actions.Add("Slippery Move", new GameAction("Slippery Move", 1));
                        if (amount + UsedAction <= action)
                            Actions.Add("Move", new GameAction("Move", amount));
                    }
                }
                int distance = Board.Board.GetShortestPath(CurrentTile, tileClicked);
                if (distance!=-1&&distance <= 2 && tileClicked!=CurrentTile && !JumpMovedAlready && Traits.Contains(Trait.JUMP))
                {
                    if(SlipperyMovedAlready)
                        Actions.Add("Jump", new GameAction("Jump", amount));
                    else
                        Actions.Add("Jump", new GameAction("Jump", 1));
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
                        Traits.Add(Trait.P1TDRM);
                    else
                        Traits.Add(Trait.JUMP);
                    return;
                case 3:
                    if (option == 1)
                        Traits.Add(Trait.P1DMM);
                    else if (option == 2)
                    {
                        Traits.Add(Trait.P1FCA);
                        FreeActions.Add("Attack", new GameAction("Attack", 0));
                    }
                    else
                        Traits.Add(Trait.ROLL6);
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
                        "+1 to dice roll: melee",
                        "jump"
                    };
                case 3:
                    return new List<string>()
                    {
                        "+1 damage: melee",
                        "+1 free combat action",
                        "roll 6: +1 die combat"
                    };
                default: return null;
            }
        }
    }
}
