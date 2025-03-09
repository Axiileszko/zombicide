using Model.Board;
using Persistence;
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
        public override void SetActions(MapTile tileClicked)
        {
            if(tileClicked==CurrentTile)
                Actions.Add("Search", new GameAction("Search", 1, () => Search()));
            if (tileClicked.Neighbours.Select(x => x.Destination).ToList().Contains(tileClicked))
            {
                Actions.Add("Move", new GameAction("Move", 0, () => Move(tileClicked)));
                Actions.Add("Move", new GameAction("Move", 1, () => Move(tileClicked)));
            }
            if(model.GetZombiesInPriorityOrderOnTile(tileClicked).Count>0)
                Actions.Add("Attack",new GameAction("Attack",1,null));
        }
    }
}
