using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using HYBase.RecordManager;
using static HYBase.Utils.Utils;
using System.Text;
using System.Runtime.InteropServices;

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
        public CatalogManager() { }
        public void Init(RecordManager.RecordManager rm, Stream relcat, Stream attrcat, Stream indexcat)
        {
            relCatalog = rm.CreateFile(relcat, Marshal.SizeOf<RelationCatalog>());
            attrCatalog = rm.CreateFile(attrcat, Marshal.SizeOf<AttributeCatalog>());
            indexCatalog = rm.CreateFile(indexcat, Marshal.SizeOf<IndexCatalog>());
            scan = new RecordManager.FileScan();
        }

        public int CreateIndex(String tableName, String columnName, String indexName)
        {

            Record r;

            scan.OpenScan(attrCatalog, 32, 0, AttrType.String, CompOp.EQ, Encoding.UTF8.GetBytes(tableName));

            while (scan.NextRecord(out r))
            {

                var attr = ByteArrayToStructure<AttributeCatalog>(r.Data);
                if (BytesToString(attr.attributeName) == columnName)
                {
                    var catalog = new IndexCatalog(tableName, columnName, indexName, indexCatalog.IncreaseKey());
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
        public void CreateTable(String tableName, AttributeInfo[] attributes, int recordLength)
        {
            var catalog = new RelationCatalog(tableName, attributes.Length, recordLength);
            relCatalog.InsertRec(StructureToByteArray(catalog));
            int offset = 0;
            foreach (var attr in attributes)
            {
                var attrcatalog = new AttributeCatalog(tableName, attr.AttributeName, offset, attr.type, attr.AttributeLength, -1);

                offset += attr.AttributeLength;
                attrCatalog.InsertRec(StructureToByteArray(attrcatalog));
            }
        }
        public void DropTable(String tableName)
        {
            scan.OpenScan(relCatalog, 32, 0, AttrType.String, CompOp.EQ, Encoding.UTF8.GetBytes(tableName));

            Record r;
            while (scan.NextRecord(out r))
            {
                relCatalog.DeleteRec(r.Rid);
            }
            scan.CloseScan();
            scan.OpenScan(attrCatalog, 32, 0, AttrType.String, CompOp.EQ, Encoding.UTF8.GetBytes(tableName));

            while (scan.NextRecord(out r))
            {
                attrCatalog.DeleteRec(r.Rid);
            }
            scan.CloseScan();
        }
        public void DropIndex(String tableName)
        {
            scan.OpenScan(indexCatalog, 32, 0, AttrType.String, CompOp.EQ, Encoding.UTF8.GetBytes(tableName));

            Record r;
            while (scan.NextRecord(out r))
            {
                indexCatalog.DeleteRec(r.Rid);
                scan.CloseScan();
                scan.OpenScan(attrCatalog, 32, 0, AttrType.String, CompOp.EQ, Encoding.UTF8.GetBytes(tableName));
                var ind = ByteArrayToStructure<IndexCatalog>(r.Data);
                while (scan.NextRecord(out r))
                {

                    var attr = ByteArrayToStructure<AttributeCatalog>(r.Data);
                    if (BytesToString(attr.attributeName) == ind.AttributeName)
                    {
                        attr.indexNo = -1;
                        r.Data = StructureToByteArray(attr);
                        attrCatalog.UpdateRec(r);
                        scan.CloseScan();
                        break;
                    }
                }
                return;
            }
            scan.CloseScan();
            throw new Exception("no such index!");
        }
        public bool TableExist(String tableName)
        {
            scan.OpenScan(relCatalog, 32, 0, AttrType.String, CompOp.EQ, Encoding.UTF8.GetBytes(tableName));

            Record r;
            while (scan.NextRecord(out r))
            {
                scan.CloseScan();
                return true;
            }
            scan.CloseScan();
            return false;
        }
        public RelationCatalog? GetTable(String tableName)
        {
            scan.OpenScan(relCatalog, 32, 0, AttrType.String, CompOp.EQ, Encoding.UTF8.GetBytes(tableName));

            Record r;
            while (scan.NextRecord(out r))
            {
                scan.CloseScan();
                return ByteArrayToStructure<RelationCatalog>(r.Data);
            }
            scan.CloseScan();
            return null;
        }
        public void CloseAll()
        {
            attrCatalog.Close();
            relCatalog.Close();
            indexCatalog.Close();
        }
        public bool IndexExist(String tableName, String columnName)
        {
            scan.OpenScan(attrCatalog, 32, 0, AttrType.String, CompOp.EQ, Encoding.UTF8.GetBytes(tableName));

            Record r;
            while (scan.NextRecord(out r))
            {
                var attr = ByteArrayToStructure<AttributeCatalog>(r.Data);
                if (BytesToString(attr.attributeName) == columnName)
                {
                    scan.CloseScan();
                    if (attr.indexNo < 0) return false;
                    return true;
                }
            }
            scan.CloseScan();
            throw new Exception("no such column!");
        }
        public bool IndexExist(string indexName)
        {
            Record r;
            scan.OpenScan(indexCatalog, 32, 64, AttrType.String, CompOp.EQ, Encoding.UTF8.GetBytes(indexName));

            while (scan.NextRecord(out r))
            {

                scan.CloseScan();
                return true;
            }
            return false;

        }
        public AttributeCatalog? GetColumn(string tableName, String columnName)
        {
            scan.OpenScan(attrCatalog, 32, 0, AttrType.String, CompOp.EQ, Encoding.UTF8.GetBytes(tableName));

            Record r;
            while (scan.NextRecord(out r))
            {
                var attr = ByteArrayToStructure<AttributeCatalog>(r.Data);
                if (BytesToString(attr.attributeName) == columnName)
                {
                    scan.CloseScan();
                    return attr;
                }
            }
            scan.CloseScan();
            return null;
        }
        public bool ColumnExist(String tableName, String columnName)
        {
            scan.OpenScan(attrCatalog, 32, 0, AttrType.String, CompOp.EQ, Encoding.UTF8.GetBytes(tableName));

            Record r;
            while (scan.NextRecord(out r))
            {
                var attr = ByteArrayToStructure<AttributeCatalog>(r.Data);
                if (BytesToString(attr.attributeName) == columnName)
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
            scan.OpenScan(attrCatalog, 32, 0, AttrType.String, CompOp.EQ, Encoding.UTF8.GetBytes(tableName));

            Record r;
            while (scan.NextRecord(out r))
            {
                var attr = ByteArrayToStructure<AttributeCatalog>(r.Data);
                if (BytesToString(attr.attributeName) == columnName)
                {
                    scan.CloseScan();
                    index = null;
                    if (attr.indexNo < 0) return false;
                    scan.OpenScan(indexCatalog, 4, 48, AttrType.Int, CompOp.EQ, BitConverter.GetBytes(attr.indexNo));

                    while (scan.NextRecord(out r))
                    {
                        index = ByteArrayToStructure<IndexCatalog>(r.Data);
                        scan.CloseScan();
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
            scan.OpenScan(attrCatalog, 32, 0, AttrType.String, CompOp.EQ, Encoding.UTF8.GetBytes(tableName));

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