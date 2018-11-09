using Assets.Script.Enums;
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
        public virtual int Powor { get; set; }
    }

    public class WeaponMaster : ItemMaster
    {
        public override ItemCategory Category { get { return ItemCategory.Weapon; } }
    }

    public class ArmorMaster : ItemMaster
    {
        public override ItemCategory Category { get { return ItemCategory.Armor; } }
    }

    public class PotionMaster : ItemMaster
    {
        public override ItemCategory Category { get { return ItemCategory.Potion; } }
        public PotionEffectType EffectType { get; set; }
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
            #endregion
        };
    }
}
