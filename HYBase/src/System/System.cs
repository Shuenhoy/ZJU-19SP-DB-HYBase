using System;
using HYBase.CatalogManager;

namespace HYBase.System
{
    public class DataBaseSystem
    {
        CatalogManager.CatalogManager catalogManager;

        void CreateTable(String tableName, AttributeInfo[] attribute)
        {
            catalogManager.CreateTable(tableName, attribute);
        }
        void DropTable(String tableName)
        {
            catalogManager.DropTable(tableName);
        }

    }
}