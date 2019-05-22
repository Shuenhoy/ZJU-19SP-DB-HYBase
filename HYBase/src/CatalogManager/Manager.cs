using System;
using HYBase.RecordManager;

namespace HYBase.CatalogManager
{
    /// <summary>
    /// 存储数据库中的表信息与表的属性信息
    /// 二者同样作为表存在
    /// </summary>
    class CatalogManager
    {
        public CatalogManager(RecordManager.RecordManager rm)
        {
            throw new NotImplementedException();
        }
        public void CreateTable(String tableName, AttributeInfo[] attributes)
        {
            throw new NotImplementedException();
        }
        public void DropTable(String tableName)
        {
            throw new NotImplementedException();
        }
        public bool TableExist(String tableName)
        {
            throw new NotImplementedException();
        }
        AttributeCatalog[] GetAttributes(String tableName)
        {
            throw new NotImplementedException();
        }

    }
}