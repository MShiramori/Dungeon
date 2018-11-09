using System;
using System.Collections.Generic;

namespace Assets.Script.Database
{
    public class FloorMaster
    {
        public string TileSetName { get; set; }
        public int ItemTableId { get; set; }
        public int EnemyTableId { get; set; }

        public ItemTableMaster[] ItemTable { get { return DataBase.ItemTableMasters[ItemTableId]; } }
        public int[] EnemyTable { get { return DataBase.EnemyTableMasters[EnemyTableId]; } }
    }

    public class ItemTableMaster
    {
        public int ItemId { get; set; }
        public int Rate { get; set; }
    }

    public partial class DataBase
    {
        /// <summary>
        /// フロア情報マスタ
        /// </summary>
        public static readonly Dictionary<int, FloorMaster> FloorMasters = new Dictionary<int, FloorMaster>() {
            { 1, new FloorMaster() { TileSetName = "Default", ItemTableId = 1, EnemyTableId = 1 }},
            { 2, new FloorMaster() { TileSetName = "Default", ItemTableId = 1, EnemyTableId = 1 }},
            { 3, new FloorMaster() { TileSetName = "Default", ItemTableId = 1, EnemyTableId = 1 }},
            { 4, new FloorMaster() { TileSetName = "Default", ItemTableId = 1, EnemyTableId = 1 }},
            { 5, new FloorMaster() { TileSetName = "Default", ItemTableId = 1, EnemyTableId = 1 }},
            { 6, new FloorMaster() { TileSetName = "Default", ItemTableId = 1, EnemyTableId = 1 }},
            { 7, new FloorMaster() { TileSetName = "Default", ItemTableId = 1, EnemyTableId = 1 }},
            { 8, new FloorMaster() { TileSetName = "Default", ItemTableId = 1, EnemyTableId = 1 }},
            { 9, new FloorMaster() { TileSetName = "Default", ItemTableId = 1, EnemyTableId = 1 }},
            { 10, new FloorMaster() { TileSetName = "Default", ItemTableId = 1, EnemyTableId = 1 }},
        };

        /// <summary>
        /// アイテム抽選テーブルマスタ
        /// </summary>
        public static readonly Dictionary<int, ItemTableMaster[]> ItemTableMasters = new Dictionary<int, ItemTableMaster[]>()
        {
            { 1, new ItemTableMaster[]
                {
                    new ItemTableMaster() { ItemId = 101, Rate = 100 },
                    new ItemTableMaster() { ItemId = 102, Rate = 100 },
                    new ItemTableMaster() { ItemId = 103, Rate = 70 },
                    new ItemTableMaster() { ItemId = 104, Rate = 30 },
                    new ItemTableMaster() { ItemId = 201, Rate = 100 },
                    new ItemTableMaster() { ItemId = 202, Rate = 100 },
                    new ItemTableMaster() { ItemId = 203, Rate = 70 },
                    new ItemTableMaster() { ItemId = 204, Rate = 30 },
                    new ItemTableMaster() { ItemId = 601, Rate = 150 },
                    new ItemTableMaster() { ItemId = 602, Rate = 70 },
                    new ItemTableMaster() { ItemId = 603, Rate = 30 },
                }
            },
        };

        /// <summary>
        /// 敵出現テーブルマスタ
        /// </summary>
        public static readonly Dictionary<int, int[]> EnemyTableMasters = new Dictionary<int, int[]>()
        {
            { 1, new int[]{ 1 } },
        };
    }
}
