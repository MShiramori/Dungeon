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

    public enum EnemyId
    {
        ゴブリン = 1,
        大ネズミ = 2,
        大コウモリ = 3,
        ローチ = 4,
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
        封印 = 51,
        出血 = 61,
        酔い = 71,
        泥酔 = 72,
        濡れ = 81,
        油まみれ = 82,
        目薬 = 101,
        浮遊 = 102,
        透明 = 103,
    }

    //装備品の装備効果
    public enum EquipEffectType
    {
        なし = 0,
        隠密移動 = 11,
        騒音移動 = 12,
        罠回避 = 13,
        罠確定発動 = 14,
        食料節制 = 21,
        水路渡り = 31,
        テレポート = 32,
        毒防止 = 101,
        睡眠防止 = 102,
        混乱防止 = 103,
        パラメータ低下耐性 = 201,
        装備保護 = 202,
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
        力アップ = 41,
        力ダウン = 42,
        力回復 = 43,
        睡眠薬 = 101,
        混乱薬 = 102,
        盲目薬 = 103,
        幻覚薬 = 104,
        浮遊薬 = 111,
        油 = 201,
    }

    //巻物の効果
    public enum ScrollEffectType
    {
        武器強化 = 1,
        防具強化 = 2,
        装備保護 = 3,
        鑑定 = 11,
        解呪 = 12,
        テレポート = 21,
        マッピング = 22,
        部屋攻撃 = 31,
        周囲睡眠 = 32,
        警報 = 41,
        召喚 = 42,
    }

    public enum RodEffectType
    {
        効果なし = 0,
        ダメージ = 1,
        回復 = 2,
        睡眠 = 11,
        混乱 = 12,
        麻痺 = 13,
        鈍足 = 21,
        倍速 = 22,
        封印 = 23,
        透明 = 24,
        テレポート = 31,
        変身 = 32,
    }
}
