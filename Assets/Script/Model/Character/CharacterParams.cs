using Assets.Script.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script.Model
{
    public class CharacterParams
    {
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int Speed { get; set; }
        public List<Item> Items { get; private set; }

        public CharacterParams()
        {
            Items = new List<Item>();
        }
    }

    public class PlayerParams : CharacterParams
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int Stamina { get; set; }
        public int MaxStamina { get; set; }
    }

    public class EnemyParams : CharacterParams
    {
        public string Name { get; set; }
        public EnemyType EnemyType { get; set; }
    }
}
