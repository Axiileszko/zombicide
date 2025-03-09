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
        public override void SetActions(MapTile tileClicked)
        {
            if (tileClicked == CurrentTile)
                Actions.Add("Search", new GameAction("Search", 1, () => Search()));
            if (tileClicked.Neighbours.Select(x => x.Destination).ToList().Contains(tileClicked))
            {
                Actions.Add("Move", new GameAction("Move", 1, () => Move(tileClicked)));
            }
            if (model.GetZombiesInPriorityOrderOnTile(tileClicked).Count > 0)
                Actions.Add("Sniper Attack", new GameAction("Sniper Attack", 1, null));
        }
    }
}
