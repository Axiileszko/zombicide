using Model.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Lou:Survivor
    {
        private static Lou instance;
        public static Lou Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Lou("Lou", true);
                }
                return instance;
            }
        }
        private Lou(string name, bool isKid) : base(name, isKid) { Traits.Add(Trait.CHARGE); }
        public override void SetActions(MapTile tileClicked)
        {
            if (tileClicked == CurrentTile)
                Actions.Add("Search", new GameAction("Search", 1, () => Search()));
            if (tileClicked.Neighbours.Select(x => x.Destination).ToList().Contains(tileClicked))
            {
                Actions.Add("Charge Move", new GameAction("Charge Move", 1, () => Move(tileClicked)));
                Actions.Add("Slippery Move", new GameAction("Slippery Move", 1, () => Move(tileClicked)));
                Actions.Add("Move", new GameAction("Move", 1, () => Move(tileClicked)));
            }
            if (model.GetZombiesInPriorityOrderOnTile(tileClicked).Count > 0)
                Actions.Add("Attack", new GameAction("Attack", 1, null));
        }
    }
}
