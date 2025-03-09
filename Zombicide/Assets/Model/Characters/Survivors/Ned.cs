using Model.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Ned:Survivor
    {
        private static Ned instance;
        public static Ned Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Ned("Ned", false);
                }
                return instance;
            }
        }
        private Ned(string name, bool isKid) : base(name, isKid) { }
        public override void SetActions(MapTile tileClicked)
        {
            if (tileClicked == CurrentTile)
                Actions.Add("Search", new GameAction("Search", 0, () => Search()));
                Actions.Add("Search", new GameAction("Search", 1, () => Search()));
            if (tileClicked.Neighbours.Select(x => x.Destination).ToList().Contains(tileClicked))
            {
                Actions.Add("Move", new GameAction("Move", 1, () => Move(tileClicked)));
            }
            if (model.GetZombiesInPriorityOrderOnTile(tileClicked).Count > 0)
                Actions.Add("Attack", new GameAction("Attack", 1, null));
        }
    }
}
