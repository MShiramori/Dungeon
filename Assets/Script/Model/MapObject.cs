using Assets.Script.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script.Model
{
    public class MapObject
    {
        public int UniqueId { get; set; }
        public Form Position { get; set; }
    }

    public class Step : MapObject
    {
        public bool IsNext { get; set; }
    }

    public class Item : MapObject
    {
        public ItemCategory Category { get; set; }
    }

    public class Trap : MapObject
    {
        public TrapType Type { get; set; }
    }
}
