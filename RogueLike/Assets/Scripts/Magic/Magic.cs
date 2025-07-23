using UnityEngine;

namespace MagicSpells
{
    public class Magic
    {
        public float Dmg;
        public int Cost;

        public Magic(float Dmg, int Cost)
        {
            this.Dmg = Dmg;
            this.Cost = Cost;
        }

    };

    public class SpellBook
    {
        public Magic Bolt = new Magic(50f, 25);
        public Magic DarkBolt = new Magic(70f, 30);
    }
}
