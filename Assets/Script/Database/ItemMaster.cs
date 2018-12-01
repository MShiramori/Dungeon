using Assets.Script.Enums;
using Assets.Script.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script.Database
{
    public abstract class ItemMaster
    {
        public virtual ItemCategory Category { get { return ItemCategory.None; } }
        public string Name { get; set; }
        public string Desc { get; set; }
        public virtual int Powor { get; set; }
    }

    public class WeaponMaster : ItemMaster
    {
        public override ItemCategory Category { get { return ItemCategory.Weapon; } }
        public Params Params { get; set; }

        public WeaponMaster()
        {
            Params = new Params();
        }
    }

    public class ArmorMaster : ItemMaster
    {
        public override ItemCategory Category { get { return ItemCategory.Armor; } }
        public Params Params { get; set; }

        public ArmorMaster()
        {
            Params = new Params();
        }
    }

    public class ArrowMaster : ItemMaster
    {
        public override ItemCategory Category { get { return ItemCategory.Arrow; } }
    }

    public class RingMaster : ItemMaster
    {
        public override ItemCategory Category { get { return ItemCategory.Ring; } }
        public Params Params { get; set; }
        public EquipEffectType EffectType { get; set; }

        public RingMaster()
        {
            Params = new Params();
        }
    }

    public class FoodMaster : ItemMaster
    {
        public override ItemCategory Category { get { return ItemCategory.Food; } }
    }

    public class PotionMaster : ItemMaster
    {
        public override ItemCategory Category { get { return ItemCategory.Potion; } }
        public PotionEffectType EffectType { get; set; }
    }

    public class ScrollMaster : ItemMaster
    {
        public override ItemCategory Category { get { return ItemCategory.Scroll; } }
        public ScrollEffectType EffectType { get; set; }
    }

    public class RodMaster : ItemMaster
    {
        public override ItemCategory Category { get { return ItemCategory.Rod; } }
        public RodEffectType EffectType { get; set; }
    }

    public partial class DataBase
    {
        /// <summary>
        /// アイテムマスタ
        /// </summary>
        public static readonly Dictionary<int, ItemMaster> ItemMasters = new Dictionary<int, ItemMaster>()
        {
            #region 武器
            { 101, new WeaponMaster()
                {
                    Name = "木の剣",
                    Powor = 2,
                }
            },
            { 102, new WeaponMaster()
                {
                    Name = "青銅の剣",
                    Powor = 4,
                }
            },
            { 103, new WeaponMaster()
                {
                    Name = "鉄の剣",
                    Powor = 7,
                }
            },
            { 104, new WeaponMaster()
                {
                    Name = "ミスリルの剣",
                    Powor = 10,
                }
            },
            #endregion

            #region 防具
            { 201, new ArmorMaster()
                {
                    Name = "木の盾",
                    Powor = 2,
                }
            },
            { 202, new ArmorMaster()
                {
                    Name = "青銅の盾",
                    Powor = 4,
                }
            },
            { 203, new ArmorMaster()
                {
                    Name = "鉄の盾",
                    Powor = 7,
                }
            },
            { 204, new ArmorMaster()
                {
                    Name = "ミスリルの盾",
                    Powor = 10,
                }
            },
            #endregion

            #region 矢弾
            { 301, new ArrowMaster()
                {
                    Name = "木の矢",
                    Powor = 2,
                }
            },
            { 302, new ArrowMaster()
                {
                    Name = "鉄の矢",
                    Powor = 5,
                }
            },
            { 303, new ArrowMaster()
                {
                    Name = "銀の矢",
                    Powor = 10,
                }
            },
            #endregion

            #region 指輪
            { 401, new RingMaster()
                {
                    Name = "豪腕の指輪",
                    EffectType = EquipEffectType.なし,
                    Params = new Params(3,0,0,0),
                    Desc = "力が強くなる指輪",
                }
            },
            { 402, new RingMaster()
                {
                    Name = "脱力の指輪",
                    EffectType = EquipEffectType.なし,
                    Params = new Params(-3,0,0,0),
                    Desc = "力が弱くなる指輪",
                }
            },
            { 411, new RingMaster()
                {
                    Name = "隠密の指輪",
                    EffectType = EquipEffectType.隠密移動,
                    Desc = "魔物を起こさずに移動できるようになる指輪",
                }
            },
            { 412, new RingMaster()
                {
                    Name = "騒音の指輪",
                    EffectType = EquipEffectType.騒音移動,
                    Desc = "魔物を必ず起こしてしまうようになる指輪",
                }
            },
            { 413, new RingMaster()
                {
                    Name = "軽業の指輪",
                    EffectType = EquipEffectType.罠回避,
                    Desc = "罠を回避できるようになる指輪",
                }
            },
            { 414, new RingMaster()
                {
                    Name = "鈍重の指輪",
                    EffectType = EquipEffectType.罠確定発動,
                    Desc = "罠に確実にかかるようになる指輪",
                }
            },
            { 415, new RingMaster()
                {
                    Name = "節制の指輪",
                    EffectType = EquipEffectType.食料節制,
                    Desc = "食料の消費を抑えられるようになる指輪",
                }
            },
            { 421, new RingMaster()
                {
                    Name = "転移の指輪",
                    EffectType = EquipEffectType.テレポート,
                    Desc = "ランダムでテレポートするようになる指輪",
                }
            },
            { 431, new RingMaster()
                {
                    Name = "解毒の指輪",
                    EffectType = EquipEffectType.毒防止,
                    Desc = "毒にかからなくなる指輪",
                }
            },
            { 432, new RingMaster()
                {
                    Name = "覚醒の指輪",
                    EffectType = EquipEffectType.睡眠防止,
                    Desc = "眠らなくなる指輪",
                }
            },
            { 433, new RingMaster()
                {
                    Name = "安息の指輪",
                    EffectType = EquipEffectType.混乱防止,
                    Desc = "混乱しなくなる指輪",
                }
            },
            { 441, new RingMaster()
                {
                    Name = "抵抗の指輪",
                    EffectType = EquipEffectType.パラメータ低下耐性,
                    Desc = "力の低下を防ぐ指輪",
                }
            },
            #endregion

            #region 食料
            { 501, new FoodMaster()
                {
                    Name = "干し肉",
                    Powor = 50,
                }
            },
            { 502, new FoodMaster()
                {
                    Name = "携帯食料",
                    Powor = 100,
                }
            },
            #endregion

            #region 薬
            { 601, new PotionMaster()
                {
                    Name = "水",
                    EffectType = PotionEffectType.水,
                }
            },
            { 602, new PotionMaster()
                {
                    Name = "体力回復の薬",
                    EffectType = PotionEffectType.回復薬,
                }
            },
            { 603, new PotionMaster()
                {
                    Name = "エリクシル",
                    EffectType = PotionEffectType.強回復薬,
                }
            },
            { 604, new PotionMaster()
                {
                    Name = "レベルアップの薬",
                    EffectType = PotionEffectType.Lvアップ,
                }
            },
            #endregion
        };
    }
}
