using System;
using System.Runtime.InteropServices;

namespace HYBase.Utils
{
    public partial class Utils
    {
        public static unsafe T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            fixed (byte* ptr = &bytes[0])
            {
                return (T)Marshal.PtrToStructure((IntPtr)ptr, typeof(T));
            }
        }
    }
}