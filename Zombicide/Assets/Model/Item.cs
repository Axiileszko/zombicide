using UnityEngine;

namespace Model
{
    public abstract class Item
    {
        public ItemName Name { get; private set; }
        public Item(ItemName name)
        {
            Name = name;
        }
    }
    public class Consumable : Item
    {
        public int ApPoints { get; private set; }
        public Consumable(ItemName name, int apPoints):base(name)
        {
            ApPoints = apPoints;
        }
    }
    public enum ItemName
    {
        AXE, BASEBALLBAT, CHAINSAW, CROWBAR, KATANA, KUKRI, MACHETE, MOLOTOV, PISTOL, SAWEDOFF, SHOTGUN, SNIPERRIFLE, SUBMG, CANNEDFOOD, RICE, WATER, PLENTYOFBULLETS, PLENTYOFSHELLS, FLASHLIGHT, AUTSHOTGUN, EVILTWINS, GOLDENKUKRI, GOLDENAK47, GUNBLADE, MASSHOTGUN, MILITARYSNIPERRIFLE, NAILBAT, ZANTETSUKEN
    }
}
