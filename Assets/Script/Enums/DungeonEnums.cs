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
        Weapon = 0,
        Armor = 1,
        Food = 2,
        Potion = 3,
        Scroll = 4,
    }

    public enum TrapType
    {
        Arrow = 0,
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
