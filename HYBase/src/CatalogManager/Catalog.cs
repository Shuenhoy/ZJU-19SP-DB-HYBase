using System.Runtime.InteropServices;
using System;
using System.Text;

namespace HYBase.CatalogManager
{

    [StructLayout(LayoutKind.Sequential)]
    struct RelationCatalog
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] relationName;
        public int attrCount;
        public int recordLength;

        public RelationCatalog(string rn, int ac, int ic)
        {
            relationName = new byte[32];
            attrCount = ac;
            recordLength = ic;
            Encoding.UTF8.GetBytes(rn).CopyTo(relationName.AsSpan());
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct IndexCatalog
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] relationName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] attributeName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] indexName;
        public string AttributeName
        {
            get
            {
                return Utils.Utils.BytesToString(attributeName);
            }
        }
        public string IndexName
        {
            get
            {
                return Utils.Utils.BytesToString(indexName);
            }
        }
        public int indexID;
        public IndexCatalog(string rn, string an, string inn, int ii)
        {
            relationName = new byte[32];
            attributeName = new byte[32];

            indexName = new byte[32];
            indexID = ii;
            Encoding.UTF8.GetBytes(rn).CopyTo(relationName.AsSpan());
            Encoding.UTF8.GetBytes(an).CopyTo(attributeName.AsSpan());
            Encoding.UTF8.GetBytes(inn).CopyTo(indexName.AsSpan());


        }
    }
    [StructLayout(LayoutKind.Sequential)]

    struct AttributeCatalog
    {

        public string RelationName
        {
            get
            {
                return Utils.Utils.BytesToString(relationName);
            }
        }
        public string AttributeName
        {
            get
            {
                return Utils.Utils.BytesToString(attributeName);
            }
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] relationName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] attributeName;
        public int offset;
        public AttrType attributeType;
        public int attributeLength;
        public int indexNo;
        public AttributeCatalog(string rn, string an, int o, AttrType attr, int attrlen, int io)
        {
            relationName = new byte[32];
            attributeName = new byte[32];
            offset = o;
            indexNo = io;
            attributeLength = attrlen;
            attributeType = attr;
            Encoding.UTF8.GetBytes(rn).CopyTo(relationName.AsSpan());
            Encoding.UTF8.GetBytes(an).CopyTo(attributeName.AsSpan());


        }

    }
}