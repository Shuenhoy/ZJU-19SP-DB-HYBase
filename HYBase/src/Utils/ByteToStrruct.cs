using System;
using System.Runtime.InteropServices;

namespace HYBase.Utils
{
    public static partial class Utils
    {
        public static unsafe T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            fixed (byte* ptr = &bytes[0])
            {

                return (T)Marshal.PtrToStructure((IntPtr)ptr, typeof(T));
            }
        }
        public static byte[] StructureToByteArray<T>(T str) where T : struct
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }
    }
}