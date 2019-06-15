using HYBase.RecordManager;
using System;


namespace HYBase.CatalogManager
{
    public struct AttributeInfo
    {
        public AttrType type;
        public String AttributeName;
        public int AttributeLength;
        public AttributeInfo(AttrType t, String attrName, int attrLength)
        {
            type = t;
            AttributeName = attrName;
            AttributeLength = attrLength;
        }
    }
}