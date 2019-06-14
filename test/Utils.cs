using System;
using System.Collections.Generic;

namespace HYBase.UnitTests
{
    static class Utils
    {
        private
          static Random rng = new Random(724523);

        public
          static IList<T> Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
} // namespace HYBase.UnitTests