using System;
using System.IO;
using System.Collections.Generic;
using HYBase.RecordManager;
using static HYBase.Utils.Utils;

namespace HYBase.CatalogManager
{
    /// <summary>
    /// 存储数据库中的表信息与表的属性信息
    /// 二者同样作为表存在
    /// </summary>
    class CatalogManager
    {
        RecordFile relCatalog, attrCatalog;
        FileScan scan;

        public CatalogManager(RecordManager.RecordManager rm, Stream relcat, Stream attrcat)
        {
            relCatalog = rm.OpenFile(relcat);
            attrCatalog = rm.OpenFile(attrcat);
            scan = new RecordManager.FileScan();
        }
        public void CreateTable(String tableName, AttributeInfo[] attributes)
        {
            var catalog = new RelationCatalog { relationName = tableName, attrCount = attributes.Length, indexCount = 0 };
            relCatalog.InsertRec(StructureToByteArray(catalog));
            int offset = 0;
            foreach (var attr in attributes)
            {
                var attrcatalog = new AttributeCatalog
                {
                    relationName = tableName,
                    attributeName = attr.AttributeName,
                    attributeLength = attr.AttributeLength,
                    attributeType = attr.type,
                    offset = offset
                };
                offset += attr.AttributeLength;
                attrCatalog.InsertRec(StructureToByteArray(attrcatalog));
            }
        }
        public void DropTable(String tableName)
        {
            scan.OpenScan(relCatalog, 32, 0, CompOp.EQ, tableName);
            Record r;
            while (scan.NextRecord(out r))
            {
                relCatalog.DeleteRec(r.Rid);
            }
            scan.CloseScan();

            scan.OpenScan(attrCatalog, 32, 0, CompOp.EQ, tableName);
            while (scan.NextRecord(out r))
            {
                attrCatalog.DeleteRec(r.Rid);
            }
            scan.CloseScan();
        }
        public bool TableExist(String tableName)
        {
            scan.OpenScan(relCatalog, 32, 0, CompOp.EQ, tableName);
            Record r;
            while (scan.NextRecord(out r))
            {
                relCatalog.DeleteRec(r.Rid);
                scan.CloseScan();
                return true;
            }
            scan.CloseScan();
            return false;
        }
        AttributeCatalog[] GetAttributes(String tableName)
        {
            List<AttributeCatalog> attrs = new List<AttributeCatalog>();
            scan.OpenScan(attrCatalog, 32, 0, CompOp.EQ, tableName);
            Record r;
            while (scan.NextRecord(out r))
            {
                attrs.Add(ByteArrayToStructure<AttributeCatalog>(r.Data));
            }
            scan.CloseScan();
            return attrs.ToArray();
        }

    }
}