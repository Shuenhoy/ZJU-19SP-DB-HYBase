using System.Linq;
using System.Collections.Generic;
using System;

namespace HYBase.Utils
{
    public static class Utils
    {
        public static IEnumerable<int> Range(int st, int ed)
            => Enumerable.Range(st, ed - st + 1);
        public static U AggregateWhile<T, U>(this IEnumerable<T> sequence, U st, Func<U, T, (bool, U)> aggregate)
        {
            U a = st;
            foreach (var value in sequence)
            {
                var (cont, temp) = aggregate(a, value);
                if (!cont) break;
                a = temp;
            }
            return a;
        }
    }


}