using System.Runtime.InteropServices;
using System;

namespace HYBase.CatalogManager
{

    [StructLayout(LayoutKind.Sequential)]
    struct RelationCatalog
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public String relationName;
        public int attrCount;
        public int indexCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct IndexCatalog
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public String relationName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public String attributeName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public String indexName;
        public int indexID;
    }
    [StructLayout(LayoutKind.Sequential)]

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