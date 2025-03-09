using Model.Characters.Zombies;

namespace Model.Characters.Survivors
{
    public class SurvivorFactory
    {
        public static Survivor GetSurvivorByName(string s)
        {
            switch (s)
            {
                case "Amy": return Amy.Instance;
                case "Bunny G": return BunnyG.Instance;
                case "Doug": return Doug.Instance;
                case "Elle": return Elle.Instance;
                case "Josh": return Josh.Instance;
                case "Lili": return Lili.Instance;
                case "Lou": return Lou.Instance;
                case "Ned": return Ned.Instance;
                case "Odin": return Odin.Instance;
                case "Ostara": return Ostara.Instance;
                case "Tiger Sam": return TigerSam.Instance;
                case "Wanda": return Wanda.Instance;
                default: return null;
            }
        }
    }
}