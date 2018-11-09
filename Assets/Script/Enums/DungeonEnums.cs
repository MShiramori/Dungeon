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

    public enum ParamType
    {
        HP = 1,
        Level = 2,
        Exp = 3,
        Speed = 4,
        Stamina = 5,
        Str = 11,
        Vit = 12,
        Dex = 13,
        Agi = 14,
    }

    public enum StatusEffectType
    {
        仮眠 = 1,//隣接、部屋侵入で確率解除、攻撃で解除。
        睡眠 = 2,//攻撃で解除
        昏睡 = 3,//攻撃で確率解除、または睡眠に移行
        麻痺 = 4,//ターン経過以外で解除されない
        混乱 = 11,
        盲目 = 21,
        毒 = 31,
        猛毒 = 32,
        鈍足 = 41,
        倍速 = 42,
        出血 = 51,
        酔い = 61,
        泥酔 = 62,
        濡れ = 81,
        油まみれ = 82,
        目薬 = 101,
        浮遊 = 102,
        透明 = 103,
    }

    //薬の効果
    public enum PotionEffectType
    {
        水 = 0,
        回復薬 = 1,
        強回復薬 = 2,
        解毒薬 = 11,
        毒薬 = 12,
        倍速薬 = 21,
        鈍足薬 = 22,
        Lvアップ = 31,
        Lvダウン =32,
        睡眠薬 = 101,
        混乱薬 = 102,
        盲目薬 = 103,
        幻覚薬 = 104,
    }
}
