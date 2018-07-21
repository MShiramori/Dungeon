using Assets.Script.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script.Model
{
    public class CharacterParams
    {
        public string Name { get; set; }
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int Speed { get; set; }
        public int Str { get; set; }
        public int Vit { get; set; }
        public int Dex { get; set; }
        public int Agi { get; set; }
        public virtual int Attack{ get { return Str; } }
        public virtual int Defence { get { return Vit; } }
        public List<Item> Items { get; private set; }

        public CharacterParams()
        {
            Items = new List<Item>();
        }
    }

    public class PlayerParams : CharacterParams
    {
        public int Level { get; set; }
        public int Stamina { get; set; }
        public int MaxStamina { get; set; }
        public override int Attack { get { return Str + Level; } }
    }

    public class EnemyParams : CharacterParams
    {
        public EnemyType EnemyType { get; set; }
    }
}
