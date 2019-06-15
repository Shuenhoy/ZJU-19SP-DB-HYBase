using System;
using System.Collections.Generic;
using System.Linq;

namespace HYBase.UnitTests
{
    static class Utils
    {
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[rng.Next(s.Length)]).ToArray());
        }
        private
          static Random rng = new Random();

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