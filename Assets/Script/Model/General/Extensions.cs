using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script.Model
{
    public static class Extensions
    {
        public static IOrderedEnumerable<TSource> Shuffle<TSource>(this IEnumerable<TSource> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
    }
}
