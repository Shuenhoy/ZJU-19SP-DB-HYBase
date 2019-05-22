using System.Runtime.InteropServices;
using System;

namespace HYBase.CatalogManager
{

    [StructLayout(LayoutKind.Sequential)]
    struct RelationCatalog
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        String relationName;
        int attrCount;
        int indexCount;
    }
    struct AttributeCatalog
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public String relationName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public String attributeName;
        public int offset;
        public AttrType attributeType;
        public int attributeLength;
        public int indexNo;

    }
}