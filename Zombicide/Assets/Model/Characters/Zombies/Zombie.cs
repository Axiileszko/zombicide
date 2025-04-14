using Model.Board;
using Model.Characters.Survivors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Characters.Zombies
{
    public abstract class Zombie:Character
    {
        public int Priority { get; protected set; }
        /// <summary>
        /// Moves the zombie to the tile determined by the algorithm.
        /// </summary>
        public void Move()
        {
            List<Survivor> survivors = model.GetSurvivorsOnTile(CurrentTile);
            if (survivors != null && survivors.Count > 0)
            {
                Attack(survivors);
                return;
            } 
            Dictionary<MapTile, int> priority = new Dictionary<MapTile, int>();
            foreach (var item in CurrentTile.Neighbours)
            {
                if (model.Board.CanMove(CurrentTile, item.Destination))
                {
                    priority.Add(item.Destination, 1);

                    if (CurrentTile.Type == Board.TileType.STREET)
                    {
                        Street street = model.Board.GetStreetByTiles(CurrentTile.Id, item.Destination.Id);
                        if (street != null)
                        {
                            int survivorsSeen = LookUpStreet(street);
                            priority[item.Destination] += survivorsSeen;
                        }
                    }
                    else
                    {
                        int seen = 0;
                        foreach (var location in model.SurvivorLocations)
                        {
                            if (location == item.Destination.Id && item.IsDoorOpen)
                                seen++;
                        }
                        priority[item.Destination] += seen;
                    }
                }
            }
            MapTile noisiest = model.FindNextStepToNoisiest(CurrentTile);
            if (noisiest != null)
            {
                priority[noisiest] += 1;
            }
            var destination = priority.First(x => x.Value == priority.Values.Max());
            MoveTo(destination.Key);
        }
        /// <summary>
        /// Attacks the first survivor of the list given
        /// </summary>
        /// <param name="survivors">List of survivors</param>
        public void Attack(List<Survivor> survivors)
        {
            survivors[0].TakeDamage(1);
            model.CheckWin();
        }
        /// <summary>
        /// Returns the amount of survivors seen by the zombie
        /// </summary>
        /// <param name="street">The street the zombie is on</param>
        /// <returns>Amount of survivors seen</returns>
        private int LookUpStreet(Street street)
        {
            int seen = 0;
            foreach (var tile in street.Tiles)
            {
                foreach (var location in model.SurvivorLocations)
                {
                    if(location==tile)
                        seen++;
                }
            }
            return seen;
        }
    }
}
