using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniRx;
using UnityEngine;

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

        /// <summary>
        /// メニューウィンドウ入力キーマスタ
        /// </summary>
        public static readonly Dictionary<int, Tuple<string, KeyCode>> KeyMaster = new Dictionary<int, Tuple<string, KeyCode>>()
        {
            { 0, Tuple.Create("a", KeyCode.A) },
            { 1, Tuple.Create("b", KeyCode.B) },
            { 2, Tuple.Create("c", KeyCode.C) },
            { 3, Tuple.Create("d", KeyCode.D) },
            { 4, Tuple.Create("e", KeyCode.E) },
            { 5, Tuple.Create("f", KeyCode.F) },
            { 6, Tuple.Create("g", KeyCode.G) },
            { 7, Tuple.Create("h", KeyCode.H) },
            { 8, Tuple.Create("i", KeyCode.I) },
            { 9, Tuple.Create("j", KeyCode.J) },
            { 10, Tuple.Create("k", KeyCode.K) },
            { 11, Tuple.Create("l", KeyCode.L) },
            { 12, Tuple.Create("m", KeyCode.M) },
            { 13, Tuple.Create("n", KeyCode.N) },
            { 14, Tuple.Create("o", KeyCode.O) },
            { 15, Tuple.Create("p", KeyCode.P) },
            { 16, Tuple.Create("q", KeyCode.Q) },
            { 17, Tuple.Create("r", KeyCode.R) },
            { 18, Tuple.Create("s", KeyCode.S) },
            { 19, Tuple.Create("t", KeyCode.T) },
            { 20, Tuple.Create("u", KeyCode.U) },
            { 21, Tuple.Create("v", KeyCode.V) },
            { 22, Tuple.Create("w", KeyCode.W) },
            { 23, Tuple.Create("x", KeyCode.X) },
            { 24, Tuple.Create("y", KeyCode.Y) },
            { 25, Tuple.Create("z", KeyCode.Z) },
        };
    }
}
