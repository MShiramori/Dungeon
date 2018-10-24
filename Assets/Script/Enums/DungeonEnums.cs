using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script.Enums
{
    public enum Terrain
    {
        Wall = 0,
        Room = 11,
        Passage = 12,
    }

    public enum ItemCategory
    {
        None = 0,
        Weapon = 1,
        Armor = 2,
        Arrow = 3,
        Ring = 4,
        Food = 5,
        Potion = 6,
        Scroll = 7,
        Rod = 8,
    }

    public enum TrapType
    {
        Arrow = 1,
    }

    public enum CharacterType
    {
        Player = 1,
        Enemy = 2,
        Npc = 3,
    }

    public enum EnemyType
    {
        ゴブリン = 1,
    }
}
