using System;

namespace HYBase.Utils
{
    public static partial class Utils
    {
        public static int ArrayHashCode<T>(T[] objects)
        {
            int hash = 13;

            foreach (var obj in objects)
            {
                hash = (hash * 7) + (!ReferenceEquals(null, obj) ? obj.GetHashCode() : 0);
            }

            return hash;
        }
    }
}