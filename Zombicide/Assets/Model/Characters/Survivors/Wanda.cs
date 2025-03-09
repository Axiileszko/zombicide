using Model.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Survivors
{
    public class Wanda:Survivor
    {
        private static Wanda instance;
        public static Wanda Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Wanda("Wanda", false);
                }
                return instance;
            }
        }
        private Wanda(string name, bool isKid) : base(name, isKid) { Traits.Add(Trait.SPRINT); }
        public override void SetActions(MapTile tileClicked)
        {
            if (tileClicked == CurrentTile)
                Actions.Add("Search", new GameAction("Search", 1, () => Search()));
            if (tileClicked.Neighbours.Select(x => x.Destination).ToList().Contains(tileClicked))
            {
                Actions.Add("Move", new GameAction("Move", 1, () => Move(tileClicked)));
                Actions.Add("Sprint Move", new GameAction("Sprint Move", 1, () => Move(tileClicked)));
            }
            if (model.GetZombiesInPriorityOrderOnTile(tileClicked).Count > 0)
                Actions.Add("Attack", new GameAction("Attack", 1, null));
        }
    }
}
