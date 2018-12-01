using Assets.Script.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script.Model
{
    public class Params
    {
        public int MaxHP { get; set; }
        public int Speed { get; set; }
        public int Str { get; set; }
        public int Vit { get; set; }
        public int Dex { get; set; }
        public int Agi { get; set; }

        public Params() { }
        public Params(int str, int vit, int dex, int agi)
        {
            this.Str = str;
            this.Vit = vit;
            this.Dex = dex;
            this.Agi = agi;
        }
    }

    public class CharacterParams
    {
        public string Name { get; set; }
        public int HP { get; set; }
        public Params Params { get; set; }
        public int MaxHP { get { return Params.MaxHP; } set { Params.MaxHP = value; } }
        public int Speed { get { return Params.Speed; } set { Params.Speed = value; } }
        public int Str { get { return Params.Str; } set { Params.Str = value; } }
        public int Vit { get { return Params.Vit; } set { Params.Vit = value; } }
        public int Dex { get { return Params.Dex; } set { Params.Dex = value; } }
        public int Agi { get { return Params.Agi; } set { Params.Agi = value; } }
        public virtual int Attack{ get { return Str; } }
        public virtual int Defence { get { return Vit; } }
        public List<Item> Items { get; private set; }
        public Dictionary<ItemCategory, Item> Equips;

        public CharacterParams()
        {
            Params = new Params();
            Items = new List<Item>();
            Equips = new Dictionary<ItemCategory, Item>();
        }
    }

    public class PlayerParams : CharacterParams
    {
        public int Level { get; set; }
        public long Exp { get; set; }
        public float Stamina { get; set; }
        public int MaxStamina { get; set; }
        public override int Attack { get { return Str + Level; } }

        public PlayerParams()
        {
            
        }
    }

    public class EnemyParams : CharacterParams
    {
        public EnemyId EnemyType { get; set; }
        public int HasExp { get; set; }
    }
}
