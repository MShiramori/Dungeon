using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script.Database
{
    public partial class DataBase
    {
        public static readonly Dictionary<int, long> PlayerLevelMasters = new Dictionary<int, long>()
        {
            { 1, 0 },
            { 2, 10 },
            { 3, 30 },
            { 4, 60 },
            { 5, 100 },
            { 6, 150 },
            { 7, 230 },
            { 8, 350 },
            { 9, 500 },
            { 10, 700 },
            { 11, 950 },
            { 12, 1200 },
            { 13, 1500 },
            { 14, 1800 },
            { 15, 2300 },
            { 16, 2800 },
            { 17, 3500 },
            { 18, 4200 },
            { 19, 5000 },
            { 20, 6000 },
        };
    }
}
