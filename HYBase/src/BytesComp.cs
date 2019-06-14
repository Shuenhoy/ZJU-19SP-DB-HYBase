using System;
using HYBase.RecordManager;
using System.Text;
namespace HYBase
{

    public static class BytesComp
    {
        public static int Comp(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b, AttrType attrType)
        {
            switch (attrType)
            {
                case AttrType.Int:
                    {
                        int aa = BitConverter.ToInt32(a);
                        int bb = BitConverter.ToInt32(b);
                        return aa - bb;
                    }
                case AttrType.Float:
                    {
                        float aa = BitConverter.ToSingle(a);
                        float bb = BitConverter.ToSingle(b);
                        return aa > bb ? 1 : Math.Abs(aa - bb) < 1e-6 ? 0 : -1;
                    }
                case AttrType.String:
                    {

                        string aa = Encoding.UTF8.GetString(a);
                        string bb = Encoding.UTF8.GetString(b);
                        return String.Compare(aa, bb);
                    }
                default:
                    throw new NotImplementedException();
            }
        }
    }
}