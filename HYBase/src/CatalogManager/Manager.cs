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
        RecordFile relCatalog, attrCatalog, indexCatalog;
        FileScan scan;

        public CatalogManager(RecordManager.RecordManager rm, Stream relcat, Stream attrcat, Stream indexcat)
        {
            relCatalog = rm.OpenFile(relcat);
            attrCatalog = rm.OpenFile(attrcat);
            indexCatalog = rm.OpenFile(indexcat);
            scan = new RecordManager.FileScan();
        }
        public int CreateIndex(String tableName, String columnName, String indexName)
        {

            Record r;

            scan.OpenScan(attrCatalog, 32, 0, CompOp.EQ, tableName);

            while (scan.NextRecord(out r))
            {
                var attr = ByteArrayToStructure<AttributeCatalog>(r.Data);
                if (attr.attributeName == columnName)
                {
                    var catalog = new IndexCatalog
                    {
                        relationName = tableName,
                        attributeName = columnName,
                        indexName = indexName,
                        indexID = indexCatalog.IncreaseKey()
                    };
                    attr.indexNo = catalog.indexID;
                    r.Data = StructureToByteArray(attr);
                    attrCatalog.UpdateRec(r);
                    indexCatalog.InsertRec(StructureToByteArray(catalog));
                    scan.CloseScan();
                    return catalog.indexID;

                }
            }
            scan.CloseScan();
            throw new Exception("no such index");
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
        public void DropIndex(String tableName)
        {
            scan.OpenScan(indexCatalog, 32, 0, CompOp.EQ, tableName);
            Record r;
            while (scan.NextRecord(out r))
            {
                indexCatalog.DeleteRec(r.Rid);
                return;
            }
            scan.CloseScan();
            throw new Exception("no such index!");
        }
        public bool TableExist(String tableName)
        {
            scan.OpenScan(relCatalog, 32, 0, CompOp.EQ, tableName);
            Record r;
            while (scan.NextRecord(out r))
            {
                scan.CloseScan();
                return true;
            }
            scan.CloseScan();
            return false;
        }
        public bool ColumnExist(String tableName, String columnName)
        {
            scan.OpenScan(attrCatalog, 32, 0, CompOp.EQ, tableName);
            Record r;
            while (scan.NextRecord(out r))
            {
                var attr = ByteArrayToStructure<AttributeCatalog>(r.Data);
                if (attr.attributeName == columnName)
                {
                    scan.CloseScan();
                    return true;
                }
            }
            scan.CloseScan();
            return false;
        }
        public bool GetIndex(String tableName, String columnName, out IndexCatalog? index)
        {
            scan.OpenScan(attrCatalog, 32, 0, CompOp.EQ, tableName);
            Record r;
            while (scan.NextRecord(out r))
            {
                var attr = ByteArrayToStructure<AttributeCatalog>(r.Data);
                if (attr.attributeName == columnName)
                {
                    scan.CloseScan();
                    index = null;
                    if (attr.indexNo < 0) return false;
                    scan.OpenScan(indexCatalog, 4, 48, CompOp.EQ, attr.indexNo);
                    while (scan.NextRecord(out r))
                    {
                        index = ByteArrayToStructure<IndexCatalog>(r.Data);
                        return true;
                    }
                    return true;
                }
            }
            scan.CloseScan();
            throw new Exception("no such column!");
        }
        public AttributeCatalog[] GetAttributes(String tableName)
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