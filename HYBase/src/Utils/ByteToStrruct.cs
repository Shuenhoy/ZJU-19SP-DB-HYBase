using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

namespace HYBase.Utils
{
    public static partial class Utils
    {
        public static string BytesToString(byte[] bytes)
        {
            var length = bytes.TakeWhile(b => b != 0).Count();
            return Encoding.UTF8.GetString(bytes, 0, length);

        }
        public static string BytesToString(byte[] bytes, int start, int len)
        {
            var length = bytes.Skip(start).Take(len).TakeWhile(b => b != 0).Count();
            return Encoding.UTF8.GetString(bytes, 0, length);

        }
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