using Assets.Script.Enums;
using Assets.Script.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script.Database
{
    public class EnemyMaster
    {
        public string Name { get; set; }
        public int HP { get; set; }
        public int Speed { get; set; }
        public int Str { get; set; }
        public int Vit { get; set; }
        public int HasExp { get; set; }
        public int MaxActionCount { get; set; }
    }

    public partial class DataBase
    {
        public static readonly Dictionary<EnemyId, EnemyMaster> EnemyMasters = new Dictionary<EnemyId, EnemyMaster>()
        {
            { EnemyId.ゴブリン, new EnemyMaster()
                {
                    Name = "ゴブリン",
                    HP = 6,
                    Speed = 8,
                    Str = 2,
                    Vit = 0,
                    HasExp = 2,
                }
            },
            { EnemyId.大ネズミ, new EnemyMaster()
                {
                    Name = "大ネズミ",
                    HP = 11,
                    Speed = 8,
                    Str = 4,
                    Vit = 0,
                    HasExp = 5,
                }
            },
            { EnemyId.ローチ, new EnemyMaster()
                {
                    Name = "ローチ",
                    HP = 8,
                    Speed = 16,
                    Str = 2,
                    Vit = 0,
                    HasExp = 3,
                    MaxActionCount = 1,
                }
            },
        };
    }
}
