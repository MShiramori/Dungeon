using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script.Extentions
{
    public static class EnumerableExtention
    {
        //重み付けランダム
        public static T WaitedSample<T>(this IEnumerable<T> source, Func<T, int> func)
        {
            var totalWeight = source.Sum(x => func(x));
            var value = (int)UnityEngine.Random.Range(1, totalWeight + 1);
            foreach (var data in source)
            {
                var waight = func(data);
                if (waight >= value)
                {
                    return data;
                }
                value -= waight;
            }
            return source.LastOrDefault();
        }
    }
}
